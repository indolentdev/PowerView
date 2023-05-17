using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub;

internal interface IEnergiDataServiceImport
{
    Task Import(IServiceScope serviceScope, DateTime dateTime);
}
