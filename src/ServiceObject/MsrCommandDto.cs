using System.Threading;
using com.iiko.unitech.Protocol;

namespace com.iiko.unitech
{
    public class MsrCommandDto
    {
        public RequestPacket request;
        public ResponsePacket response;
        public readonly ManualResetEvent completedEvent = new ManualResetEvent(false);
        public string ErrMessage;

        public MsrCommandDto(RequestPacket request)
        {
            this.request = request;
        }

    }
}
