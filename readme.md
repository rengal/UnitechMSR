# Настройка драйвера

1. Добавить путь к файлу UnitechMsrSO.dll (Service Object) в реестре. Для этого в ветке реестра HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\POSfor.NET\ControlAssemblies необходимо добавить строковый параметр
Unitech=D:\Full\Path\To\Driver

2. Настроить файл Configuration.xml

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

Здесь вместо COM1 нужно подставить порт, к которому подключено устройство.
