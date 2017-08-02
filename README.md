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
In `MainActivity.cs` the android device searches automatically for a matching WiFi network. Only networks with a SSID including *Rasp* and/or *Pi* (no matter if upper- or lowercase) can be found. If you want to change this limitation, you have to edit the following line in `RefreshWiFiList`:
```C#
    IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));
```
You can replace an exisiting string or add a new one by writing `|| w.Ssid.ToUpper().Contains("WHATEVER")`.
If there is no matching network, you can't go on to the Controller.
##### Connect to a network
If you press the __Connect__ button you will be asked to enter the password. The default password is 87654321 (or 00000000), but it depends on the hostapd-configuration of the Raspberry Pi. If you are already connected to the network, you can just press the OK button and ignore the dialog and a new socket connection (`SocketConnection.cs`) will be established.

##### Choose selection mode
After connecting to the network, you will be forwared to `ControllerActivity.cs`. Before using the smartphone as a controller, you have to choose between two selection modes by clicking either on the radio button or the image.

<img src="https://github.com/sguertl/Flying_Pi/blob/master/Res/Images/mode1.png" alt="Mode 1" width="400"><img src="https://github.com/sguertl/Flying_Pi/blob/master/Res/Images/mode2.png" alt="Mode 2" width="400">

You can't change the controlling mode during a flight.

#### Pilot the drone
After pressing __Start__, a view with two joysticks, which are responsible for controlling the drone, and the socket connection will open. This process takes place in `ControllerActivity.cs` and `ControllerView.cs`. 
At the top of the screen you can make following settings:
+ Activate/deactivate altitude control
+ Adjust trims for yaw, pitch and roll





