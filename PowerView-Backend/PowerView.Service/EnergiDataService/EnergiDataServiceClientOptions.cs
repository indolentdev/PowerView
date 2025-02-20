using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PowerView.Service.EnergiDataService;

public class EnergiDataServiceClientOptions : IOptions<EnergiDataServiceClientOptions>
{
    public Uri BaseUrl { get; set; } = new Uri("https://api.energidataservice.dk/dataset/");

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(240);
    EnergiDataServiceClientOptions IOptions<EnergiDataServiceClientOptions>.Value => this;
}