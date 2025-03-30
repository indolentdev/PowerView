using System;

namespace PowerView.Model.Repository
{
    public interface IObisColorProvider
    {
        string GetColor(ObisCode obisCode);
    }
}

