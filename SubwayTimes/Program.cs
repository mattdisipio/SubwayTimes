using Microsoft.Extensions.DependencyInjection;
using System;
using SubwayTimes.Services;

namespace SubwayTimes
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ISubwayTimeService, SubwayTimeService>()
                .AddSingleton<SubwayTimeServiceRunner>()
                .BuildServiceProvider();

            var app = serviceProvider.GetService<SubwayTimeServiceRunner>();

            app.Run();

            Console.ReadKey();
        }
    }
}
