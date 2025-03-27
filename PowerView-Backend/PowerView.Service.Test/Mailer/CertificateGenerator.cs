using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PowerView.Service.Test.Mailer
{
  internal class CertificateGenerator
  {
    public byte[] GenerateCertificateForPersonalFileExchange(string siteName, string pfxPassword)
    {
      using (var rsa = RSA.Create(2048))
      {
        var request = new CertificateRequest($"CN={siteName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Add basic constraints (self-signed, no CA)
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        // Add key usage (digital signature and key encipherment)
        request.CertificateExtensions.Add(
          new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

        // Add enhanced key usage (server authentication)
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
          new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, // Server Authentication
          true));

        // Create the self-signed certificate
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

        // Export the certificate as a PFX file
        var pfxBytes = certificate.Export(X509ContentType.Pfx, pfxPassword);

        return pfxBytes;
      }
    }
  }
}