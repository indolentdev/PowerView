using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class EntityDeserializerTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new EntityDeserializer(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetValueThrows()
    {
      // Arrange
      var target = CreateTarget("{}");

      // Act & Assert
      Assert.That(() => target.GetValue<int>(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.GetValue<int>(new string[0]), Throws.TypeOf<EntitySerializationException>());
      Assert.That(() => target.GetValue<int>("NonExistingProperty"), Throws.TypeOf<EntitySerializationException>());
    }

    [Test]
    public void GetValueRootProperty()
    {
      // Arrange
      var target = CreateTarget("{\"TheProperty\":1234}");

      // Act
      var l = target.GetValue<long>("TheProperty");

      // Assert
      Assert.That(l, Is.EqualTo(1234));
    }

    [Test]
    public void GetValueNestedProperty()
    {
      // Arrange
      var target = CreateTarget("{\"Nested\":{\"TheProperty\":\"ABCD\"}}");

      // Act
      var s = target.GetValue<string>("Nested", "TheProperty");

      // Assert
      Assert.That(s, Is.EqualTo("ABCD"));
    }

    private static EntityDeserializer CreateTarget(string json)
    {
      var target = new EntityDeserializer((Newtonsoft.Json.Linq.JContainer)Newtonsoft.Json.JsonConvert.DeserializeObject(json));
      return target;
    }

  }
}

