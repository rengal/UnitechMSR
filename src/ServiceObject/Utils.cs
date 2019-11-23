using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace com.iiko.unitech
{
    public static class Utils
    {
        public static string ByteArrayToHexString(byte[] source, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + length; i++)
            {
                var item = source[i];
                if (i < offset + length - 1)
                    sb.Append(item.ToString("X2") + " ");
                else
                    sb.Append(item.ToString("X2"));
            }

            return sb.ToString();
        }

        public static int FindFirst(byte[] source, byte[] subArray, int startOffset = 0)
        {
            for (var index = startOffset; index < source.Length; index++)
            {
                if (source
                    .Skip(index)
                    .Take(subArray.Length)
                    .SequenceEqual(subArray))
                    return index;
            }
            return -1;
        }
    }
}