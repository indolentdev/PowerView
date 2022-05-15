/*
using System;
using Org.BouncyCastle.Crypto;

namespace PowerView.Service
{
  // Causes the BouncyCastle.Crypto.dll to be copied into the _PowerView_ project output directory..
  // Seemes to only copy the dll if this lib actually "uses" the reference.
  public static class BouncyCastleReference
  {
    private static void Dummy()
    {
      Action<Type> noop = _ => { };
      var dummy = typeof(BufferedBlockCipher);
      noop(dummy);
    }
  }
}
*/