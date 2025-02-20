namespace PowerView.Service.EnergiDataService;

public interface IEnergiDataServiceClient
{
    Task<IList<KwhAmount>> GetElectricityAmounts(DateTime start, TimeSpan timeSpan, string priceArea);
}