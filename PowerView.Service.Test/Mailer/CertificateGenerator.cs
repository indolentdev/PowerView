using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace PowerView.Service.Test.Mailer
{
  /// <summary>
  /// With inspiration from:  (and then some)
  /// to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
  /// http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development
  /// </summary>
  internal class CertificateGenerator : IDisposable
  {
    private readonly string folder;

    // {0}: site name
    // {1}: pfx password
    private readonly string[] templateCommands =
    {
      "makecert -n \"CN={0}CertificateAuthority\" -cy authority -a sha1 -sv \"{0}CertificateAuthorityPrivateKey.pvk\" -r \"{0}CertificateAuthority.cer\"",
      "makecert -n \"CN={0}\" -ic \"{0}CertificateAuthority.cer\" -iv \"{0}CertificateAuthorityPrivateKey.pvk\" -a sha1 -sv \"{0}PrivateKey.pvk\" \"{0}.cer\"",
      "openssl rsa -inform pvk -in \"{0}PrivateKey.pvk\" -outform pem -out \"{0}PrivateKey.pem\"",
      "openssl x509 -inform DER -in \"{0}CertificateAuthority.cer\" -out \"{0}CertificateAuthority.crt\"",
      "openssl x509 -inform DER -in \"{0}.cer\" -out \"{0}.crt\"",
      "openssl pkcs12 -export -passout pass:\"{1}\" -out \"{0}.pfx\" -inkey \"{0}PrivateKey.pem\" -in \"{0}.crt\" -certfile \"{0}CertificateAuthority.crt\""
    };


    public CertificateGenerator()
    {
      var codeBase = new Uri(this.GetType().Assembly.CodeBase);
      var asmDirectory = Path.GetDirectoryName(codeBase.AbsolutePath);
      folder = Path.Combine(asmDirectory, "CertificateGenerator" + DateTime.Now.Millisecond);

      Directory.CreateDirectory(folder);
    }

    public byte[] GenerateCertificateForPersonalFileExchange(string siteName, string pfxPassword)
    {
      try
      {
        foreach (var templateCommand in templateCommands)
        {
          RunCommand(templateCommand, siteName, pfxPassword);
        }
      }
      catch (Exception)
      {
        Dispose();
        throw;
      }

      var pfxCertificate = Path.Combine(folder, siteName + ".pfx");
      return File.ReadAllBytes(pfxCertificate);
    }

    private void RunCommand(string commandFormat, params object[] args)
    {
      var command = string.Format(System.Globalization.CultureInfo.InvariantCulture, commandFormat, args);
      var executableIndex = command.IndexOf(' ');
      var fileName = command.Substring(0, executableIndex);
      var arguments = command.Substring(executableIndex+1);

      var processStartInfo = new ProcessStartInfo
      {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = folder,
        UseShellExecute = false,
        CreateNoWindow = true,
        ErrorDialog = false,
        RedirectStandardOutput = true, // Prevent cert tool output on the command line.
        RedirectStandardError = true // Prevent cert tool output on the command line.
      };
      var process = Process.Start(processStartInfo);
      process.WaitForExit();
      if (process.ExitCode != 0)
      {
        throw new ApplicationException("Failed running process. Exitcode:" + process.ExitCode + ", Process:" + command);
      }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (Directory.Exists(folder))
          {
            Directory.Delete(folder, true);
          }
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion

  }

}