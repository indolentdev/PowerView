using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Moq;
using NUnit.Framework;
using PowerView.Service.Controllers;


namespace PowerView.Service.Test.Controllers;

[TestFixture]
public class EmptyQueryStringBinderProviderTest
{
    [Test]
    public void GetBinder_BindingSourceNotQuery()
    {
        // Arrange
        var context = GetModelBinderProviderContext(BindingSource.Body, typeof(string));
        var target = CreateTarget();

        // Act
        var res = target.GetBinder(context.Object);

        // Assert
        Assert.That(res, Is.Null);
    }

    [Test]
    public void GetBinder_ModelTypeNotString()
    {
        // Arrange
        var context = GetModelBinderProviderContext(BindingSource.Query, typeof(int));
        var target = CreateTarget();

        // Act
        var res = target.GetBinder(context.Object);

        // Assert
        Assert.That(res, Is.Null);
    }

    [Test]
    public void GetBinder()
    {
        // Arrange
        var context = GetModelBinderProviderContext(BindingSource.Query, typeof(string));
        var target = CreateTarget();

        // Act
        var res = target.GetBinder(context.Object);

        // Assert
        Assert.That(res, Is.TypeOf<EmptyQueryStringBinder>());
    }

    private Mock<ModelBinderProviderContext> GetModelBinderProviderContext(BindingSource bindingSource, Type modelType)
    {
        var context = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);

        var bindingInfo = new BindingInfo();
        bindingInfo.BindingSource = bindingSource;
        context.Setup(x => x.BindingInfo).Returns(bindingInfo);

        var key = ModelMetadataIdentity.ForType(modelType);
        var provider = new EmptyModelMetadataProvider();
        var detailsProvider = new Mock<ICompositeMetadataDetailsProvider>(MockBehavior.Strict);
        var parameter = this.GetType().GetMethod(nameof(DummyMethod), BindingFlags.NonPublic | BindingFlags.Instance).GetParameters()[0];
        var details = new DefaultMetadataDetails(key, ModelAttributes.GetAttributesForParameter(parameter, modelType));
        var metaData = new DefaultModelMetadata(provider, detailsProvider.Object, details);

        context.Setup(x => x.Metadata).Returns(metaData);

        return context;
    }

    private void DummyMethod(string stringParameter)
    {
    }

    private EmptyQueryStringBinderProvider CreateTarget()
    {
        return new EmptyQueryStringBinderProvider();
    }

}