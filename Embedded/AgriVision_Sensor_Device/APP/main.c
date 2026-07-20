/**
 * @file main.c
 * @brief AgriVision sensor-device application.
 * @author Ahmed Basem
 *
 * This firmware runs on the STM32F401RC Black Pill sensor node. It reads the
 * 7-in-1 NPK soil sensor through a MAX485 RS485 transceiver, prints the latest
 * values on a 20x4 character LCD, and sends a compact 32-byte telemetry payload
 * to the gateway over NRF24L01.
 *
 * Hardware mapping:
 * - NRF24L01 SPI1: SCK PA5, MISO PA6, MOSI PA7, CSN PA4, CE PA0, IRQ PB0
 * - LCD 20x4 4-bit: D4 PB7, D5 PB6, D6 PB5, D7 PB4, E PB8, RS PB9
 * - MAX485/NPK: DI PA2 USART2_TX, RO PA3 USART2_RX, DE/RE PA1
 */

#include "app.h"

#include "FreeRTOS.h"
#include "HAL_LCD.h"
#include "HAL_NPK_SENSOR.h"
#include "HAL_NRF24L01.h"
#include "MCAL_RCC.h"
#include "MCAL_SPI.h"
#include "MCAL_USART.h"
#include "app_protocol.h"
#include "mcal_gpio.h"
#include "task.h"

#define APP_CLOCK_HZ                 (16000000UL)
#define APP_NRF24_CHANNEL            (76U)
#define APP_NRF24_PAYLOAD_WIDTH      (APP_PROTOCOL_PAYLOAD_SIZE)
#define APP_NPK_TASK_PERIOD_MS       (2000U)
#define APP_LCD_TASK_PERIOD_MS       (500U)
#define APP_RADIO_TASK_PERIOD_MS     (2000U)
#define APP_BOOT_DELAY_MS            (100U)
#define APP_DELAY_LOOPS_PER_MS       (4000UL)

/* GPIO alternate-function setup is kept local because the current GPIO MCAL
 * exposes pin mode but not AF selection as a public helper.
 */
static void APP_GPIO_SetAlternateFunction(GPIO_RegDef_t *gpio_port,
                                          mcal_gpio_pin_t pin,
                                          uint8 alternate_function);
static void APP_SPI1_Init(void);
static void APP_USART2_Init(void);
static void APP_StatusLed_Init(void);
static void APP_StatusLed_Toggle(void);
static uint8 APP_InitPeripherals(void);
static void APP_LCD_WriteLine(uint8 row, const char *text);
static void APP_LineReset(char *line);
static void APP_LineAppendChar(char *line, uint8 *position, char ch);
static void APP_LineAppendText(char *line, uint8 *position, const char *text);
static void APP_LineAppendUInt(char *line, uint8 *position, uint32 value);
static void APP_LineAppendUIntX10(char *line, uint8 *position, uint16 value_x10);
static void APP_LineAppendSIntX10(char *line, uint8 *position, sint16 value_x10);
static const char *APP_NpkStatusText(hal_npk_sensor_status_t status);
static void APP_BuildPayload(uint8 *payload, uint8 status);
static uint16 APP_TemperatureForPayload(sint16 temperature_x10);
static void APP_SensorTask(void *argument);
static void APP_LcdTask(void *argument);
static void APP_RadioTask(void *argument);
static void APP_ErrorTask(void *argument);
static void APP_DelayMs(uint32 milliseconds);

