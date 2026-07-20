/**
 * @file HAL_NPK_SENSOR.c
 * @brief Modbus RTU implementation for the 7-in-1 NPK soil sensor.
 */

#include "HAL_NPK_SENSOR.h"

#define HAL_NPK_SENSOR_FUNC_READ_HOLDING_REGS     (0x03U)
#define HAL_NPK_SENSOR_START_REGISTER             (0x0000U)
#define HAL_NPK_SENSOR_REGISTER_COUNT             (7U)
#define HAL_NPK_SENSOR_REQUEST_SIZE               (8U)
#define HAL_NPK_SENSOR_RESPONSE_SIZE              (19U)
#define HAL_NPK_SENSOR_RESPONSE_DATA_BYTES        (14U)
#define HAL_NPK_SENSOR_TURNAROUND_DELAY_MS        (2U)

/* The sensor uses Modbus RTU CRC16 with polynomial 0xA001 and low byte first. */
static void HAL_NPK_SENSOR_DefaultDelay(uint32 milliseconds);
static uint16 HAL_NPK_SENSOR_Crc16(const uint8 *data, uint16 length);
static uint16 HAL_NPK_SENSOR_ReadU16BE(const uint8 *data);
static void HAL_NPK_SENSOR_ClearRx(hal_npk_sensor_t *sensor);
static void HAL_NPK_SENSOR_SetReceiveMode(hal_npk_sensor_t *sensor);
static void HAL_NPK_SENSOR_SetTransmitMode(hal_npk_sensor_t *sensor);

/**
 * @brief Fallback blocking delay when the application does not provide one.
 */
static void HAL_NPK_SENSOR_DefaultDelay(uint32 milliseconds)
{
    volatile uint32 cycles;

    while (milliseconds > 0U)
    {
        cycles = 4000UL;
        while (cycles > 0U)
        {
            __asm volatile ("nop");
            cycles--;
        }

        milliseconds--;
    }
}

/**
 * @brief Calculate Modbus RTU CRC16.
 */
static uint16 HAL_NPK_SENSOR_Crc16(const uint8 *data, uint16 length)
{
    uint16 crc = 0xFFFFU;
    uint16 index;
    uint8 bit;

    for (index = 0U; index < length; index++)
    {
        crc ^= data[index];

        for (bit = 0U; bit < 8U; bit++)
        {
            if ((crc & 0x0001U) != 0U)
            {
                crc >>= 1U;
                crc ^= 0xA001U;
            }
            else
            {
                crc >>= 1U;
            }
        }
    }

    return crc;
}

/**
 * @brief Read a big-endian Modbus register value.
 */
static uint16 HAL_NPK_SENSOR_ReadU16BE(const uint8 *data)
{
    return (uint16)(((uint16)data[0] << 8U) | data[1]);
}

/**
 * @brief Clear stale RX bytes and USART error flags before a new request.
 */
static void HAL_NPK_SENSOR_ClearRx(hal_npk_sensor_t *sensor)
{
    volatile uint32 dummy;

    if ((sensor == NULL) || (sensor->uart == NULL))
    {
        return;
    }

    while (MCAL_USART_GetFlagStatus(sensor->uart, MCAL_USART_FLAG_RXNE) != 0U)
    {
        dummy = sensor->uart->DR;
        (void)dummy;
    }

    MCAL_USART_ClearFlag(sensor->uart, MCAL_USART_FLAG_ORE);
    MCAL_USART_ClearFlag(sensor->uart, MCAL_USART_FLAG_FE);
    MCAL_USART_ClearFlag(sensor->uart, MCAL_USART_FLAG_NF);
}

/**
 * @brief Put MAX485 in receive mode.
 */
static void HAL_NPK_SENSOR_SetReceiveMode(hal_npk_sensor_t *sensor)
{
    MCAL_GPIO_WritePin(sensor->de_re_port, sensor->de_re_pin, GPIO_PIN_RESET);
}

/**
 * @brief Put MAX485 in transmit mode.
 */
static void HAL_NPK_SENSOR_SetTransmitMode(hal_npk_sensor_t *sensor)
{
    MCAL_GPIO_WritePin(sensor->de_re_port, sensor->de_re_pin, GPIO_PIN_SET);
}

