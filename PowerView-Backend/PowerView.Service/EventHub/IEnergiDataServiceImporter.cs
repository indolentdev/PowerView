namespace PowerView.Service.EventHub;

public interface IEnergiDataServiceImporter
{
    Task Import(DateTime timestamp);
}