using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace PowerView.Model
{
  public class BoxCryptor
  {
    public static string Encrypt(string plainText, DateTime ivDateTime)
    {
      ArgCheck.ThrowIfNullOrEmpty(plainText);
      ArgCheck.ThrowIfNotUtc(ivDateTime);

      using (var aes = Aes.Create())
      {
        ConfigureAes(aes, ivDateTime);

        ICryptoTransform encryptor = aes.CreateEncryptor();
        using (MemoryStream msEncrypt = new MemoryStream())
        {
          using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
          {
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, System.Text.Encoding.UTF8))
            {
              swEncrypt.Write(plainText);
            }
            var cipherText = msEncrypt.ToArray();
            return Convert.ToBase64String(cipherText);
          }
        }
      }
    }

    public static string Decrypt(string cipherTextBase64, DateTime ivDateTime)
    {
      ArgCheck.ThrowIfNullOrEmpty(cipherTextBase64);
      ArgCheck.ThrowIfNotUtc(ivDateTime);

      byte[] cipherText;
      try
      {
        cipherText = Convert.FromBase64String(cipherTextBase64);
      }
      catch (FormatException e)
      {
        throw new BoxCryptorException("Decrypt failed. Base64 decode error", e);
      }

      try
      {
        using (var aes = Aes.Create())
        {
          ConfigureAes(aes, ivDateTime);

          ICryptoTransform decryptor = aes.CreateDecryptor();
          using (MemoryStream msDecrypt = new MemoryStream(cipherText))
          {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
              using (StreamReader srDecrypt = new StreamReader(csDecrypt, System.Text.Encoding.UTF8))
              {
                return srDecrypt.ReadToEnd();
              }
            }
          }

        }
      }
      catch (CryptographicException e)
      {
        throw new BoxCryptorException("Decrypt failed. Crypto error", e);
      }
    }

    private static void ConfigureAes(Aes aes, DateTime ivDateTime)
    {
      aes.Mode = CipherMode.CBC;
      aes.Padding = PaddingMode.PKCS7;
      aes.KeySize = 256;
      aes.BlockSize = 128;
      aes.IV = GetIV(ivDateTime);
      aes.Key = GetKey();
    }

    private static byte[] GetIV(DateTime ivDateTime)
    {
      var dateTimeBytes = BitConverter.GetBytes(ivDateTime.Ticks);
      return dateTimeBytes.Concat(dateTimeBytes).ToArray();
    }

    private static byte[] GetKey()
    {
      var key = new byte[] { 0x00, 0x00 }.Concat(Enumerable.Repeat((byte)0x01, 30)).ToArray();

      var physicalAddress = GetEthernetPhysicalAddress();
      if (physicalAddress != null && physicalAddress.Length > 0)
      {
        int reps = (key.Length-2) / physicalAddress.Length;
        for (var i = 0; i < reps; i++)
        {
          Array.Copy(physicalAddress, 0, key, 2+i*physicalAddress.Length, physicalAddress.Length);
        }
      }
      return key;
    }

    private static byte[] GetEthernetPhysicalAddress()
    {
      var firstEthernetPhysicalAddress = NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.NetworkInterfaceType.ToString().Contains("Ethernet"))
        .Select(x => new
        {
          AddressBytes = x.GetPhysicalAddress().GetAddressBytes(),
          AddressString = BitConverter.ToString(x.GetPhysicalAddress().GetAddressBytes())
        })
        .OrderBy(x => x.AddressString)
        .FirstOrDefault();

      return firstEthernetPhysicalAddress != null ? firstEthernetPhysicalAddress.AddressBytes : null;
    }

  }
}
