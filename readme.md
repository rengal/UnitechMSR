# Настройка драйвера

1. Add path to UnitechMsrSO.dll (Service Object) to the register. Add string parameter to the HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET\ControlAssemblies registry key
Unitech=<Full Path to UnitechMsrSO.dll>, e.g Unitech=D:\driver\Unitech

2. Edit Configuration.xml file. Full path to configuration file is stored in string param Configuration in HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET\ registry path.

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
