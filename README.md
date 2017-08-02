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
##### Change SSID limitations
In `MainActivity.cs` the android device searches automatically for a matching WiFi network. Only networks with a SSID including <i>Raspberry</i> and/or <i>Pi</i> (no matter if upper- or lowercase) can be found. If you want to change this limitation, you have to edit the following line in `RefreshWiFiList`:
```C#
    IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));
```
You can replace an exisiting string or add a new one by writing `|| w.Ssid.ToUpper().Contains("NEWLIMITATION")`.
If there is no matching network, you can't go on to the Controller.
##### Connect to a network









 