/* HD44780-compatible LCD configuration for 20 columns x 4 rows in 4-bit mode. */
static hal_lcd_t g_app_lcd = {
    .interface_mode = HAL_LCD_INTERFACE_4BIT,
    .lines_mode = HAL_LCD_LINES_2,
    .font_mode = HAL_LCD_FONT_5X8,
    .gpio_speed = MCAL_GPIO_SPEED_LOW,
    .rows = 4U,
    .columns = 20U,
    .rs_pin = { GPIOB, MCAL_GPIO_PIN_9 },
    .en_pin = { GPIOB, MCAL_GPIO_PIN_8 },
    .rw_pin_used = 0U,
    .rw_pin = { NULL, MCAL_GPIO_PIN_0 },
    .data_pins = {
        { NULL, MCAL_GPIO_PIN_0 },
        { NULL, MCAL_GPIO_PIN_0 },
        { NULL, MCAL_GPIO_PIN_0 },
        { NULL, MCAL_GPIO_PIN_0 },
        { GPIOB, MCAL_GPIO_PIN_7 },
        { GPIOB, MCAL_GPIO_PIN_6 },
        { GPIOB, MCAL_GPIO_PIN_5 },
        { GPIOB, MCAL_GPIO_PIN_4 }
    },
    .backlight_pin_used = 0U,
    .backlight_pin = { NULL, MCAL_GPIO_PIN_0 },
    .display_control = 0U,
    .entry_mode = 0U,
    .function_set = 0U
};

static hal_nrf24_t g_app_nrf24;
static const uint8 g_app_nrf24_address[5] = { 'A', 'G', 'R', 'I', '1' };

/* Gateway must use the same address, channel, and payload width. */
static const hal_nrf24_config_t g_app_nrf24_config = {
    .spi_port = SPI1,
    .ce_pin = { GPIOA, MCAL_GPIO_PIN_0 },
    .csn_pin = { GPIOA, MCAL_GPIO_PIN_4 },
    .irq_pin = { GPIOB, MCAL_GPIO_PIN_0 },
    .channel = APP_NRF24_CHANNEL,
    .payload_width = APP_NRF24_PAYLOAD_WIDTH,
    .address_width = 5U,
    .power_level = 0U,
    .data_rate = 0U
};

static hal_npk_sensor_t g_app_npk_sensor;
static const hal_npk_sensor_config_t g_app_npk_config = {
    .uart = USART2,
    .de_re_port = GPIOA,
    .de_re_pin = MCAL_GPIO_PIN_1,
    .device_address = HAL_NPK_SENSOR_DEFAULT_ADDRESS,
    .delay_ms = APP_DelayMs
};

static volatile uint8 g_app_peripherals_ok = 0U;
static volatile uint8 g_app_sensor_valid = 0U;
static volatile uint16 g_app_tx_sequence = 0U;
static hal_npk_sensor_data_t g_app_sensor_data = { 0U, 0, 0U, 0U, 0U, 0U, 0U };
static hal_npk_sensor_status_t g_app_last_sensor_status = HAL_NPK_SENSOR_STATUS_ERROR;

/**
 * @brief Blocking millisecond delay used by peripheral drivers during init and
 *        RS485 direction turnaround.
 */
static void APP_DelayMs(uint32 milliseconds)
{
    volatile uint32 cycles;

    while (milliseconds > 0U)
    {
        cycles = APP_DELAY_LOOPS_PER_MS;
        while (cycles > 0U)
        {
            __asm volatile ("nop");
            cycles--;
        }

        milliseconds--;
    }
}

/**
 * @brief Configure a GPIO pin alternate-function value.
 */
static void APP_GPIO_SetAlternateFunction(GPIO_RegDef_t *gpio_port,
                                          mcal_gpio_pin_t pin,
                                          uint8 alternate_function)
{
    uint32 shift;

    if ((gpio_port == NULL) || (pin > MCAL_GPIO_PIN_15) || (alternate_function > 15U))
    {
        return;
    }

    if (pin < MCAL_GPIO_PIN_8)
    {
        shift = (uint32)pin * 4U;
        gpio_port->AFRL &= ~(0xFUL << shift);
        gpio_port->AFRL |= ((uint32)alternate_function << shift);
    }
    else
    {
        shift = ((uint32)pin - 8U) * 4U;
        gpio_port->AFRH &= ~(0xFUL << shift);
        gpio_port->AFRH |= ((uint32)alternate_function << shift);
    }
}

