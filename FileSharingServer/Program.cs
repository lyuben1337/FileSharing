using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using FileSharingServer.Net.IO;

namespace FileSharingServer;

class Program
{
    private static TcpListener _listener;
    private static List<Client> _users;
    private static List<FileModel> _files;

    public static void Main(string[] args)
    {
        _users = new List<Client>();
        _files = new List<FileModel>(); 
        _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8010);
        _listener.Start();
        while (true)
        {
            var client = new Client(_listener.AcceptTcpClient());
            _users.Add(client);
            BroadcastMessage($"[{DateTime.Now:g}] Server: {client.Username} connected!");
            BroadcastConnection();
        }
    }

    public static void BroadcastConnection()
    {
        foreach (var user in _users)
        {
            foreach (var usr in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(1);
                msgPacket.WriteMessage(usr.Username);
                msgPacket.WriteMessage(usr.Id.ToString());
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }
    }

    public static void BroadcastMessage(string message)
    {
        foreach (var user in _users)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(2);
            msgPacket.WriteMessage(message);
            user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }
    }

    public static void BroadcastDisconnect(string uid)
    {
        var disconnectedUser = _users.Where(u => u.Id.ToString() == uid).FirstOrDefault();
        _users.Remove(disconnectedUser);

        foreach (var user in _users)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(3);
            msgPacket.WriteMessage(uid);
            user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }

        BroadcastMessage($"[{DateTime.Now:g}] Server: {disconnectedUser.Username} disconnected!");
    }

    public static void SaveFile(byte[] fileContent, string sender, string fileName)
    {
        var file = new FileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = fileName,
            Sender = sender,
            Content = fileContent
        };

        _files.Add(file);

        BroadcastMessage($"[{DateTime.Now:g}] Server: {sender} saved {fileName}!");

        foreach (var user in _users)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(4);
            msgPacket.WriteMessage(file.Id);
            msgPacket.WriteMessage(file.Name);
            msgPacket.WriteMessage(file.Sender);
            msgPacket.WriteFile(file.Content);
            user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }
    }
}