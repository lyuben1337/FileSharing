using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using FileSharingClient.MVVM.Core;
using FileSharingClient.MVVM.Model;
using FileSharingClient.Net;
using Microsoft.Win32;

namespace FileSharingClient.MVVM.ViewModel;

public class MainViewModel
{
    public ObservableCollection<UserModel> Users { get; set; }
    public ObservableCollection<string> Messages { get; set; }
    public ObservableCollection<FileModel> Files { get; set; }
    public RelayCommand ConnectToServerCommand { get; set; }
    public RelayCommand SendMessageToServerCommand { get; set; }
    public RelayCommand SendFileCommand { get; set; }

    private Server _server;
    public string Username { get; set; }
    public string Message { get; set; }
    public  string FilePath { get; set; }
    public RelayCommand SelectFileCommand { get; set; }

    public MainViewModel()
    {
        Users = new ObservableCollection<UserModel>();
        Messages = new ObservableCollection<string>();
        Files = new ObservableCollection<FileModel>();

        _server = new Server();

        _server.ConnectedEvent += UserConnected;
        _server.MsgRecievedEvent += MessageRecieved;
        _server.DisconnectedEvent += UserDisconnected;
        _server.FileRecievedEvent += FileRecieved;

        ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
        SendMessageToServerCommand = new RelayCommand(o => _server.SendMessage(Message, Username), o => !string.IsNullOrEmpty(Message));
        SendFileCommand = new RelayCommand(o => _server.SendFile(FilePath, Username), o => !string.IsNullOrEmpty(FilePath));
        SelectFileCommand = new RelayCommand(o => SelectFile());
    }

    private void SelectFile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            FilePath = openFileDialog.FileName;
            SendFileCommand.Execute(null);
        }
    }

    private void FileRecieved()
    {
        var id = _server.PacketReader.ReadMessage();
        var fileName = _server.PacketReader.ReadMessage();
        var sender = _server.PacketReader.ReadMessage();
        var fileStream = _server.PacketReader.ReadFile();

        Application.Current.Dispatcher.Invoke(() => Files.Add(new()
        {
            Id = id,
            Name = fileName,
            Sender = sender,
            Content = fileStream
        }));
    }

    private void UserDisconnected()
    {
        var id = _server.PacketReader.ReadMessage();
        var user = Users.Where(u => u.Id == id).FirstOrDefault();
        Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
    }


    private void MessageRecieved()
    {
        var msg = _server.PacketReader.ReadMessage();
        Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
    }

    private void UserConnected()
    {
        var user = new UserModel
        {
            Username = _server.PacketReader.ReadMessage(),
            Id = _server.PacketReader.ReadMessage()
        };

        if (Users.All(u => u.Id != user.Id))
        {
            Application.Current.Dispatcher.Invoke(() => Users.Add(user));
        }
    }
}