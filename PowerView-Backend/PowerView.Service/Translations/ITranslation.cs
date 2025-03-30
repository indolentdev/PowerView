using System;

namespace PowerView.Service.Translations
{
    public interface ITranslation
    {
        string Get(ResId resId);
        string Get(ResId resId, object arg1);
        string Get(ResId resId, object arg1, object arg2);
        string Get(ResId resId, object arg1, object arg2, object arg3);
        string Get(ResId resId, params object[] args);
    }
}