hal_npk_sensor_status_t HAL_NPK_SENSOR_Init(hal_npk_sensor_t *sensor,
                                            const hal_npk_sensor_config_t *config)
{
    if ((sensor == NULL) || (config == NULL) || (config->uart == NULL) ||
        (config->de_re_port == NULL) || (config->de_re_pin > MCAL_GPIO_PIN_15))
    {
        return HAL_NPK_SENSOR_STATUS_INVALID_PARAM;
    }

    sensor->uart = config->uart;
    sensor->de_re_port = config->de_re_port;
    sensor->de_re_pin = config->de_re_pin;
    sensor->device_address = config->device_address;
    sensor->delay_ms = config->delay_ms;

    if (sensor->device_address == 0U)
    {
        sensor->device_address = HAL_NPK_SENSOR_DEFAULT_ADDRESS;
    }

    if (sensor->delay_ms == NULL)
    {
        sensor->delay_ms = HAL_NPK_SENSOR_DefaultDelay;
    }

    MCAL_GPIO_Init(sensor->de_re_port,
                   sensor->de_re_pin,
                   MCAL_GPIO_MODE_OUTPUT_PP,
                   MCAL_GPIO_SPEED_LOW);
    HAL_NPK_SENSOR_SetReceiveMode(sensor);

    sensor->is_initialized = 1U;

    return HAL_NPK_SENSOR_STATUS_OK;
}

hal_npk_sensor_status_t HAL_NPK_SENSOR_ReadAll(hal_npk_sensor_t *sensor,
                                               hal_npk_sensor_data_t *data)
{
    uint8 request[HAL_NPK_SENSOR_REQUEST_SIZE];
    uint8 response[HAL_NPK_SENSOR_RESPONSE_SIZE];
    uint16 crc;
    mcal_usart_status_t usart_status;

    if ((sensor == NULL) || (data == NULL) || (sensor->is_initialized == 0U))
    {
        return HAL_NPK_SENSOR_STATUS_INVALID_PARAM;
    }

    request[0] = sensor->device_address;
    request[1] = HAL_NPK_SENSOR_FUNC_READ_HOLDING_REGS;
    request[2] = (uint8)((HAL_NPK_SENSOR_START_REGISTER >> 8U) & 0xFFU);
    request[3] = (uint8)(HAL_NPK_SENSOR_START_REGISTER & 0xFFU);
    request[4] = 0U;
    request[5] = HAL_NPK_SENSOR_REGISTER_COUNT;
    crc = HAL_NPK_SENSOR_Crc16(request, 6U);
    request[6] = (uint8)(crc & 0xFFU);
    request[7] = (uint8)((crc >> 8U) & 0xFFU);

    HAL_NPK_SENSOR_ClearRx(sensor);
    HAL_NPK_SENSOR_SetTransmitMode(sensor);
    sensor->delay_ms(HAL_NPK_SENSOR_TURNAROUND_DELAY_MS);

    usart_status = MCAL_USART_SendData(sensor->uart, request, HAL_NPK_SENSOR_REQUEST_SIZE);
    if (usart_status != MCAL_USART_STATUS_OK)
    {
        HAL_NPK_SENSOR_SetReceiveMode(sensor);
        return HAL_NPK_SENSOR_STATUS_ERROR;
    }

    sensor->delay_ms(HAL_NPK_SENSOR_TURNAROUND_DELAY_MS);
    HAL_NPK_SENSOR_SetReceiveMode(sensor);

    usart_status = MCAL_USART_ReceiveData(sensor->uart, response, HAL_NPK_SENSOR_RESPONSE_SIZE);
    if (usart_status == MCAL_USART_STATUS_TIMEOUT)
    {
        return HAL_NPK_SENSOR_STATUS_TIMEOUT;
    }

    if (usart_status != MCAL_USART_STATUS_OK)
    {
        return HAL_NPK_SENSOR_STATUS_ERROR;
    }

    crc = HAL_NPK_SENSOR_Crc16(response, (uint16)(HAL_NPK_SENSOR_RESPONSE_SIZE - 2U));
    if ((response[HAL_NPK_SENSOR_RESPONSE_SIZE - 2U] != (uint8)(crc & 0xFFU)) ||
        (response[HAL_NPK_SENSOR_RESPONSE_SIZE - 1U] != (uint8)((crc >> 8U) & 0xFFU)))
    {
        return HAL_NPK_SENSOR_STATUS_CRC_ERROR;
    }

    if ((response[0] != sensor->device_address) ||
        (response[1] != HAL_NPK_SENSOR_FUNC_READ_HOLDING_REGS) ||
        (response[2] != HAL_NPK_SENSOR_RESPONSE_DATA_BYTES))
    {
        return HAL_NPK_SENSOR_STATUS_ERROR;
    }

    data->moisture_x10 = HAL_NPK_SENSOR_ReadU16BE(&response[3]);
    data->temperature_x10 = (sint16)HAL_NPK_SENSOR_ReadU16BE(&response[5]);
    data->conductivity_us_cm = HAL_NPK_SENSOR_ReadU16BE(&response[7]);
    data->ph_x10 = HAL_NPK_SENSOR_ReadU16BE(&response[9]);
    data->nitrogen_mg_kg = HAL_NPK_SENSOR_ReadU16BE(&response[11]);
    data->phosphorus_mg_kg = HAL_NPK_SENSOR_ReadU16BE(&response[13]);
    data->potassium_mg_kg = HAL_NPK_SENSOR_ReadU16BE(&response[15]);

    return HAL_NPK_SENSOR_STATUS_OK;
}
