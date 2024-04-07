using System.Net.Sockets;
using FileSharingServer.Net.IO;

namespace FileSharingServer;

public class Client
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public TcpClient ClientSocket { get; set; }

    private PacketReader _packetReader;

    public Client(TcpClient client)
    {
        ClientSocket = client;
        Id = Guid.NewGuid();

        _packetReader = new PacketReader(ClientSocket.GetStream());
        var opcode = _packetReader.ReadByte();
        Username = _packetReader.ReadMessage();

        Console.WriteLine($"[{DateTime.Now:g}]: {Username} has connected");

        Task.Run(Process);
    }

    void Process()
    {
        while (true)
        {
            try
            {
                var opcode = _packetReader.ReadByte();
                switch (opcode)
                {
                    case 2:
                        ProcessMessage();
                        break;
                    case 4:
                        ProcessFile();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{Id.ToString()}]: Disconnected!");
                Program.BroadcastDisconnect(Id.ToString());
                ClientSocket.Close();
                break;
            }
        }
    }

    private void ProcessMessage()
    {
        var user = _packetReader.ReadMessage();
        var msg = _packetReader.ReadMessage();
        var message = $"[{DateTime.Now:g}] {user}: {msg}";
        Console.WriteLine($"Message received!: {message}");
        Program.BroadcastMessage(message);
    }
    private void ProcessFile()
    {
        var user = _packetReader.ReadMessage();
        var fileName = _packetReader.ReadMessage();
        var file = _packetReader.ReadFile();
        Console.WriteLine($"File received!: {fileName}");
        Program.SaveFile(file, user, fileName);

    }
}