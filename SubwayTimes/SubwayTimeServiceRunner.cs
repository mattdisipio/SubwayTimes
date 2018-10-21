using MTAServiceStatus;
using MTAServiceStatus.Models;
using SubwayTimes.Models;
using SubwayTimes.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SubwayTimes
{
    public class SubwayTimeServiceRunner
    {
        private readonly MTASubwayStatus _subwayStatusService;
        private Timer timer = null;
        private DateTime LastRun = DateTime.UtcNow;
        private ConcurrentDictionary<string, SortedSet<SubwayInfo>> subwayTimes = new ConcurrentDictionary<string, SortedSet<SubwayInfo>>();

        public SubwayTimeServiceRunner(MTASubwayStatus subwayStatusService)
        {
            _subwayStatusService = subwayStatusService;
        }

        public void Run()
        {
            Task.Run(() => DoWork());

            while(!(Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                var input = Console.ReadLine();

                var args = input.Split(null);
                if (args.Length != 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Bad number of arguments");
                    Console.ResetColor();
                    return;
                }

                var command = args.First().ToUpper();
                var line = args.Last().ToUpper();

                switch (command)
                {
                    case CommandConstants.Status:
                        var subwayHistory = subwayTimes[line];

                        var info = subwayHistory.First();

                        Console.WriteLine($"{line} is currently {info.LineInfo.Status}");
                        break;
                    case CommandConstants.Uptime:
                        var currentHistory = subwayTimes[line];

                        var totalTime =  DateTime.UtcNow.Subtract(currentHistory.Last().DateForRecord).TotalHours;
                        SubwayInfo prevEntry = null;
                        var TotalTimeDelayed = 0.0;
                        var TotalTimeUp = 0.0;

                        foreach(var entry in currentHistory)
                        {
                            if(prevEntry != null)
                            {
                                if(entry.LineInfo.Status == ServiceStatus.DELAYS)
                                {
                                    TotalTimeDelayed += prevEntry.DateForRecord.Subtract(entry.DateForRecord).TotalHours;
                                    continue;
                                }

                                TotalTimeUp += prevEntry.DateForRecord.Subtract(entry.DateForRecord).TotalHours;
                            }
                        }


                        break;
                    default:
                        LogErrorToConsole($"Bad argument provided for command: {command}");
                        break;
                }
            }
        }

        private  void DoWork()
        {
            timer = new Timer(async (e) =>
            {
                Console.Clear();
                Console.WriteLine("Updating Times");

                var status = new MTASubwayStatus();
                var lines = await status.GetLinesAsync();
                foreach (var line in lines)
                {
                    UpdateTimetable(line);
                }

                PipeToConsole();

            }, false, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private void UpdateTimetable(SubwayLine item)
        {
            var added = false;

            while (!added)
            {
                var newItem = new SubwayInfo
                {
                    LineId = item.Name,
                    LineInfo = item,
                    DateForRecord = DateTime.UtcNow
                };

                if (subwayTimes.ContainsKey(item.Name))
                {
                    var set = subwayTimes[item.Name];

                   

                    set.Add(newItem);
                    return;
                }

                added = subwayTimes.TryAdd(item.Name, new SortedSet<SubwayInfo> { newItem });
            }
        }


        private void PipeToConsole()
        {
            foreach(var entry in subwayTimes)
            {
                var history = entry.Value;

                if(history.Count < 2)
                {
                    Console.WriteLine($"{entry.Key}'s current status is: {entry.Value.First().LineInfo.Status}");
                    continue;
                }

                var current = history.ElementAt(0);

                var previous = history.ElementAt(1);

                if(current.LineInfo.Status == ServiceStatus.DELAYS && previous.LineInfo.Status != ServiceStatus.DELAYS)
                {
                    Console.WriteLine($"{entry.Key}'s status has changed to: {SubwayStatus.OnTime}");
                }

                else if (current.LineInfo.Status != ServiceStatus.DELAYS && current.LineInfo.Status == ServiceStatus.DELAYS)
                {
                    Console.WriteLine($"{entry.Key}'s status has changed to: {SubwayStatus.Delayed}");
                }
            }
        }

        private void LogErrorToConsole(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }


    }
}
