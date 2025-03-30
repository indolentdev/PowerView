using System;

namespace PowerView.Model.Repository
{
    internal interface IEntityDeserializer
    {
        TType GetValue<TType>(params string[] path);
    }
}

