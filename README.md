# Raspberry Remote for Windows 10 IoT Core

## About
Control your remote plugs with your Raspberry Pi running Windows 10 IoT Core.

The code of this project is based on the code from the [Raspberry-Remote](https://github.com/xkonni/raspberry-remote) project.

Instead of using [wiringPi](https://projects.drogon.net/raspberry-pi/wiringpi/) this project uses the Windows.Devices.Gpio interface
provided by Microsoft in the extension Windows IoT Extensions for the UWP.

## Credits
* xkonni, sui li, r10r and x3 for their work on the original [Rrapberry-Remote](https://github.com/xkonni/raspberry-remote)

## Required Hardware
* Raspberry Pi
* 433 MHz transmitter
* Some radio controlled power sockets
* Some connector wires

## Setup
* Setup Windows 10 IoT Core on your Raspberry Pi [GetStarted](http://ms-iot.github.io/content/en-US/GetStarted.htm)
* Connect your transmitter with your Raspberry Pi. As data port I prefer the GPIO port 5 which is the pin 29 on the board of your Raspberry. But you can chose every port you want (theoretically). If you're not familiar the Raspberry Pi pin mappings you should have a look on [Raspberry Pi 2 Pin Mappings](http://ms-iot.github.io/content/en-US/win10/samples/PinMappingsRPi2.htm).
* If you don't already have a project in Visual Studio create a new Windows Universal project.
* Checkout the Raspberry Remote for Windows 10 IoT Core project and add it to your solution by doing a right click on your solution and choose "Add" -> "Existing Project". Navigate to RaspberryRemote.csproj and confirm your selection.
* Now add a reference to Raspberry Remote to your project by doing a right click on "References" and choose "Add Reference..." -> "Projects" -> "Solution" -> RaspberryRemote. Confirm with OK.


## Usage
* Insert the namespace of RaspberryRemote
```
	using RaspberryRemote;
```
* Create a new instance of RCSwitch
```
    RCSwitch switch = new RCSwitch();
```
* Set up the GPIO pin which your transmitter is connected with. The parameter defines the GPIO pin not the pin number on the board.
```
	switch.EnableTransmit(5);
```
* Now you can switch on your remote plug with
```
	switch.SwitchOn("11011", 1);
```
The first parameter is the code of the switch group (refers to DIP switches 1..5 where "1" = on and "0" = off, if all DIP switches are on it's "11111").
The second parameter is the number of the switch itself (1..4).

* To switch off your remote plug call
```
	switch.SwitchOff("11011", 1);
```

# A short example will follow soon.
