using System;
using System.Linq;
using System.IO;
using System.Reflection;

namespace PowerView.Service
{
  public static class EnvironmentHelper
  {
    public static string GetMonoRuntimeVersion()
    {
      Type type = Type.GetType("Mono.Runtime");
      if (type == null)
      {
        return null;
      }

      MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
      if (displayName == null)
      {
        return null;
      }
      var monoRuntimeVersion = displayName.Invoke(null, null);
      return monoRuntimeVersion != null ? monoRuntimeVersion.ToString() : null;
    }

    public static string GetCpuInfoRevision()
    {
      const string cpuInfo = @"/proc/cpuinfo";
      if (!File.Exists(cpuInfo))
      {
        return null;
      }

      try
      {
        var cpuInfoLines = File.ReadAllLines(cpuInfo);
        var revision = cpuInfoLines.FirstOrDefault(x => x.StartsWith("Revision", StringComparison.InvariantCultureIgnoreCase));
        if (revision == null)
        {
          return null;
        }
        var seperator = revision.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
        if (seperator < 0 || seperator + 2 >= revision.Length)
        {
          return null;
        }

        var revisionValue = revision.Substring(seperator + 1);
        return revisionValue.Trim();
      }
      catch (UnauthorizedAccessException) { }
      catch (IOException) { }
      return null;
    }

  }
}
