using System.IO;
using System.Security.Cryptography;

namespace SystemTask3;

public class AesOperation
{
    public static byte[] EncryptStringToBytes(string original, byte[] key, byte[] IV)
    {
        using var encryption = Aes.Create();

        encryption.Key = key;
        encryption.IV = IV;

        ICryptoTransform encryptor = encryption.CreateEncryptor(encryption.Key, encryption.IV);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

        using (var swEncrypt = new StreamWriter(csEncrypt))
            swEncrypt.Write(original);

        return msEncrypt.ToArray();
    }

    public static string DecryptStringFromBytes(byte[] encrypted, byte[] key, byte[] IV)
    {
        using var encryption = Aes.Create();

        encryption.Key = key;
        encryption.IV = IV;

        ICryptoTransform decryptor = encryption.CreateDecryptor(encryption.Key, encryption.IV);

        using MemoryStream msDecrypt = new MemoryStream(encrypted);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        {
            return srDecrypt.ReadToEnd();
        }
    }
}