/**
 * @brief Configure SPI1 pins and SPI peripheral for NRF24L01 communication.
 */
static void APP_SPI1_Init(void)
{
    const mcal_spi_config_t spi_config = {
        .device_mode = MCAL_SPI_DEVICE_MASTER,
        .bus_config = MCAL_SPI_BUS_FULL_DUPLEX,
        .data_frame = MCAL_SPI_DFF_8BIT,
        .clock_polarity = MCAL_SPI_CPOL_LOW,
        .clock_phase = MCAL_SPI_CPHA_FIRST_EDGE,
        .baud_prescaler = MCAL_SPI_BAUD_DIV16,
        .first_bit = MCAL_SPI_FIRSTBIT_MSB,
        .software_slave_management = MCAL_SPI_SSM_ENABLE,
        .nss_mode = MCAL_SPI_NSS_SOFTWARE_INTERNAL,
        .crc_calculation = MCAL_SPI_CRC_DISABLE,
        .ti_mode = MCAL_SPI_TI_MODE_DISABLE,
        .dma_rx = MCAL_SPI_DMA_RX_DISABLE,
        .dma_tx = MCAL_SPI_DMA_TX_DISABLE
    };

    MCAL_GPIO_Init(GPIOA, MCAL_GPIO_PIN_5, MCAL_GPIO_MODE_AF_PP, MCAL_GPIO_SPEED_VERY_HIGH);
    MCAL_GPIO_Init(GPIOA, MCAL_GPIO_PIN_6, MCAL_GPIO_MODE_AF_PP_PULLUP, MCAL_GPIO_SPEED_VERY_HIGH);
    MCAL_GPIO_Init(GPIOA, MCAL_GPIO_PIN_7, MCAL_GPIO_MODE_AF_PP, MCAL_GPIO_SPEED_VERY_HIGH);

    APP_GPIO_SetAlternateFunction(GPIOA, MCAL_GPIO_PIN_5, 5U);
    APP_GPIO_SetAlternateFunction(GPIOA, MCAL_GPIO_PIN_6, 5U);
    APP_GPIO_SetAlternateFunction(GPIOA, MCAL_GPIO_PIN_7, 5U);

    (void)MCAL_SPI_Init(SPI1, &spi_config);
    MCAL_SPI_Enable(SPI1);
}

/**
 * @brief Configure USART2 pins and UART peripheral for the Modbus NPK sensor.
 */
static void APP_USART2_Init(void)
{
    const mcal_usart_config_t usart_config = {
        .mode = MCAL_USART_MODE_TX_RX,
        .baud_rate = HAL_NPK_SENSOR_DEFAULT_BAUDRATE,
        .pclk_hz = APP_CLOCK_HZ,
        .word_length = MCAL_USART_WORDLENGTH_8BIT,
        .stop_bits = MCAL_USART_STOPBITS_1,
        .parity = MCAL_USART_PARITY_NONE,
        .hw_flow_control = MCAL_USART_HWFLOW_NONE,
        .oversampling = MCAL_USART_OVERSAMPLING_16,
        .sync_clock = MCAL_USART_SYNC_CLOCK_DISABLE,
        .clock_polarity = MCAL_USART_CLOCK_POLARITY_LOW,
        .clock_phase = MCAL_USART_CLOCK_PHASE_1EDGE,
        .last_bit_clock = MCAL_USART_LASTBITCLOCK_DISABLE,
        .lin_mode = MCAL_USART_LIN_DISABLE,
        .half_duplex = MCAL_USART_HALFDUPLEX_DISABLE,
        .irda_mode = MCAL_USART_IRDA_DISABLE,
        .irda_power = MCAL_USART_IRDA_NORMAL,
        .smartcard_mode = MCAL_USART_SMARTCARD_DISABLE,
        .dma_rx = MCAL_USART_DMA_RX_DISABLE,
        .dma_tx = MCAL_USART_DMA_TX_DISABLE,
        .one_bit_sampling = MCAL_USART_ONEBIT_DISABLE
    };

    MCAL_GPIO_Init(GPIOA, MCAL_GPIO_PIN_2, MCAL_GPIO_MODE_AF_PP, MCAL_GPIO_SPEED_VERY_HIGH);
    MCAL_GPIO_Init(GPIOA, MCAL_GPIO_PIN_3, MCAL_GPIO_MODE_AF_PP_PULLUP, MCAL_GPIO_SPEED_VERY_HIGH);
    APP_GPIO_SetAlternateFunction(GPIOA, MCAL_GPIO_PIN_2, 7U);
    APP_GPIO_SetAlternateFunction(GPIOA, MCAL_GPIO_PIN_3, 7U);

    (void)MCAL_USART_Init(USART2, &usart_config);
}

