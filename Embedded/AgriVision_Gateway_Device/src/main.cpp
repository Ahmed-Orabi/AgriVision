/**
 * @file main.cpp
 * @brief AgriVision ESP32 gateway firmware.
 *
 * The gateway receives 32-byte telemetry packets from the STM32 sensor device
 * through NRF24L01 and displays the latest soil values on a 16x2 LCD.
 *
 * Hardware mapping:
 * - LCD 16x2 4-bit: D4 GPIO27, D5 GPIO26, D6 GPIO25, D7 GPIO33, E GPIO14, RS GPIO12
 * - NRF24L01 VSPI custom pins: IRQ GPIO22, MOSI GPIO21, MISO GPIO19,
 *   SCK GPIO18, CSN GPIO5, CE GPIO4
 */

#include <Arduino.h>
#include <LiquidCrystal.h>
#include <RF24.h>
#include <SPI.h>

#include "app_protocol.h"

#define APP_LCD_RS_PIN             (12)
#define APP_LCD_E_PIN              (14)
#define APP_LCD_D4_PIN             (27)
#define APP_LCD_D5_PIN             (26)
#define APP_LCD_D6_PIN             (25)
#define APP_LCD_D7_PIN             (33)

#define APP_NRF_IRQ_PIN            (22)
#define APP_NRF_MOSI_PIN           (21)
#define APP_NRF_MISO_PIN           (19)
#define APP_NRF_SCK_PIN            (18)
#define APP_NRF_CSN_PIN            (5)
#define APP_NRF_CE_PIN             (4)

#define APP_NRF_CHANNEL            (76)
#define APP_PACKET_TIMEOUT_MS      (5000UL)
#define APP_LCD_PAGE_PERIOD_MS     (2000UL)
#define APP_SERIAL_BAUDRATE        (115200UL)

typedef struct
{
    uint16_t sequence;
    uint8_t status;
    uint16_t moisture_x10;
    int16_t temperature_x10;
    uint16_t ph_x10;
    uint16_t conductivity_us_cm;
    uint16_t nitrogen_mg_kg;
    uint16_t phosphorus_mg_kg;
    uint16_t potassium_mg_kg;
} app_sensor_data_t;

static LiquidCrystal g_lcd(APP_LCD_RS_PIN,
                           APP_LCD_E_PIN,
                           APP_LCD_D4_PIN,
                           APP_LCD_D5_PIN,
                           APP_LCD_D6_PIN,
                           APP_LCD_D7_PIN);
static RF24 g_radio(APP_NRF_CE_PIN, APP_NRF_CSN_PIN);
static const uint8_t g_radio_address[5] = { 'A', 'G', 'R', 'I', '1' };

static app_sensor_data_t g_latest_data = {};
static uint32_t g_last_packet_ms = 0UL;
static uint32_t g_last_page_ms = 0UL;
static uint8_t g_lcd_page = 0U;
static bool g_has_packet = false;
static bool g_radio_ok = false;

static void APP_LcdWriteLine(uint8_t row, const char *text);
static void APP_PrintFixedX10(uint16_t value_x10);
static void APP_PrintSignedFixedX10(int16_t value_x10);
static bool APP_IsPayloadValid(const uint8_t *payload);
static void APP_DecodePayload(const uint8_t *payload, app_sensor_data_t *data);
static void APP_InitRadio(void);
static void APP_ShowStartup(void);
static void APP_ShowNoPacket(void);
static void APP_ShowSensorError(void);
static void APP_ShowPage0(void);
static void APP_ShowPage1(void);
static void APP_UpdateLcd(void);
static void APP_PollRadio(void);

/**
 * @brief Write one padded 16-character line to the LCD.
 */
static void APP_LcdWriteLine(uint8_t row, const char *text)
{
    char line[17];

    for (uint8_t index = 0U; index < 16U; index++)
    {
        if ((text != nullptr) && (text[index] != '\0'))
        {
            line[index] = text[index];
        }
        else
        {
            line[index] = ' ';
        }
    }

    line[16] = '\0';
    g_lcd.setCursor(0, row);
    g_lcd.print(line);
}

/**
 * @brief Print an unsigned fixed-point value stored as value * 10.
 */
static void APP_PrintFixedX10(uint16_t value_x10)
{
    g_lcd.print(value_x10 / 10U);
    g_lcd.print('.');
    g_lcd.print(value_x10 % 10U);
}

/**
 * @brief Print a signed fixed-point value stored as value * 10.
 */
static void APP_PrintSignedFixedX10(int16_t value_x10)
{
    if (value_x10 < 0)
    {
        g_lcd.print('-');
        value_x10 = (int16_t)-value_x10;
    }

    APP_PrintFixedX10((uint16_t)value_x10);
}

/**
 * @brief Validate packet magic, protocol version, and null pointer.
 */
static bool APP_IsPayloadValid(const uint8_t *payload)
{
    if (payload == nullptr)
    {
        return false;
    }

    return (payload[APP_PROTOCOL_IDX_MAGIC0] == APP_PROTOCOL_MAGIC_0) &&
           (payload[APP_PROTOCOL_IDX_MAGIC1] == APP_PROTOCOL_MAGIC_1) &&
           (payload[APP_PROTOCOL_IDX_VERSION] == APP_PROTOCOL_VERSION);
}

/**
 * @brief Decode a valid 32-byte sensor payload.
 */
