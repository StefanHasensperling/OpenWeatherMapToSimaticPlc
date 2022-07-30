# What is it
This small tool allows you to query current Weather data from OpenWeatherMap.org and send the data to any Siemens Simatic PLC
This is helpfull if you need this current Weather data, such as Sea-Level-Air Pressure and such in an Automation System, for example to calculate Evaporations and such. 

Currently all configurations are done via the OpenWeatherMapToSimaticPLC.xml file. Thre are no command line parameters.

All values are sent as "Real" datatypes to the PLC, so you have to reserve 4 bytes for each value

# What Requirements does it have
It requres an valid OpenWeatherMap API-Key. You have to get your own key by registering to one of the subscriptions on [OpenWeatherMap](https://OpenWeatherMap.org).
And it needs an Internet connection of course, since it has to query whe OpenWeatherMap API. This should not be a problem, but in Industrial Automation systems this may be more difficult. 

# How does it Work
It uses OpenWeatherMap by calling its API, sending the API-Key and Location. The received data is then Parsed by using the great [Newtonsoft.Json](https://www.newtonsoft.com/json).
The data is then sent to any compatible Simatic PLC by using the great [DotNetSiemensPLCToolBoxLibrary](https://github.com/dotnetprojects/DotNetSiemensPLCToolBoxLibrary).

# How to use it
First, you need the API-Key. Get one from OpenWeatherMap.org. If you have an key, open the OpenWeatherMapToSimaticPLC.config file and edit the settings.
All settings should be self explanitory and also have comments on them to clarify.
If you call this application it will connect to OpenWeatherMap once, parse the data end sent it to the PLC, then exits.
if you want to create an cyclic update, then you should use the "Windows Task Scheduler" and create an new repeating task, that call the executabel in any frequency you like. 
Keep in mind, that you may have an Requests per day limit, dependin on you Subscription with OpenWeatherMap.