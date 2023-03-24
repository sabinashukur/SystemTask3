using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SystemTask3;

public partial class MainWindow : Window
{
    private readonly OpenFileDialog _openFileDialog;
    private readonly CancellationTokenSource _cts;

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
        if (_openFileDialog.ShowDialog() is true)
        {
            tbox_fileName.Text = _openFileDialog.FileName;
            btn_start.IsEnabled = true;
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

        if (rdbutton_encrpyt.IsChecked == false && rdbutton_decrpyt.IsChecked == false)
        {
            MessageBox.Show("Choose action");
            return;
        }

        if (rdbutton_encrpyt.IsChecked is true)
        {
            EncryptAndWrite(_cts.Token);
        }

        if (rdbutton_decrpyt.IsChecked is true)
        {
            DecryptAndWrite(_cts.Token);
        }
    }

    private void EncryptAndWrite(CancellationToken token)
    {
        var filePath = tbox_fileName.Text;
        var plainText = File.ReadAllText(filePath);
        var key = Encoding.UTF8.GetBytes(tbox_password.Text);

        var encryptedBytes = AesOperation.EncryptStringToBytes(plainText, key, key);

        var dividedBytesArray = encryptedBytes.Chunk(10);

        progressBar.Maximum = dividedBytesArray.Count();


        ThreadPool.QueueUserWorkItem(_ =>
        {
            using FileStream fs = new FileStream(filePath, FileMode.Truncate);

            foreach (var bytesArray in dividedBytesArray)
            {
                if (token.IsCancellationRequested)
                {
                    MessageBox.Show("Cancelled");
                    return;
                }

                fs.Write(bytesArray);

                Thread.Sleep(20);

                Dispatcher.Invoke(() =>
                {
                    progressBar.Value++;
                }, DispatcherPriority.Normal, token);
            }

            MessageBox.Show("Completed successfully");
        });

    }

    private void DecryptAndWrite(CancellationToken token)
    {
        var encryptedBytes = File.ReadAllBytes(tbox_fileName.Text);
        var key = Encoding.UTF8.GetBytes(tbox_password.Text);

        var plainText = AesOperation.DecryptStringFromBytes(encryptedBytes, key, key);

        var dividedBytesArray = Encoding.UTF8.GetBytes(plainText).Chunk(10);

        using FileStream fs = new FileStream(tbox_fileName.Text, FileMode.Truncate);

        foreach (var bytesArray in dividedBytesArray)
        {
            fs.Write(bytesArray);
            //Thread.Sleep(100);
        }

        MessageBox.Show("Completed successfully");

    }

    private void btn_cancel_Click(object sender, RoutedEventArgs e)
    {
        _cts.Cancel();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to close the window", "Close?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

        if (result != MessageBoxResult.Yes)
        {
            e.Cancel = true;
            _cts.Cancel();
        }
    }
}