static void APP_DecodePayload(const uint8_t *payload, app_sensor_data_t *data)
{
    if ((payload == nullptr) || (data == nullptr))
    {
        return;
    }

    data->status = payload[APP_PROTOCOL_IDX_STATUS];
    data->sequence = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_SEQUENCE_L);
    data->moisture_x10 = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_MOISTURE_L);
    data->temperature_x10 = APP_PROTOCOL_ReadS16(payload, APP_PROTOCOL_IDX_TEMPERATURE_L);
    data->ph_x10 = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_PH_L);
    data->conductivity_us_cm = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_EC_L);
    data->nitrogen_mg_kg = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_N_L);
    data->phosphorus_mg_kg = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_P_L);
    data->potassium_mg_kg = APP_PROTOCOL_ReadU16(payload, APP_PROTOCOL_IDX_K_L);
}

/**
 * @brief Configure NRF24L01 receiver to match the STM32 sensor transmitter.
 */
static void APP_InitRadio(void)
{
    pinMode(APP_NRF_IRQ_PIN, INPUT_PULLUP);
    SPI.begin(APP_NRF_SCK_PIN, APP_NRF_MISO_PIN, APP_NRF_MOSI_PIN, APP_NRF_CSN_PIN);

    g_radio_ok = g_radio.begin();
    if (!g_radio_ok)
    {
        return;
    }

    g_radio.setChannel(APP_NRF_CHANNEL);
    g_radio.setPayloadSize(APP_PROTOCOL_PAYLOAD_SIZE);
    g_radio.setAddressWidth(5);
    g_radio.setDataRate(RF24_1MBPS);
    g_radio.setPALevel(RF24_PA_HIGH);
    g_radio.setCRCLength(RF24_CRC_16);
    g_radio.setAutoAck(true);
    g_radio.openReadingPipe(0, g_radio_address);
    g_radio.startListening();
}

static void APP_ShowStartup(void)
{
    APP_LcdWriteLine(0U, "AgriVision GW");
    APP_LcdWriteLine(1U, "Starting...");
}

static void APP_ShowNoPacket(void)
{
    if (g_radio_ok)
    {
        APP_LcdWriteLine(0U, "Waiting sensor");
        APP_LcdWriteLine(1U, "NRF CH76 ID:AG");
    }
    else
    {
        APP_LcdWriteLine(0U, "NRF init failed");
        APP_LcdWriteLine(1U, "Check wiring");
    }
}

static void APP_ShowSensorError(void)
{
    APP_LcdWriteLine(0U, "Sensor node err");
    APP_LcdWriteLine(1U, "Check NPK side");
}

static void APP_ShowPage0(void)
{
    g_lcd.setCursor(0, 0);
    g_lcd.print("M:");
    APP_PrintFixedX10(g_latest_data.moisture_x10);
    g_lcd.print("% T:");
    APP_PrintSignedFixedX10(g_latest_data.temperature_x10);
    g_lcd.print("C   ");

    g_lcd.setCursor(0, 1);
    g_lcd.print("pH:");
    APP_PrintFixedX10(g_latest_data.ph_x10);
    g_lcd.print(" EC:");
    g_lcd.print(g_latest_data.conductivity_us_cm);
    g_lcd.print("    ");
}

static void APP_ShowPage1(void)
{
    g_lcd.setCursor(0, 0);
    g_lcd.print("N:");
    g_lcd.print(g_latest_data.nitrogen_mg_kg);
    g_lcd.print(" P:");
    g_lcd.print(g_latest_data.phosphorus_mg_kg);
    g_lcd.print("     ");

    g_lcd.setCursor(0, 1);
    g_lcd.print("K:");
    g_lcd.print(g_latest_data.potassium_mg_kg);
    g_lcd.print(" Seq:");
    g_lcd.print(g_latest_data.sequence);
    g_lcd.print("     ");
}

/**
 * @brief Refresh LCD based on radio and sensor state.
 */
static void APP_UpdateLcd(void)
{
    const uint32_t now_ms = millis();

    if ((now_ms - g_last_page_ms) < APP_LCD_PAGE_PERIOD_MS)
    {
        return;
    }

    g_last_page_ms = now_ms;

    if ((!g_has_packet) || ((now_ms - g_last_packet_ms) > APP_PACKET_TIMEOUT_MS))
    {
        APP_ShowNoPacket();
        return;
    }

    if (g_latest_data.status != APP_PROTOCOL_STATUS_OK)
    {
        APP_ShowSensorError();
        return;
    }

    if (g_lcd_page == 0U)
    {
        APP_ShowPage0();
        g_lcd_page = 1U;
    }
    else
    {
        APP_ShowPage1();
        g_lcd_page = 0U;
    }
}

/**
 * @brief Poll NRF24L01 and decode all queued packets.
 */
static void APP_PollRadio(void)
{
    uint8_t payload[APP_PROTOCOL_PAYLOAD_SIZE];

    if (!g_radio_ok)
    {
        return;
    }

    while (g_radio.available())
    {
        g_radio.read(payload, APP_PROTOCOL_PAYLOAD_SIZE);

        if (APP_IsPayloadValid(payload))
        {
            APP_DecodePayload(payload, &g_latest_data);
            g_last_packet_ms = millis();
            g_has_packet = true;

            Serial.print("RX seq=");
            Serial.print(g_latest_data.sequence);
            Serial.print(" status=");
            Serial.println(g_latest_data.status);
        }
        else
        {
            Serial.println("Invalid payload");
        }
    }
}

void setup(void)
{
    Serial.begin(APP_SERIAL_BAUDRATE);
    g_lcd.begin(16, 2);
    APP_ShowStartup();

    APP_InitRadio();
    delay(1000);
    g_last_page_ms = 0UL;
}

void loop(void)
{
    APP_PollRadio();
    APP_UpdateLcd();
    delay(10);
}
