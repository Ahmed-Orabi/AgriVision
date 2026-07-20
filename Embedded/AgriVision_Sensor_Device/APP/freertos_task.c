/**
 * @file freertos_task.c
 * @brief Cooperative scheduler implementation for the local FreeRTOS-like API.
 *
 * Each registered task must return after one short unit of work. Calling
 * vTaskDelay() records the next wake tick for the current task. This is not a
 * preemptive RTOS and does not provide real per-task stacks, but it preserves a
 * task-based application structure for the sensor device firmware.
 */

#include "task.h"

#define FREERTOS_TASK_MAX_TASKS      (8U)
#define FREERTOS_DELAY_LOOPS_PER_MS  (4000UL)

typedef struct
{
    TaskFunction_t task_code;
    void *parameters;
    TickType_t wake_tick;
    uint8 is_used;
} freertos_task_control_t;

static freertos_task_control_t g_freertos_tasks[FREERTOS_TASK_MAX_TASKS];
static freertos_task_control_t *g_freertos_current_task = NULL;
static volatile TickType_t g_freertos_tick_count = 0U;

/**
 * @brief Busy-wait for a number of CPU cycles.
 */
static void FreeRTOS_DelayCycles(volatile uint32 cycles)
{
    while (cycles > 0U)
    {
        __asm volatile ("nop");
        cycles--;
    }
}

/**
 * @brief Blocking delay used by the scheduler tick loop.
 */
static void FreeRTOS_DelayMs(uint32 milliseconds)
{
    while (milliseconds > 0U)
    {
        FreeRTOS_DelayCycles(FREERTOS_DELAY_LOOPS_PER_MS);
        milliseconds--;
    }
}

BaseType_t xTaskCreate(TaskFunction_t task_code,
                       const char *task_name,
                       uint16 stack_depth,
                       void *parameters,
                       UBaseType_t priority,
                       TaskHandle_t *created_task)
{
    uint8 index;

    (void)task_name;
    (void)stack_depth;
    (void)priority;

    if (task_code == NULL)
    {
        return pdFAIL;
    }

    for (index = 0U; index < FREERTOS_TASK_MAX_TASKS; index++)
    {
        if (g_freertos_tasks[index].is_used == 0U)
        {
            g_freertos_tasks[index].task_code = task_code;
            g_freertos_tasks[index].parameters = parameters;
            g_freertos_tasks[index].wake_tick = 0U;
            g_freertos_tasks[index].is_used = 1U;

            if (created_task != NULL)
            {
                *created_task = &g_freertos_tasks[index];
            }

            return pdPASS;
        }
    }

    return pdFAIL;
}

void vTaskDelay(TickType_t ticks_to_delay)
{
    if (ticks_to_delay == 0U)
    {
        ticks_to_delay = 1U;
    }

    if (g_freertos_current_task != NULL)
    {
        g_freertos_current_task->wake_tick = g_freertos_tick_count + ticks_to_delay;
    }
    else
    {
        FreeRTOS_DelayMs(ticks_to_delay);
        g_freertos_tick_count += ticks_to_delay;
    }
}

TickType_t xTaskGetTickCount(void)
{
    return g_freertos_tick_count;
}

void vTaskStartScheduler(void)
{
    uint8 index;

    while (1)
    {
        for (index = 0U; index < FREERTOS_TASK_MAX_TASKS; index++)
        {
            if ((g_freertos_tasks[index].is_used != 0U) &&
                ((sint32)(g_freertos_tick_count - g_freertos_tasks[index].wake_tick) >= 0))
            {
                g_freertos_current_task = &g_freertos_tasks[index];
                g_freertos_current_task->wake_tick = g_freertos_tick_count + 1U;
                g_freertos_current_task->task_code(g_freertos_current_task->parameters);
                g_freertos_current_task = NULL;
            }
        }

        FreeRTOS_DelayMs(1U);
        g_freertos_tick_count++;
    }
}
