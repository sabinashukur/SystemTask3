using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace SystemTask3;

public partial class MainWindow : Window
{
    private readonly OpenFileDialog _openFileDialog;
    private readonly CancellationTokenSource _cts;

    //private bool _isVisiblePasswordBox;
    //public bool IsVisiblePasswordBox
    //{
    //    get { return tbox_fileName.Text.Length > 0; }
    //    set { _isVisiblePasswordBox = value; }
    //}

    public bool IsVisiblePasswordBox => tbox_fileName.Text.Length > 0;


    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _openFileDialog = new();
        _cts = new();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _openFileDialog.Filter = "Txt files (*.txt)|*.txt";
        _openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    private void btn_chooseFile_Click(object sender, RoutedEventArgs e)
    {
        if (_openFileDialog.ShowDialog() == true)
        {
            tbox_fileName.Text = _openFileDialog.FileName;
        }
    }

    private void btn_start_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(tbox_fileName.Text))
        {
            MessageBox.Show("File path cannot be empty");
            return;
        }

        if (!File.Exists(tbox_fileName.Text))
        {
            MessageBox.Show("File doesn't exist");
            return;
        }

        if (tbox_password.Text.Length < 16)
        {
            MessageBox.Show("Password must contain 16 characters");
            return;
        }

        if (rdbutton_encrpyt.IsChecked == false || rdbutton_decrpyt.IsChecked == false)
        {
            MessageBox.Show("Choose action");
            return;
        }

        EncryptAndWrite(_cts.Token);

    }

    private void EncryptAndWrite(CancellationToken token)
    {
        var plainText = File.ReadAllText(tbox_fileName.Text);

        var encryptedBytes = AesOperation.EncryptStringToBytes(tbox_password.Text, plainText);

        var dividedBytesArray = encryptedBytes.Chunk(10);
        using FileStream fs = new FileStream(tbox_fileName.Text, FileMode.Truncate);

        foreach (var bytesArray in dividedBytesArray)
        {
            fs.Write(bytesArray, 0, bytesArray.Length);
            //Thread.Sleep(100);
        }
    }

    private void btn_cancel_Click(object sender, RoutedEventArgs e)
    {

    }
}
