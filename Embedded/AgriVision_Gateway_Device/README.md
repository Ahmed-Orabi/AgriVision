# AgriVision Gateway Device

ESP32 DevKit V1 gateway firmware for receiving NRF24L01 telemetry from the STM32 sensor device and displaying it on a 16x2 LCD.

## Hardware

LCD 16x2 4-bit:
- RS -> GPIO12
- E -> GPIO14
- D4 -> GPIO27
- D5 -> GPIO26
- D6 -> GPIO25
- D7 -> GPIO33

NRF24L01:
- CE -> GPIO4
- CSN -> GPIO5
- SCK -> GPIO18
- MISO -> GPIO19
- MOSI -> GPIO21
- IRQ -> GPIO22

## Build

This folder is a PlatformIO Arduino project.

```powershell
pio run
pio run --target upload
pio device monitor
```

Required libraries are listed in `platformio.ini`.
