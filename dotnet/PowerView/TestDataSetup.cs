using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView
{
    public class TestDataSetup : ITestDataSetup
    {
        private readonly IServiceProvider serviceProvider;

        public TestDataSetup(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void SetupTestData()
        {
#if DEBUG            
            using (var scope = serviceProvider.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

                var testDataGenerator = new TestDataGenerator(loggerFactory.CreateLogger<TestDataGenerator>(), scope);
                testDataGenerator.Generate();
            }
#endif            
        }

    }
}
