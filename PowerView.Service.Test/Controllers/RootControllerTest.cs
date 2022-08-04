using System;
using PowerView.Service.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace PowerView.Service.Test.Controllers
{
    [TestFixture]
    public class RootControllerTest
    {
        private RootController target;

        [SetUp]
        public void SetUp()
        {
            target = new RootController();
        }

        [Test]
        public void GetRootRouteRedirect()
        {
            // Arrange

            // Act
            var result = target.Get();

            // Assert
            Assert.That(result, Is.TypeOf<RedirectResult>());
            var redirectResult = (RedirectResult)result;
            Assert.That(redirectResult.Url, Is.EqualTo("web/index.html"));
            Assert.That(redirectResult.Permanent, Is.False);
            Assert.That(redirectResult.PreserveMethod, Is.False);
        }


    }
}
