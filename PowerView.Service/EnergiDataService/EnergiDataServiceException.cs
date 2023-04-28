using System;

namespace PowerView.Service.EnergiDataService;

public class EnergiDataServiceClientException : Exception
{
    public EnergiDataServiceClientException()
    {
    }

    public EnergiDataServiceClientException(string message)
      : base(message)
    {
    }

    public EnergiDataServiceClientException(string message, Exception inner)
      : base(message, inner)
    {
    }
}
