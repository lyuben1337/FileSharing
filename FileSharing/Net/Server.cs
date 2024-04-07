using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using FileSharingClient.Net.IO;

namespace FileSharingClient.Net;

public class Server
{
    private TcpClient _client;
    public PacketReader PacketReader;

    public event Action ConnectedEvent;
    public event Action MsgRecievedEvent;
    public event Action DisconnectedEvent;
    public event Action FileRecievedEvent;
    public Server()
    {
        _client = new TcpClient();
    }

    public void ConnectToServer(string username)
    {
        if (!_client.Connected)
        {
            _client.Connect("127.0.0.1", 8010);
            PacketReader = new PacketReader(_client.GetStream());

            if (!string.IsNullOrEmpty(username))
            {
                var connectPacket = new PacketBuilder();
                connectPacket.WriteOpCode(0);
                connectPacket.WriteMessage(username);
                _client.Client.Send(connectPacket.GetPacketBytes());
            }

            ReadPackets();
        }
    }

    public void SendMessage(string? message, string username)
    {
        var messagePacket = new PacketBuilder();
        messagePacket.WriteOpCode(2);
        messagePacket.WriteMessage(username);
        messagePacket.WriteMessage(message);
        _client.Client.Send(messagePacket.GetPacketBytes());
    }

    public void SendFile(string filePath, string username)
    {
        byte[] fileData;
        using (var fileStream = File.OpenRead(filePath))
        {
            var fileName = Path.GetFileName(filePath);
            fileData = new byte[fileStream.Length];
            fileStream.Read(fileData, 0, fileData.Length);
        }

        var filePacket = new PacketBuilder();
        filePacket.WriteOpCode(4);
        filePacket.WriteMessage(username);
        filePacket.WriteMessage(Path.GetFileName(filePath));
        filePacket.WriteFile(fileData);

        _client.Client.Send(filePacket.GetPacketBytes());
    }


    private void ReadPackets()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var opcode = PacketReader.ReadByte();
                switch (opcode)
                {
                    case 1:
                        ConnectedEvent?.Invoke();
                        break;
                    case 2:
                        MsgRecievedEvent?.Invoke();
                        break;
                    case 3:
                        DisconnectedEvent?.Invoke();
                        break;
                    case 4:
                        FileRecievedEvent?.Invoke();
                        break;
                    default:
                        break;
                }
            }
        });
    }
}