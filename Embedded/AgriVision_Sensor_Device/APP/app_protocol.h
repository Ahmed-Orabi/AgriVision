/**
 * @file app_protocol.h
 * @brief NRF24L01 telemetry payload layout shared by sensor and gateway devices.
 *
 * Payload size is fixed at 32 bytes because the NRF24L01 maximum static payload
 * is 32 bytes. Multi-byte values are written little-endian.
 */

#ifndef APP_PROTOCOL_H
#define APP_PROTOCOL_H

#include "PLATFORM_TYPES.h"

#define APP_PROTOCOL_PAYLOAD_SIZE       (32U)
#define APP_PROTOCOL_MAGIC_0            ((uint8)'A')
#define APP_PROTOCOL_MAGIC_1            ((uint8)'G')
#define APP_PROTOCOL_VERSION            (1U)

#define APP_PROTOCOL_STATUS_OK          (0U)
#define APP_PROTOCOL_STATUS_SENSOR_ERR  (1U)

typedef enum
{
    APP_PROTOCOL_IDX_MAGIC0 = 0U,
    APP_PROTOCOL_IDX_MAGIC1 = 1U,
    APP_PROTOCOL_IDX_VERSION = 2U,
    APP_PROTOCOL_IDX_STATUS = 3U,
    APP_PROTOCOL_IDX_SEQUENCE_L = 4U,
    APP_PROTOCOL_IDX_SEQUENCE_H = 5U,
    APP_PROTOCOL_IDX_MOISTURE_L = 6U,
    APP_PROTOCOL_IDX_MOISTURE_H = 7U,
    APP_PROTOCOL_IDX_TEMPERATURE_L = 8U,
    APP_PROTOCOL_IDX_TEMPERATURE_H = 9U,
    APP_PROTOCOL_IDX_PH_L = 10U,
    APP_PROTOCOL_IDX_PH_H = 11U,
    APP_PROTOCOL_IDX_EC_L = 12U,
    APP_PROTOCOL_IDX_EC_H = 13U,
    APP_PROTOCOL_IDX_N_L = 14U,
    APP_PROTOCOL_IDX_N_H = 15U,
    APP_PROTOCOL_IDX_P_L = 16U,
    APP_PROTOCOL_IDX_P_H = 17U,
    APP_PROTOCOL_IDX_K_L = 18U,
    APP_PROTOCOL_IDX_K_H = 19U
} app_protocol_index_t;

/**
 * @brief Write a 16-bit value to a payload buffer in little-endian order.
 */
static inline void APP_PROTOCOL_WriteU16(uint8 *payload, uint8 index, uint16 value)
{
    payload[index] = (uint8)(value & 0xFFU);
    payload[index + 1U] = (uint8)((value >> 8U) & 0xFFU);
}

#endif
