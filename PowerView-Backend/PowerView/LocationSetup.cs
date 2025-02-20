using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView
{
    public class LocationSetup : ILocationSetup
    {
        private readonly IServiceProvider serviceProvider;

        public LocationSetup(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void SetupLocation()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ILocationResolver>().ResolveToDatabase();

                var locationProvider = scope.ServiceProvider.GetRequiredService<ILocationProvider>();
                var timeZoneInfo = locationProvider.GetTimeZone();
                var cultureInfo = locationProvider.GetCultureInfo();
                scope.ServiceProvider.GetRequiredService<LocationContext>().Setup(timeZoneInfo, cultureInfo);
            }
        }

    }
}
