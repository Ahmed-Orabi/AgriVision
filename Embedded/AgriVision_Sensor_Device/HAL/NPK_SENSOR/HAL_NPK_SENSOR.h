/**
 * @file HAL_NPK_SENSOR.h
 * @brief HAL driver for a Modbus RTU 7-in-1 NPK soil sensor over MAX485.
 *
 * The driver uses an already-configured USART peripheral and one GPIO pin to
 * control the MAX485 DE/RE direction line. It reads the common seven-register
 * map: moisture, temperature, conductivity, pH, nitrogen, phosphorus, potassium.
 */

#ifndef HAL_NPK_SENSOR_H
#define HAL_NPK_SENSOR_H

#include "MCAL_USART.h"
#include "mcal_gpio.h"

#define HAL_NPK_SENSOR_DEFAULT_ADDRESS       (0x01U)
#define HAL_NPK_SENSOR_DEFAULT_BAUDRATE      (4800UL)

typedef enum
{
    /** Operation completed successfully. */
    HAL_NPK_SENSOR_STATUS_OK = 0U,
    /** Generic communication or protocol error. */
    HAL_NPK_SENSOR_STATUS_ERROR,
    /** One or more input pointers or pins are invalid. */
    HAL_NPK_SENSOR_STATUS_INVALID_PARAM,
    /** Response CRC16 does not match the received frame. */
    HAL_NPK_SENSOR_STATUS_CRC_ERROR,
    /** USART receive timed out before the full Modbus response arrived. */
    HAL_NPK_SENSOR_STATUS_TIMEOUT
} hal_npk_sensor_status_t;

/**
 * @brief Static configuration for an NPK sensor instance.
 */
typedef struct
{
    /** USART used for Modbus RTU through MAX485. */
    USART_RegDef_t *uart;
    /** GPIO port for the shared DE/RE direction-control pin. */
    GPIO_RegDef_t *de_re_port;
    /** GPIO pin for the shared DE/RE direction-control pin. */
    mcal_gpio_pin_t de_re_pin;
    /** Modbus slave address. Use 0 to select HAL_NPK_SENSOR_DEFAULT_ADDRESS. */
    uint8 device_address;
    /** Blocking millisecond delay callback used for RS485 turnaround timing. */
    void (*delay_ms)(uint32 milliseconds);
} hal_npk_sensor_config_t;

/**
 * @brief Decoded 7-in-1 soil sensor reading.
 */
typedef struct
{
    /** Soil moisture in percent x10. Example: 253 means 25.3%. */
    uint16 moisture_x10;
    /** Soil temperature in Celsius x10. Example: 241 means 24.1 C. */
    sint16 temperature_x10;
    /** Electrical conductivity in us/cm. */
    uint16 conductivity_us_cm;
    /** Soil pH x10. Example: 68 means pH 6.8. */
    uint16 ph_x10;
    /** Nitrogen concentration in mg/kg. */
    uint16 nitrogen_mg_kg;
    /** Phosphorus concentration in mg/kg. */
    uint16 phosphorus_mg_kg;
    /** Potassium concentration in mg/kg. */
    uint16 potassium_mg_kg;
} hal_npk_sensor_data_t;

/**
 * @brief Runtime NPK sensor object.
 */
typedef struct
{
    USART_RegDef_t *uart;
    GPIO_RegDef_t *de_re_port;
    mcal_gpio_pin_t de_re_pin;
    uint8 device_address;
    void (*delay_ms)(uint32 milliseconds);
    uint8 is_initialized;
} hal_npk_sensor_t;

/**
 * @brief Initialize an NPK sensor object and configure the MAX485 direction pin.
 *
 * USART baud rate and TX/RX GPIO alternate functions must be configured by the
 * application before calling this function.
 */
hal_npk_sensor_status_t HAL_NPK_SENSOR_Init(hal_npk_sensor_t *sensor,
                                            const hal_npk_sensor_config_t *config);

/**
 * @brief Read all seven soil values using one Modbus RTU transaction.
 */
hal_npk_sensor_status_t HAL_NPK_SENSOR_ReadAll(hal_npk_sensor_t *sensor,
                                               hal_npk_sensor_data_t *data);

#endif
