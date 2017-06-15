# TemperatureAlarm

Main purpose of this application is monitoring of temperature in the fridge (provided by some DS18b20 sensors).
It is meant to run on Raspberry PI (1,2,3, no matter).
When necessary it performs alarming via cellular network - sends SMSes and calls subscribers.
It is written in C# (2.0), and uses RPI.net GPIO library.

## Hardware
In order to make use of this software some pieces of hardware are required:
a) Raspberry Pi,
b) Hardware clock (i.e DS1307, nor really necessary but useful, especially when RPI is not connected to the net),
c) USB GSM/WCDMA modem (Raspberry compatible, accepting some AT commands, I used Huawei e220),
d) DS18B20 temperature sensors.

## Software
a) Linux with Mono,
b) RPI.net GPIO library.
