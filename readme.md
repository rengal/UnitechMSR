# Step-by-step driver setup

1. Add path to UnitechMsrSO.dll (Service Object) to the register. Add string parameter to the HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET\ControlAssemblies registry key
Unitech=<Full Path to UnitechMsrSO.dll>, e.g Unitech=D:\driver\Unitech

2. Edit Configuration.xml file. Full path to configuration file is stored in string param Configuration in HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET\ registry path. By default,
C:\ProgramData\Microsoft\Point Of Service\Configuration\Configuration.xml

```xml
<PointOfServiceConfig Version="1.0">
  ...
  <ServiceObject Name="Unitech" Type="Scanner">
    <Device HardwarePath="COM1" Enabled="yes" PnP="no">
      <LogicalName Name="Unitech01" />
    </Device>
  </ServiceObject>
</PointOfServiceConfig>
```
Write serial port name instread of "COM1"

3. Put log4net.config file in the same folder as UnitechMsrSO.dll

```xml
<?xml version="1.0" encoding="utf-8"?>
<log4net>
  <appender name="mainAppender" type="log4net.Appender.RollingFileAppender">
    <lockingModel type="log4net.Appender.FileAppender+ExclusiveLock" />
    <file type="log4net.Util.PatternString" value="%property{LogFileDir}/unitech.log" />
    <threshold type="log4net.Core.Level" value="TRACE" />
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
