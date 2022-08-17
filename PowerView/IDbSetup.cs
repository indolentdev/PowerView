using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace PowerView
{
    public interface IDbSetup
    {
        void SetupDatabase();
    }
}
