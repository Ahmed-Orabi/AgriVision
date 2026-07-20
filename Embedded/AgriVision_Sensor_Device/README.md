# AgriVision Sensor Device

STM32F401RC Black Pill firmware for the AgriVision sensor node. The device reads a 7-in-1 NPK soil sensor through MAX485/RS485, displays the latest values on a 20x4 LCD, and sends telemetry to the ESP32 gateway over NRF24L01.

## Hardware

Main board:
- STM32F401RC6 Black Pill

Wireless:
- NRF24L01

Display:
- HD44780-compatible LCD 20x4 in 4-bit mode

Soil sensor:
- NPK soil sensor 7-in-1 using Modbus RTU over RS485
- MAX485 transceiver between sensor and STM32 UART

## Pinout

### NRF24L01

| NRF24L01 | STM32F401RC |
| --- | --- |
| MOSI | PA7 |
| MISO | PA6 |
| SCK | PA5 |
| CSN | PA4 |
| CE | PA0 |
| IRQ | PB0 |
| VCC | 3.3V |
| GND | GND |

Use a stable 3.3V supply and place a capacitor near the NRF24L01 module.

### LCD 20x4

| LCD | STM32F401RC |
| --- | --- |
| D7 | PB4 |
| D6 | PB5 |
| D5 | PB6 |
| D4 | PB7 |
| E | PB8 |
| RS | PB9 |
| RW | GND |
| VSS | GND |
| VDD | 5V or LCD-rated supply |

### MAX485 / NPK Sensor

| MAX485 | STM32F401RC |
| --- | --- |
| DI | PA2 / USART2_TX |
| RO | PA3 / USART2_RX |
| DE/RE | PA1 |
| VCC | Module-rated supply |
| GND | GND |

Connect MAX485 `A/B` to the NPK sensor RS485 `A/B`. If the LCD shows `NPK: TIMEOUT`, try swapping `A` and `B`, verify common ground, and check the sensor baud/address.

## Firmware Structure

```text
APP/
  main.c          Sensor-node application tasks
  app.h           Application entry point
  app_protocol.h  32-byte NRF24 telemetry payload layout
  FreeRTOS.h      Minimal FreeRTOS-compatible types
  task.h          Minimal task API
  freertos_task.c Cooperative scheduler shim
  syscalls.c      Bare-metal newlib syscall stubs

HAL/
  LCD/            HD44780 LCD driver
  NRF24L01/       NRF24L01 driver
  NPK_SENSOR/     Modbus RTU NPK sensor driver

MCAL/
  GPIO/
  RCC/
  SPI/
  USART/
  Core/
```

## NPK Sensor Defaults

The firmware currently uses:
- Modbus address: `1`
- Baud rate: `4800`
- UART: `USART2`
- Register start: `0x0000`
- Register count: `7`

Decoded values:
- Moisture: percent x10
- Temperature: Celsius x10
- Conductivity: us/cm
- pH: pH x10
- Nitrogen: mg/kg
- Phosphorus: mg/kg
- Potassium: mg/kg

If your sensor manual specifies another baud rate or register map, update `HAL/NPK_SENSOR/HAL_NPK_SENSOR.h` and `HAL/NPK_SENSOR/HAL_NPK_SENSOR.c`.

## NRF24L01 Protocol

The sensor sends a fixed 32-byte payload to the gateway.

Radio settings:
- Channel: `76`
- Address: `AGRI1`
- Payload width: `32`
- CRC: enabled, 16-bit
- Data rate expected by gateway: `1 Mbps`

Payload layout is defined in:

```text
APP/app_protocol.h
```

The ESP32 gateway must use the same channel, address, and payload layout.

## Build

From this folder:

```powershell
cmake --preset Release
cmake --build --preset Release
```

Generated firmware files:

```text
build/Release/AgriVision_Sensor_Device.elf
build/Release/AgriVision_Sensor_Device.hex
build/Release/AgriVision_Sensor_Device.bin
```

## Flash

Using STM32CubeProgrammer CLI over ST-LINK/SWD:

```powershell
STM32_Programmer_CLI -c port=SWD -w build/Release/AgriVision_Sensor_Device.hex -v -rst
```

## LCD Diagnostics

If NPK data is not valid, the LCD shows a readable driver status:

| LCD Message | Meaning |
| --- | --- |
| `NPK: TIMEOUT` | No Modbus response received. Check power, A/B wiring, GND, baud rate, and address. |
| `NPK: CRC ERROR` | Response received but corrupted/noisy or wrong serial settings. |
| `NPK: ERROR` | Response format does not match the expected register frame. |
| `NPK: BAD PARAM` | Firmware configuration problem. |

## Notes

- The included `FreeRTOS.h`, `task.h`, and `freertos_task.c` are a small compatibility layer, not the official FreeRTOS kernel.
- The project builds and runs with the current CMake/STM32CubeCLT setup.
- Do not commit generated `build/` outputs.
