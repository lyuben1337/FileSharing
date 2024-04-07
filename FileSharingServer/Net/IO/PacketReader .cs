using System.Net.Sockets;
using System.Text;

namespace FileSharingServer.Net.IO;

public class PacketReader : BinaryReader
{
    private NetworkStream _ns;

    public PacketReader(NetworkStream ns) : base(ns)
    {
        _ns = ns;
    }

    public string ReadMessage()
    {
        var length = ReadInt32();
        var msgBuffer = new byte[length];
        _ns.Read(msgBuffer, 0, length);

        var msg = Encoding.ASCII.GetString(msgBuffer);
        return msg;
    }

    public byte[] ReadFile()
    {
        var fileSizeBytes = ReadBytes(sizeof(int));
        var fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

        var fileData = new byte[fileSize];
        _ns.Read(fileData, 0, fileSize);

        return fileData;
    }

}