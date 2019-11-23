using System.Text;
using System.Threading.Tasks;
using com.iiko.unitech.Protocol;

namespace com.iiko.unitech
{
    public partial class MsrDriver
    {
        private void ProcessDataReceivedAsync()
        {
            Task.Factory.StartNew(() =>
            {
                lock (gate)
                {
                    ProcessDataReceived();
                }
            });
        }

        private void ProcessDataReceived()
        {
            var cardRollData = Serializer.DeserializeNext(RequestType.StartRead, receiveBuffer) as WaitCardRollResponse;
            if (cardRollData != null)
            {
                SchedulePoll();
                FireCardRolledAsync(cardRollData);
            }
            if (currentCommandDto == null || currentCommandDto.response != null)
                return;
            currentCommandDto.response = Serializer.DeserializeNext(currentCommandDto.request.Command, receiveBuffer);
            if (currentCommandDto.response != null)
            {
                currentCommandDto.completedEvent.Set();
            }
        }

        private void FireCardRolledAsync(WaitCardRollResponse cardRollData)
        {
            Task.Factory.StartNew(() =>
            {
                lock (gate)
                {
                    var track = cardRollData.Track2;
                    Log.Info($"Card rolled: {Encoding.ASCII.GetString(track)}, bytes: {Utils.ByteArrayToHexString(track, 0, track.Length)} ({track.Length} bytes)");
                    OnCardRolled?.Invoke(this, cardRollData.Track2);
                }
            });
        }

        private void ProcessCardRolled()
        {
        }
    }
}