using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PowerView.Service.Controllers;

public class EmptyQueryStringBinderProvider : IModelBinderProvider
{
    private readonly EmptyQueryStringBinder emptyQueryStringBinder = new EmptyQueryStringBinder();

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (BindingSource.Query == context.BindingInfo.BindingSource && context.Metadata.ModelType == typeof(string))
        {
            return emptyQueryStringBinder;
        }
        return null;
    }
}

