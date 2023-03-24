using Microsoft.Win32;
using System;
using System.ComponentModel;
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
    private CancellationTokenSource _cts;

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
            Reset();
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

        btn_start.IsEnabled = false;
        btn_cancel.IsEnabled = true;

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
                    fs.Close();
                    File.WriteAllText(filePath, plainText);
                    return;
                }

                fs.Write(bytesArray);

                Thread.Sleep(20);

                Dispatcher.Invoke(() =>
                {
                    if (!token.IsCancellationRequested)
                        progressBar.Value++;
                });

            }

            Dispatcher.Invoke(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    Reset();
                }
            });

            MessageBox.Show("Completed successfully");
        });

    }

    private void DecryptAndWrite(CancellationToken token)
    {
        var filePath = tbox_fileName.Text;
        var encryptedBytes = File.ReadAllBytes(filePath);
        var key = Encoding.UTF8.GetBytes(tbox_password.Text);

        var plainText = AesOperation.DecryptStringFromBytes(encryptedBytes, key, key);

        var dividedBytesArray = Encoding.UTF8.GetBytes(plainText).Chunk(10);

        progressBar.Maximum = dividedBytesArray.Count();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            using FileStream fs = new FileStream(filePath, FileMode.Truncate);

            foreach (var bytesArray in dividedBytesArray)
            {
                if (token.IsCancellationRequested)
                {
                    MessageBox.Show("Cancelled");
                    fs.Close();
                    File.WriteAllText(filePath, plainText);
                    return;
                }

                fs.Write(bytesArray);

                Thread.Sleep(20);

                Dispatcher.Invoke(() =>
                {
                    if (!token.IsCancellationRequested)
                        progressBar.Value++;
                });

            }

            Dispatcher.Invoke(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    Reset();
                }
            });

            MessageBox.Show("Completed successfully");
        });

    }

    private void btn_cancel_Click(object sender, RoutedEventArgs e)
    {
        _cts.Cancel();
        _cts = new();
        Reset();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to close the window", 
                                     "Close?", 
                                     MessageBoxButton.YesNoCancel,
                                     MessageBoxImage.Question,
                                     MessageBoxResult.Cancel);

        if (result != MessageBoxResult.Yes)
        {
            _cts.Cancel();
            e.Cancel = true;
        }
    }

    private void Reset()
    {
        btn_start.IsEnabled = true;
        btn_cancel.IsEnabled = false;
        progressBar.Value = 0;
    }
}
