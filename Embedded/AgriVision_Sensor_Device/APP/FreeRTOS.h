/**
 * @file FreeRTOS.h
 * @brief Minimal FreeRTOS-compatible type and macro definitions.
 *
 * The official FreeRTOS kernel is not part of this project tree. These
 * definitions provide the small API surface used by the application so the
 * firmware remains structured as tasks and can be migrated to the real kernel
 * later with limited application changes.
 */

#ifndef FREERTOS_H
#define FREERTOS_H

#include "PLATFORM_TYPES.h"

typedef int BaseType_t;
typedef unsigned int UBaseType_t;
typedef uint32 TickType_t;

/** @brief Generic successful return value. */
#define pdPASS              (1)
/** @brief Generic failed return value. */
#define pdFAIL              (0)
/** @brief Boolean true value used by FreeRTOS APIs. */
#define pdTRUE              (1)
/** @brief Boolean false value used by FreeRTOS APIs. */
#define pdFALSE             (0)
/** @brief Convert milliseconds to scheduler ticks. This scheduler uses 1 ms ticks. */
#define pdMS_TO_TICKS(ms)   ((TickType_t)(ms))

#endif
