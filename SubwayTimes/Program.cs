using Microsoft.Extensions.DependencyInjection;
using System;
using MTAServiceStatus;

namespace SubwayTimes
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<MTASubwayStatus>()
                .AddSingleton<SubwayTimeServiceRunner>()
                .BuildServiceProvider();

            var app = serviceProvider.GetService<SubwayTimeServiceRunner>();

            app.Run();

            Console.ReadKey();
        }
    }
}
