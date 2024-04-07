using System;
using System.IO;
using System.Net;
using System.Text;

namespace FileSharingClient.Net.IO;

public class PacketBuilder
{
    private MemoryStream _ms;
    public PacketBuilder()
    {
        _ms = new MemoryStream();
    }

    public void WriteOpCode(byte opcode)
    {
        _ms.WriteByte(opcode);
    }

    public void WriteMessage(string msg)
    {
        _ms.Write(BitConverter.GetBytes(msg.Length));
        _ms.Write(Encoding.ASCII.GetBytes(msg));
    }

    public void WriteFile(byte[] fileData)
    {
        var fileSizeBytes = BitConverter.GetBytes(fileData.Length);
        _ms.Write(fileSizeBytes, 0, fileSizeBytes.Length);

        _ms.Write(fileData, 0, fileData.Length);
    }

    public byte[] GetPacketBytes()
    {
        return _ms.ToArray();
    }
}