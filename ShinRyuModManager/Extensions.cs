using Yarhl.IO;

namespace ShinRyuModManager
{
    internal static class Extensions
    {
        public static byte[] ToArray(this DataStream stream)
        {
            long pos = stream.Position;
            stream.Position = 0;

            byte[] buf = new byte[stream.Length];
            stream.Read(buf, 0, buf.Length);

            stream.Position = pos;

            return buf;
        }
    }
}
