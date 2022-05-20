using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace PowerView.Service.Test.Mailer
{
  /// <summary>
  /// With inspiration from: 
  /// https://letsencrypt.org/docs/certificates-for-localhost/
  /// http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development
  /// .. and for reference:
  /// https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide
  /// </summary>
  internal class CertificateGenerator : IDisposable
  {
    private readonly string folder;

    // {0}: site name
    // {1}: pfx password
    private readonly string[] templateCommands =
    {
      "openssl req -x509 -out \"{0}.crt\" -keyout \"{0}.key\" -newkey rsa:2048 -nodes -sha256 -subj /CN={0}",
      "openssl pkcs12 -export -passout pass:\"{1}\" -out \"{0}.pfx\" -inkey \"{0}.key\" -in \"{0}.crt\""
    };

    public CertificateGenerator()
    {
      var assemblyLocation = new Uri(this.GetType().Assembly.Location);
      var asmDirectory = Path.GetDirectoryName(assemblyLocation.AbsolutePath);
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
      var stdout = process.StandardOutput.ReadToEnd();
      var stderr = process.StandardError.ReadToEnd();
      if (process.ExitCode != 0)
      {
        throw new ApplicationException("Failed running process. Exitcode:" + process.ExitCode + ", Process:" + command + ", stdout:" + stdout + ", stderr:" + stderr);
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