/**
 * @brief Configure the onboard status LED on PC13.
 */
static void APP_StatusLed_Init(void)
{
    MCAL_GPIO_Init(GPIOC, MCAL_GPIO_PIN_13, MCAL_GPIO_MODE_OUTPUT_PP, MCAL_GPIO_SPEED_LOW);
    MCAL_GPIO_WritePin(GPIOC, MCAL_GPIO_PIN_13, GPIO_PIN_SET);
}

/**
 * @brief Toggle the onboard status LED.
 */
static void APP_StatusLed_Toggle(void)
{
    MCAL_GPIO_TogglePin(GPIOC, MCAL_GPIO_PIN_13);
}

/**
 * @brief Initialize clocks, GPIO, LCD, NPK sensor driver, and NRF24L01.
 * @return 1 when all critical peripherals initialize correctly, otherwise 0.
 */
static uint8 APP_InitPeripherals(void)
{
    mcal_rcc_clock_config_t clock_config;

    (void)MCAL_RCC_GetDefaultClockConfig(&clock_config);
    (void)MCAL_RCC_InitClock(&clock_config);

    MCAL_RCC_EnableGPIOClock(MCAL_RCC_GPIO_PORT_A);
    MCAL_RCC_EnableGPIOClock(MCAL_RCC_GPIO_PORT_B);
    MCAL_RCC_EnableGPIOClock(MCAL_RCC_GPIO_PORT_C);
    MCAL_RCC_EnableAPB2PeripheralClock(MCAL_RCC_APB2_SPI1);
    MCAL_RCC_EnableAPB1PeripheralClock(MCAL_RCC_APB1_USART2);

    APP_StatusLed_Init();
    APP_SPI1_Init();
    APP_USART2_Init();

    if (HAL_LCD_Init(&g_app_lcd) != HAL_LCD_STATUS_OK)
    {
        return 0U;
    }

    APP_LCD_WriteLine(0U, "AgriVision Sensor");
    APP_LCD_WriteLine(1U, "Init devices...");
    APP_LCD_WriteLine(2U, "");
    APP_LCD_WriteLine(3U, "");

    if (HAL_NPK_SENSOR_Init(&g_app_npk_sensor, &g_app_npk_config) != HAL_NPK_SENSOR_STATUS_OK)
    {
        return 0U;
    }

    if (HAL_NRF24_Init(&g_app_nrf24, &g_app_nrf24_config) != HAL_NRF24_STATUS_OK)
    {
        return 0U;
    }

    (void)HAL_NRF24_WriteReg(&g_app_nrf24, NRF24_REG_EN_AA, 0x01U);
    (void)HAL_NRF24_WriteReg(&g_app_nrf24, NRF24_REG_SETUP_RETR, 0x2FU);
    (void)HAL_NRF24_WriteReg(&g_app_nrf24, NRF24_REG_RF_SETUP, 0x06U);
    (void)HAL_NRF24_WriteRegBytes(&g_app_nrf24, NRF24_REG_TX_ADDR, g_app_nrf24_address, 5U);
    (void)HAL_NRF24_WriteRegBytes(&g_app_nrf24, NRF24_REG_RX_ADDR_P0, g_app_nrf24_address, 5U);
    (void)HAL_NRF24_WriteReg(&g_app_nrf24,
                             NRF24_REG_CONFIG,
                             (uint8)(NRF24_CONFIG_EN_CRC | NRF24_CONFIG_CRCO));
    (void)HAL_NRF24_ClearIRQFlags(&g_app_nrf24);
    (void)HAL_NRF24_PowerUp(&g_app_nrf24);
    (void)HAL_NRF24_SetTXMode(&g_app_nrf24);
    APP_DelayMs(APP_BOOT_DELAY_MS);

    return 1U;
}

