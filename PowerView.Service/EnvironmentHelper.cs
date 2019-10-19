using System;
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
  }
}
