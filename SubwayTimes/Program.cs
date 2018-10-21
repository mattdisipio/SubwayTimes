using Microsoft.Extensions.DependencyInjection;
using MTAServiceStatus;

namespace SubwayTimes
{
    class Program
    {
        static void Main(string[] args)
        {
            //Wire up DI and container.
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<MTASubwayStatus>()
                .AddSingleton<SubwayTimeServiceRunner>()
                .BuildServiceProvider();

            //Get an instance of the app
            var app = serviceProvider.GetService<SubwayTimeServiceRunner>();

            app.Run();
        }
    }
}
