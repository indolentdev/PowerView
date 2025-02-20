using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PowerView.Service.Controllers;

public class EmptyQueryStringBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (value != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
            string valueString = value.Values;
            bindingContext.Result = ModelBindingResult.Success(valueString);
        }
        return Task.CompletedTask;
    }
}

