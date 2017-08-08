# Flying Pi
For a more structured view, go to [Wiki](https://github.com/sguertl/Flying_Pi/wiki).
## Overview
Flying Pi is a project for piloting a Raspberry Pi Zero W on a LARIX drone with a smartphone.
+ [Change SSID limitations](#change-ssid-limitations)
+ [Connect to a network](#connect-to-a-network)
+ [Choose selection mode](#choose-selection-mode)
+ [Pilot the drone](#pilot-the-drone)
+ [Modify the joystick](#modify-the-joystick)

### Smartphone app
All smartphone app related files can be found under [Dronection](https://github.com/sguertl/Flying_Pi/tree/master/Dronection). There you can find Android and iOS versions of the app, either using WiFi or Bluetooth (Android only).

### Raspberry Pi
The folder [RPI](https://github.com/sguertl/Flying_Pi/tree/master/RPI) contains files related to the Raspberry Pi:
+ Python scripts
+ Configuration files

### LARIX drone
In the [XMC](https://github.com/sguertl/Flying_Pi/tree/master/XMC) folder are all files stored, that are necessary for the bidirectional communication between Raspberry Pi and XMC.

___
## Dronection
### WiFiDronection - Android
WiFiDronection has two main functionalities:
+ Piloting a LARIX drone with a Raspberry Pi Zero W on it.
+ Visualizing data which was sent by the LARIX drone during a flight.
##### Change SSID limitations
In `MainActivity.cs` the android device searches automatically for a matching WiFi network. Only networks with a SSID including *Rasp* and/or *Pi* (no matter if upper- or lowercase) can be found. If you want to change this limitation, you have to edit the following line in `RefreshWiFiList()`:
```C#
IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));
```
You can replace an exisiting string or add a new one by writing `|| w.Ssid.ToUpper().Contains("WHATEVER")`.
If there is no matching network, you can't go on to the Controller.
##### Connect to network
If you press the __Connect__ button you will be asked to enter the password. The default password is 87654321 (or 00000000), but it depends on the hostapd-configuration of the Raspberry Pi. If you are already connected to the network, you can just press the OK button and ignore the dialog and a new socket connection (`SocketConnection.cs`) to `172.24.1.1:5050` will be established.

On most Android devices a popup-dialog will appear while connecting to the Raspberry PI, which will say, "The Internet is unavaiable with "..." (Picture below). This Popup is not created by the Donection application, but by the OS and just shows, that you have no Internet access over the Raspberry Pi, because it is configured as an Access Point between the mobile phone and the XMC-Drone.

<img src="https://github.com/sguertl/Flying_Pi/blob/master/Res/Images/InternetUnavaiableWith.png" alt="The Internet is unavaiable with Raspberry Pi" width="600">

##### Choose selection mode
After connecting to the network, you will be forwarded to `ControllerActivity.cs`. Before using the smartphone as a controller, you have to choose between two selection modes by clicking either on the radio button or the image.

<img src="https://github.com/sguertl/Flying_Pi/blob/master/Res/Images/mode1.png" alt="Mode 1" width="400"><img src="https://github.com/sguertl/Flying_Pi/blob/master/Res/Images/mode2.png" alt="Mode 2" width="400">

You can't change the controlling mode during a flight.

##### Pilot the drone
After pressing __Start__, a view with two joysticks, which are responsible for controlling the drone, and the socket connection will open. This process takes place in `ControllerActivity.cs` and `ControllerView.cs`. 
At the top of the screen you can make following settings:
+ Activate/deactivate altitude control
+ Adjust trims for yaw, pitch and roll

If altitude control is deactivated, the button has the background color blue and you can control the drone manually. A click on __Altitude Control__ turns the background color to red and sets the speed to 50 % (although the joystick is not at 50 %). When altitude control is activated, you can just control yaw, pitch and roll while speed is regulated by the drone itself.

Trims can be adjusted by choosing the radio button and moving the seekbar. Just for explanation:
+ Plus Yaw: Turn right
+ Minus Yaw: Turn left
+ Plus Pitch: Move forward
+ Minus Pitch: Move backward
+ Plus roll: Move right
+ Minus roll: Move left

If you want to change the minimum and maximum trim value, you have to edit `mMinTrim` in `ControllerActivity.cs` and `android:max` of the seekbar in `ControllerLayout.axml`.

##### Modify the joystick
Currently, the sensibility of the joysticks is very low. You can make it higher by giving `mMult`, `mMultRudder` and/or `mMultThrottle` a higher value in `Joystick.cs`.

___
## Raspberry Pi
### RPI as access point
A Raspberry Pi Zero W can be configured as an access point providing a wireless LAN with the following steps. If you are not root, you have to give your user rights with this command:
```
sudo chmod 777 <dir_or_file>
```
##### Step 1: Download packages
Download hostapd and dnsmasq:
```
sudo apt-get update
sudo apt-get install dnsmasq hostapd
```
##### Step 2: Configure static IP
Replace the `wlan0` section in `/etc/network/interfaces` with the following text:
```
allow-hotplug wlan0  
iface wlan0 inet static  
    address 172.24.1.1
    netmask 255.255.255.0
    network 172.24.1.0
    broadcast 172.24.1.255
```
Restart `dhcpcd` with `sudo service dhcpcd restart` and reload the configuration for `wlan0` with 
```
sudo ifdown wlan0
sudo ifup wlan0
```
##### Step 3: Configure hostapd
Create a new file with `sudo nano /etc/hostapd/hostapd.conf` with the following content:
```
# Set the name of the configured wifi interface
interface=wlan0

# Use the nl80211 driver with the brcmfmac driver
driver=nl80211

# Set the SSID of the network
ssid=RaspberryWiFi

# Use the 2.4GHz band
hw_mode=g

# Use channel 6
channel=6

# Enable 802.11n
ieee80211n=1

# Enable WMM
wmm_enabled=1

# Enable 40MHz channels with 20ns guard interval
ht_capab=[HT40][SHORT-GI-20][DSSS_CCK-40]

# Accept all MAC addresses
macaddr_acl=0

# Use WPA authentication
auth_algs=1

# Require clients to know the network name
ignore_broadcast_ssid=0

# Use WPA2
wpa=2

# Use a pre-shared key
wpa_key_mgmt=WPA-PSK

# Set network passphrase
wpa_passphrase=raspberry

# Use AES instead of TKIP
rsn_pairwise=CCMP
```
Edit the file `/etc/default/hostapd` and change the line `#DAEMON_CONF=""` to
```
DAEMON_CONF="/etc/hostapd/hostapd.conf"
```
##### Step 4: Configure dnsmasq
Open the file `/etc/dnsmasq.conf` and add the following content:
```
# Use interface wlan0
interface=wlan0

# Explicitly specify the address to listen on
listen-address=172.24.1.1

# Bind to the interface to make sure we aren't sending things elsewhere
bind-interfaces

# Forward DNS requests to Google DNS
server=8.8.8.8

# Don't forward short names
domain-needed

# Never forward addresses in the non-routed address spaces
bogus-priv

# Assign IP addresses between 172.24.1.50 and 172.24.1.150 with a 12 hour lease time
dhcp-range=172.24.1.50,172.24.1.150,12h
```
##### Step 5: Restart services
The last step is to restart the services:
```
sudo service hostapd start
sudo service dnsmasq start
```
Thanks to [Phil Martin](https://frillip.com/using-your-raspberry-pi-3-as-a-wifi-access-point-with-hostapd/) for this tutorial!

### Serial communication on RPI
To receive and send data between the XMC and Raspberry Pi Zero W, some configuration must be done on the RPI. It's a good idea to make a backup of your SD card before configuring. Reboots during the steps are also recommended. Again, if you are not root, you have to give your user rights with this command:
```
sudo chmod 777 <dir_or_file>
```
##### Step 1: Change /boot/config.txt
Add following content to the bottom of the file `/boot/config.txt`:
```
gpu_mem=128
enable_uart=1
dtoverlay=w1-gpio
force_turbo=1
```
##### Step 2: Change /boot/cmdline.txt
As the next step, search the following line in '/boot/cmdline.txt':
```
dwc_otg.lpm_enable=0 console=tty1 root=/dev/mmcblk0p2 rootfstype=ext4 elevator=deadline fsck.repair=yes rootwait
```
Delete `console=serial0,115200` and the whole content after 'rootwait'.
##### Step 3: Enable serial
Type the command `sudo raspi-config`, choose `Interface options` -> `Serial` and then disable shell and enable interface.
##### Step 4: Change /lib/systemd/system/hciuart.service [Optional]
Open the file `/lib/systemd/system/hciuart.service`and replace everything with:
```
[Unit]
Description=Configure Bluetooth Modems connected by UART
ConditionPathIsDirectory=/proc/device-tree/soc/gpio@7e200000/bt_pins
Before=bluetooth.service
After=dev-ttyS0.device

[Service]
Type=forking
ExecStart=/usr/bin/hciattach /dev/ttyS0 bcm43xx 921600 noflow -

[Install]
WantedBy=multi-user.target
```
Create a file `fhem` in `/etc/init.d/` and add `sleep 10` as content.

Thanks to [FHEM Wiki](https://wiki.fhem.de/wiki/Raspberry_Pi_3:_GPIO-Port_Module_und_Bluetooth) for this tutorial! 