/**
 * @brief Write exactly one padded 20-character line to the LCD.
 */
static void APP_LCD_WriteLine(uint8 row, const char *text)
{
    char line[21];
    uint8 index;

    for (index = 0U; index < 20U; index++)
    {
        if ((text != NULL) && (text[index] != '\0'))
        {
            line[index] = text[index];
        }
        else
        {
            line[index] = ' ';
        }
    }

    line[20] = '\0';
    (void)HAL_LCD_WriteStringAt(&g_app_lcd, row, 0U, (const uint8 *)line);
}

/**
 * @brief Reset a temporary LCD line buffer to an empty C string.
 */
static void APP_LineReset(char *line)
{
    if (line != NULL)
    {
        line[0] = '\0';
    }
}

/**
 * @brief Append one character to a 20-character LCD line buffer.
 */
static void APP_LineAppendChar(char *line, uint8 *position, char ch)
{
    if ((line == NULL) || (position == NULL) || (*position >= 20U))
    {
        return;
    }

    line[*position] = ch;
    (*position)++;
    line[*position] = '\0';
}

/**
 * @brief Append a null-terminated string to a 20-character LCD line buffer.
 */
static void APP_LineAppendText(char *line, uint8 *position, const char *text)
{
    uint8 index = 0U;

    if ((line == NULL) || (position == NULL) || (text == NULL))
    {
        return;
    }

    while ((text[index] != '\0') && (*position < 20U))
    {
        APP_LineAppendChar(line, position, text[index]);
        index++;
    }
}

/**
 * @brief Append an unsigned integer without using stdio.
 */
static void APP_LineAppendUInt(char *line, uint8 *position, uint32 value)
{
    char reversed[10];
    uint8 count = 0U;

    if ((line == NULL) || (position == NULL))
    {
        return;
    }

    if (value == 0U)
    {
        APP_LineAppendChar(line, position, '0');
        return;
    }

    while ((value > 0U) && (count < 10U))
    {
        reversed[count] = (char)('0' + (value % 10U));
        value /= 10U;
        count++;
    }

    while (count > 0U)
    {
        count--;
        APP_LineAppendChar(line, position, reversed[count]);
    }
}

/**
 * @brief Append a fixed-point unsigned value stored as value * 10.
 */
static void APP_LineAppendUIntX10(char *line, uint8 *position, uint16 value_x10)
{
    APP_LineAppendUInt(line, position, (uint32)(value_x10 / 10U));
    APP_LineAppendChar(line, position, '.');
    APP_LineAppendUInt(line, position, (uint32)(value_x10 % 10U));
}

/**
 * @brief Append a fixed-point signed value stored as value * 10.
 */
static void APP_LineAppendSIntX10(char *line, uint8 *position, sint16 value_x10)
{
    uint16 magnitude;

    if (value_x10 < 0)
    {
        APP_LineAppendChar(line, position, '-');
        magnitude = (uint16)(-value_x10);
    }
    else
    {
        magnitude = (uint16)value_x10;
    }

    APP_LineAppendUIntX10(line, position, magnitude);
}

/**
 * @brief Convert NPK driver status to short LCD text.
 */
