using System.Text;

namespace Jbot.Utils;

internal static class MemoryStreamExtensions
{
    public static void Write(this MemoryStream stream, short value)
    {
        stream.Write(BitConverter.GetBytes(value), 0, sizeof(short));
    }

    public static void Write(this MemoryStream stream, ushort value)
    {
        stream.Write(BitConverter.GetBytes(value), 0, sizeof(ushort));
    }

    public static void Write(this MemoryStream stream, int value)
    {
        stream.Write(BitConverter.GetBytes(value), 0, sizeof(int));
    }

    public static void Write(this MemoryStream stream, uint value)
    {
        stream.Write(BitConverter.GetBytes(value), 0, sizeof(uint));
    }

    public static void Write(this MemoryStream stream, long value)
    {
        stream.Write(BitConverter.GetBytes(value), 0, sizeof(long));
    }

    public static void Write(this MemoryStream stream, ulong value)
    {
        stream.Write(BitConverter.GetBytes(value), 0, sizeof(ulong));
    }

    public static void Write(this MemoryStream stream, string value)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(value);
        stream.Write((ushort)buffer.Length);
        stream.Write(buffer, 0, buffer.Length);
    }
}
