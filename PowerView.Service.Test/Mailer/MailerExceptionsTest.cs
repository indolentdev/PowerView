using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using PowerView.Service.Mailer;

namespace PowerView.Service.Test.Mailer
{
  [TestFixture]
  public class MailerExceptionsTest
  {
    [Test]
    public void MailerExceptionSerializeDeserialize()
    {
      // Arrange
      var target = new MailerException("Msg1", new Exception("Msg2"));

      // Act & Assert
      SerializeDeserializeAndAssert(target);
    }

    [Test]
    public void ConnectMailerExceptionSerializeDeserialize()
    {
      // Arrange
      var target = new ConnectMailerException("Msg1", new Exception("Msg2"));

      // Act & Assert
      SerializeDeserializeAndAssert(target);
    }

    [Test]
    public void AuthenticateMailerExceptionSerializeDeserialize()
    {
      // Arrange
      var target = new AuthenticateMailerException("Msg1", new Exception("Msg2"));

      // Act & Assert
      SerializeDeserializeAndAssert(target);
    }

    private static void SerializeDeserializeAndAssert(MailerException target)
    {
      // Arrange
      object deserialized;
      IFormatter formatter = new BinaryFormatter();

      // Act
      using (var ms = new MemoryStream())
      {
        formatter.Serialize(ms, target);
        ms.Position = 0;
        deserialized = formatter.Deserialize(ms);
      }

      // Assert
      Assert.That(deserialized, Is.Not.Null);
      Assert.That(deserialized.GetType(), Is.EqualTo(target.GetType()));
      var target2 = (MailerException)deserialized;
      Assert.That(target2.Message, Is.EqualTo(target.Message));
      Assert.That(target2.InnerException.GetType(), Is.EqualTo(target.InnerException.GetType()));
      Assert.That(target2.InnerException.Message, Is.EqualTo(target.InnerException.Message));
    }
  }
}

