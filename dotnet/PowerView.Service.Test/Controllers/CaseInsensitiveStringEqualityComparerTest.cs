using System;
using System.Collections.Generic;
using NUnit.Framework;
using PowerView.Service.Controllers;

namespace PowerView.Service.Test.Controllers;

[TestFixture]
public class CaseInsensitiveStringEqualityComparerTest
{
    [Test]
    public void EqualsSameTextSameCase()
    {
        // Arrange
        const string s1 = "HELLO";
        const string s2 = "HELLO";
        var target = CreateTarget();

        // Act
        var equal = target.Equals(s1, s2);

        // Assert
        Assert.That(equal, Is.True);
    }

    [Test]
    public void EqualsSameTextDifferentCase()
    {
        // Arrange
        const string s1 = "HELLO";
        const string s2 = "hello";
        var target = CreateTarget();

        // Act
        var equal = target.Equals(s1, s2);

        // Assert
        Assert.That(equal, Is.True);
    }

    [Test]
    public void EqualsDifferentText()
    {
        // Arrange
        const string s1 = "HELLO";
        const string s2 = "OTHER";
        var target = CreateTarget();

        // Act
        var equal = target.Equals(s1, s2);

        // Assert
        Assert.That(equal, Is.False);
    }

    [Test]
    public void GetHashCodeTest()
    {
        // Arrange
        const string s1 = "HELLO";
        var target = CreateTarget();

        // Act
        var hashCode = target.GetHashCode(s1);

        // Assert
        Assert.That(hashCode, Is.EqualTo(s1.GetHashCode()));
    }

    private static IEqualityComparer<string> CreateTarget()
    {
        return new CaseInsensitiveStringEqualityComparer();
    }
}
