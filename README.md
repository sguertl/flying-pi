# Flying Pi
Flying Pi is a project for piloting a Raspberry Pi Zero W on a LARIX drone with a smartphone.

## Overview
### Smartphone app
All smartphone app related files can be found under [Dronection](https://github.com/sguertl/Flying_Pi/tree/master/Dronection). There you can find Android and iOS versions of the app, either using WiFi or Bluetooth (Android only).

### Raspberry Pi
The folder [RPI](https://github.com/sguertl/Flying_Pi/tree/master/RPI) contains files related to the Raspberry Pi:
+ Python scripts
+ Configuration files

### LARIX drone
In the [XMC](https://github.com/sguertl/Flying_Pi/tree/master/XMC) folder are all files stored, that are necessary for the bidirectional communication between Raspberry Pi and XMC.

## Dronection
### WiFiDronection - Android
WiFiDronection has two main functionalities:
+ Piloting a LARIX drone with a Raspberry Pi Zero W on it.
+ Visualizing data which was sent by the LARIX drone during a flight.

In <code>MainActivity.cs</code> the android device searches for a matching WiFi network. The SSID <b>must</b> include <i>Raspberry</i> and/or <i>Pi</i>.