static const char *APP_NpkStatusText(hal_npk_sensor_status_t status)
{
    switch (status)
    {
        case HAL_NPK_SENSOR_STATUS_OK:
            return "OK";

        case HAL_NPK_SENSOR_STATUS_ERROR:
            return "ERROR";

        case HAL_NPK_SENSOR_STATUS_INVALID_PARAM:
            return "BAD PARAM";

        case HAL_NPK_SENSOR_STATUS_CRC_ERROR:
            return "CRC ERROR";

        case HAL_NPK_SENSOR_STATUS_TIMEOUT:
            return "TIMEOUT";

        default:
            return "UNKNOWN";
    }
}

/**
 * @brief Convert signed sensor temperature to the unsigned two-byte wire value.
 */
static uint16 APP_TemperatureForPayload(sint16 temperature_x10)
{
    return (uint16)temperature_x10;
}

/**
 * @brief Build the fixed 32-byte NRF24L01 payload for the gateway.
 */
static void APP_BuildPayload(uint8 *payload, uint8 status)
{
    uint8 index;

    if (payload == NULL)
    {
        return;
    }

    for (index = 0U; index < APP_PROTOCOL_PAYLOAD_SIZE; index++)
    {
        payload[index] = 0U;
    }

    payload[APP_PROTOCOL_IDX_MAGIC0] = APP_PROTOCOL_MAGIC_0;
    payload[APP_PROTOCOL_IDX_MAGIC1] = APP_PROTOCOL_MAGIC_1;
    payload[APP_PROTOCOL_IDX_VERSION] = APP_PROTOCOL_VERSION;
    payload[APP_PROTOCOL_IDX_STATUS] = status;
    APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_SEQUENCE_L, g_app_tx_sequence);

    if (status == APP_PROTOCOL_STATUS_OK)
    {
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_MOISTURE_L, g_app_sensor_data.moisture_x10);
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_TEMPERATURE_L,
                              APP_TemperatureForPayload(g_app_sensor_data.temperature_x10));
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_PH_L, g_app_sensor_data.ph_x10);
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_EC_L, g_app_sensor_data.conductivity_us_cm);
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_N_L, g_app_sensor_data.nitrogen_mg_kg);
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_P_L, g_app_sensor_data.phosphorus_mg_kg);
        APP_PROTOCOL_WriteU16(payload, APP_PROTOCOL_IDX_K_L, g_app_sensor_data.potassium_mg_kg);
    }
}

/**
 * @brief Periodic task that polls the RS485 NPK sensor.
 */
static void APP_SensorTask(void *argument)
{
    hal_npk_sensor_data_t data;
    hal_npk_sensor_status_t status;

    (void)argument;

    if (g_app_peripherals_ok == 0U)
    {
        vTaskDelay(pdMS_TO_TICKS(APP_NPK_TASK_PERIOD_MS));
        return;
    }

    status = HAL_NPK_SENSOR_ReadAll(&g_app_npk_sensor, &data);
    g_app_last_sensor_status = status;

    if (status == HAL_NPK_SENSOR_STATUS_OK)
    {
        g_app_sensor_data = data;
        g_app_sensor_valid = 1U;
    }
    else
    {
        g_app_sensor_valid = 0U;
    }

    vTaskDelay(pdMS_TO_TICKS(APP_NPK_TASK_PERIOD_MS));
}

/**
 * @brief Periodic task that refreshes the LCD with the latest sensor state.
 */
