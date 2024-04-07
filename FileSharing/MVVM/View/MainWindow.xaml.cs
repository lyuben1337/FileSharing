using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using FileSharingClient.MVVM.Model;

namespace FileSharingClient.MVVM.View;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
    {
        ButtonSend.IsEnabled = true;
        UsernameInput.IsEnabled = false;
        ButtonSendFile.IsEnabled = true;
    }

    private void FilesListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var selectedFile = (FileModel?)FilesListView.SelectedItem;

        if (selectedFile == null)
        {
            return;
        }

        string fileExtension = Path.GetExtension(selectedFile.Name);

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            FileName = selectedFile.Name,
            DefaultExt = fileExtension,
            Filter = $"Files (*{fileExtension})|*{fileExtension}|All files (*.*)|*.*"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            string filePath = saveFileDialog.FileName;

            File.WriteAllBytes(filePath, selectedFile.Content);

            MessageBox.Show("File downloaded successfully!");
        }
    }

}