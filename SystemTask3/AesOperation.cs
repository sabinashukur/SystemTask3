using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SystemTask3;

public class AesOperation
{
    public static byte[] EncryptStringToBytes(string key, string plainText)
    {
        byte[] iv = new byte[16];

        using Aes aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new MemoryStream();

        using CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plainText);
        }

        return memoryStream.ToArray();
    }

    public static string DecryptString(string key, string cipherText)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new MemoryStream(buffer);

        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        using (StreamReader streamReader = new StreamReader(cryptoStream))
        {
            return streamReader.ReadToEnd();
        }
    }
}
