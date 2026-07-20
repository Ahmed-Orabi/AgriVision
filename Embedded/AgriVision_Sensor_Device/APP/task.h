/**
 * @file task.h
 * @brief Small cooperative task API matching the FreeRTOS task names used here.
 */

#ifndef TASK_H
#define TASK_H

#include "FreeRTOS.h"

typedef void (*TaskFunction_t)(void *argument);
typedef void *TaskHandle_t;

/**
 * @brief Register a task function with the cooperative scheduler.
 *
 * @param task_code Function called whenever the task is ready.
 * @param task_name Name kept for FreeRTOS source compatibility.
 * @param stack_depth Stack depth kept for FreeRTOS source compatibility.
 * @param parameters User parameter passed to the task.
 * @param priority Priority kept for FreeRTOS source compatibility.
 * @param created_task Optional returned task handle.
 * @return pdPASS when the task is registered, otherwise pdFAIL.
 */
BaseType_t xTaskCreate(TaskFunction_t task_code,
                       const char *task_name,
                       uint16 stack_depth,
                       void *parameters,
                       UBaseType_t priority,
                       TaskHandle_t *created_task);

/**
 * @brief Delay the current task by the requested number of scheduler ticks.
 */
void vTaskDelay(TickType_t ticks_to_delay);

/**
 * @brief Start the cooperative scheduler. This function never returns.
 */
void vTaskStartScheduler(void);

/**
 * @brief Get the current scheduler tick count.
 */
TickType_t xTaskGetTickCount(void);

#endif