static void APP_LcdTask(void *argument)
{
    char line[21];
    uint8 position;

    (void)argument;

    if (g_app_peripherals_ok == 0U)
    {
        vTaskDelay(pdMS_TO_TICKS(APP_LCD_TASK_PERIOD_MS));
        return;
    }

    APP_LCD_WriteLine(0U, "AgriVision Sensor");

    if (g_app_sensor_valid != 0U)
    {
        position = 0U;
        APP_LineReset(line);
        APP_LineAppendText(line, &position, "M:");
        APP_LineAppendUIntX10(line, &position, g_app_sensor_data.moisture_x10);
        APP_LineAppendText(line, &position, "% T:");
        APP_LineAppendSIntX10(line, &position, g_app_sensor_data.temperature_x10);
        APP_LineAppendChar(line, &position, 'C');
        APP_LCD_WriteLine(1U, line);

        position = 0U;
        APP_LineReset(line);
        APP_LineAppendText(line, &position, "pH:");
        APP_LineAppendUIntX10(line, &position, g_app_sensor_data.ph_x10);
        APP_LineAppendText(line, &position, " EC:");
        APP_LineAppendUInt(line, &position, g_app_sensor_data.conductivity_us_cm);
        APP_LCD_WriteLine(2U, line);

        position = 0U;
        APP_LineReset(line);
        APP_LineAppendText(line, &position, "N:");
        APP_LineAppendUInt(line, &position, g_app_sensor_data.nitrogen_mg_kg);
        APP_LineAppendText(line, &position, " P:");
        APP_LineAppendUInt(line, &position, g_app_sensor_data.phosphorus_mg_kg);
        APP_LineAppendText(line, &position, " K:");
        APP_LineAppendUInt(line, &position, g_app_sensor_data.potassium_mg_kg);
        APP_LCD_WriteLine(3U, line);
    }
    else
    {
        APP_LCD_WriteLine(1U, "Waiting NPK data");
        position = 0U;
        APP_LineReset(line);
        APP_LineAppendText(line, &position, "NPK: ");
        APP_LineAppendText(line, &position, APP_NpkStatusText(g_app_last_sensor_status));
        APP_LCD_WriteLine(2U, line);

        position = 0U;
        APP_LineReset(line);
        APP_LineAppendText(line, &position, "ID:1 Baud:");
        APP_LineAppendUInt(line, &position, HAL_NPK_SENSOR_DEFAULT_BAUDRATE);
        APP_LCD_WriteLine(3U, line);
    }

    vTaskDelay(pdMS_TO_TICKS(APP_LCD_TASK_PERIOD_MS));
}

/**
 * @brief Periodic task that sends the latest sample to the NRF24L01 gateway.
 */
static void APP_RadioTask(void *argument)
{
    uint8 payload[APP_PROTOCOL_PAYLOAD_SIZE];
    uint8 status;

    (void)argument;

    if (g_app_peripherals_ok == 0U)
    {
        vTaskDelay(pdMS_TO_TICKS(APP_RADIO_TASK_PERIOD_MS));
        return;
    }

    status = (g_app_sensor_valid != 0U) ? APP_PROTOCOL_STATUS_OK : APP_PROTOCOL_STATUS_SENSOR_ERR;
    APP_BuildPayload(payload, status);
    (void)HAL_NRF24_TransmitPayload(&g_app_nrf24, payload, APP_PROTOCOL_PAYLOAD_SIZE);
    (void)HAL_NRF24_ClearIRQFlags(&g_app_nrf24);
    g_app_tx_sequence++;
    APP_StatusLed_Toggle();

    vTaskDelay(pdMS_TO_TICKS(APP_RADIO_TASK_PERIOD_MS));
}

/**
 * @brief Error blink task used when startup initialization fails.
 */
static void APP_ErrorTask(void *argument)
{
    (void)argument;

    if (g_app_peripherals_ok == 0U)
    {
        APP_StatusLed_Toggle();
    }

    vTaskDelay(pdMS_TO_TICKS(250U));
}

/**
 * @brief Application entry point called by main().
 */
void APP_Run(void)
{
    g_app_peripherals_ok = APP_InitPeripherals();

    (void)xTaskCreate(APP_SensorTask, "npk", 256U, NULL, 3U, NULL);
    (void)xTaskCreate(APP_LcdTask, "lcd", 256U, NULL, 2U, NULL);
    (void)xTaskCreate(APP_RadioTask, "nrf", 256U, NULL, 2U, NULL);
    (void)xTaskCreate(APP_ErrorTask, "err", 128U, NULL, 1U, NULL);

    vTaskStartScheduler();
}

int main(void)
{
    APP_Run();
    return 0;
}
