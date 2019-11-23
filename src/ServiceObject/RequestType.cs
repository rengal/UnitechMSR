namespace com.iiko.unitech
{
    public enum RequestType : byte
    {
        Reset = 0x61,
        TestConnection = 0x65,
        StartRead = 0x72,
        GetDeviceModel = 0x74,
        GetFirmware = 0x76
    }
}
