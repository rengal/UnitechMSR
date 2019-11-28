# Step-by-step driver setup

1. Go to the registry key  
**[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET\ControlAssemblies]**  
Create new string key with name *UnitehMSR* and specify full path to **UnitechMsrSO.dll** (not including the dll itself) in the value.

2. Go to the registry key  
**[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET]**  
locate configuration file in the **Configuration** key. By default, configuration file is located at
**C:\ProgramData\Microsoft\Point Of Service\Configuration\Configuration.xml**  
  
Edit or create (if not exists) **Configuration.xml** file. Add new entry `ServiceObject` with `Name="Unitech"` parameter:


```xml
<PointOfServiceConfig Version="1.0">у
  <ServiceObject Name="Unitech" Type="Scanner">
    <Device HardwarePath="COM1" Enabled="yes" PnP="no">
      <LogicalName Name="Unitech01" />
    </Device>
  </ServiceObject>
</PointOfServiceConfig>
```
Write serial port name instread of "COM1"

## Logger configuration

3. Create **log4net.config** file in the same folder as UnitechMsrSO.dll

```xml
<?xml version="1.0" encoding="utf-8"?>
<log4net>
  <appender name="mainAppender" type="log4net.Appender.RollingFileAppender">
    <lockingModel type="log4net.Appender.FileAppender+ExclusiveLock" />
    <file type="log4net.Util.PatternString" value="${AppData}/iiko/Unitech/unitech.log" />
    <threshold type="log4net.Core.Level" value="INFO" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="[%date{yyyy-MM-dd HH:mm:ss,fff}] %5p [%2t] [%type{1}:%M] - %m%n" />
    </layout>
    <AutoCompress value="true" />
    <LogFileAgeDays value="20" />
  </appender>
  <logger name="com.iiko.unitech" additivity="false">
    <appender-ref ref="mainAppender" />
  </logger>
</log4net>
```
You can optionaly set a different log level, e.g. **DEBUG** to log communication data with device.
```xml
<threshold type="log4net.Core.Level" value="DEBUG" />
```
