using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using PowerView.Service.Controllers;
using Microsoft.AspNetCore.Http;

namespace PowerView.Service.Test.Controllers;

[TestFixture]
public class EmptyQueryStringBinderTest
{
    [Test]
    public async Task NameAbsent()
    {
        // Arrange
        const string name = "bindId";
        var queryValues = new Dictionary<string, StringValues>();
        var bindingContext = GetModelBindingContext(name, queryValues);
        var target = CreateTarget();

        // Act
        await target.BindModelAsync(bindingContext);

        // Assert
        Assert.That(bindingContext.Result.IsModelSet, Is.False);
    }

    [Test]
    public async Task NamePresent()
    {
        // Arrange
        const string name = "bindId";
        var valueStrings = new StringValues(new[] { "Hep1", "Hep2" });
        var queryValues = new Dictionary<string, StringValues>
        {
            { name, valueStrings }
        };
        var bindingContext = GetModelBindingContext(name, queryValues);
        var target = CreateTarget();

        // Act
        await target.BindModelAsync(bindingContext);

        // Assert
        Assert.That(bindingContext.Result.IsModelSet, Is.True);
        Assert.That(bindingContext.Result.Model, Is.EqualTo(valueStrings.ToString()));
        Assert.That(bindingContext.ModelState.ContainsKey(name));
        Assert.That(bindingContext.ModelState[name].RawValue, Is.EqualTo(valueStrings));
    }

    private ModelBindingContext GetModelBindingContext(string name, Dictionary<string, StringValues> queryValues)
    {
        var bindingContext = new DefaultModelBindingContext();
        bindingContext.ModelName = name;
        var bindingSource = new BindingSource(name, name, false, false);
        bindingContext.ValueProvider = new QueryStringValueProvider(bindingSource, new QueryCollection(queryValues), CultureInfo.InvariantCulture);
        bindingContext.ModelState = new ModelStateDictionary();

        return bindingContext;
    }

    private EmptyQueryStringBinder CreateTarget()
    {
        return new EmptyQueryStringBinder();
    }

    
}