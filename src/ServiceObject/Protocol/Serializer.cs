namespace com.iiko.unitech.Protocol
{
    public static class Serializer
    {
        public static byte[] Serialize(RequestPacket packet)
        {
            return new[] {Packet.Esc, (byte)packet.Command};
        }

        public static ResponsePacket DeserializeNext(RequestType command, DeviceBuffer buffer)
        {
            int length = 0;
            ResponsePacket response = null;
            if (command == RequestType.TestConnection)
            {
                response = TestConnectionResponse.TryParse(buffer, out length);
            }
            if (command == RequestType.StartRead)
            {
                response = WaitCardRollResponse.TryParse(buffer, out length);
            }
            else if (command == RequestType.GetDeviceModel)
            {
                response = DeviceModelResponse.TryParse(buffer, out length);
            }
            else if (command == RequestType.GetFirmware)
            {
                response = DeviceFirmwareResponse.TryParse(buffer, out length);
            }
            if (response != null)
                buffer.RemoveFirst(length);
            return response;
        }
    }
}

