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
        private readonly ISubwayTimeService _subwayTimeService;
        private Timer timer = null;
        private DateTime LastRun = DateTime.UtcNow;
        private ConcurrentDictionary<SubwayLineIds, SortedSet<SubwayInfo>> subwayTimes = new ConcurrentDictionary<SubwayLineIds, SortedSet<SubwayInfo>>();

        public SubwayTimeServiceRunner(ISubwayTimeService subwayTimeService)
        {
            _subwayTimeService = subwayTimeService;
        }

        public void Run()
        {
            timer = new Timer((e) =>
            {
                Console.WriteLine("Updating Times");

                Parallel.ForEach((SubwayLineIds[])Enum.GetValues(typeof(SubwayLineIds)), new ParallelOptions { MaxDegreeOfParallelism = 5 }, lineId =>
                {
                    var model = _subwayTimeService.UpdateTimes(lineId, LastRun);
                    UpdateTimetable(model);
                });

                PipeToConsole();
                LastRun = DateTime.UtcNow;

            }, false, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        }

        private void UpdateTimetable(SubwayInfo item)
        {
            var added = false;

            while (!added)
            {
                if (subwayTimes.ContainsKey(item.LineId))
                {
                    var set = subwayTimes[item.LineId];
                    set.Add(item);
                    return;
                }

                added = subwayTimes.TryAdd(item.LineId, new SortedSet<SubwayInfo> { item });
            }
        }

        private void PipeToConsole()
        {
            foreach(var entry in subwayTimes)
            {
                var history = entry.Value;

                if(history.Count < 2)
                {
                    return;
                }

                var current = history.ElementAt(0);

                var previous = history.ElementAt(1);

                if(current.Status == SubwayStatus.Delayed && previous.Status == SubwayStatus.OnTime)
                {
                    Console.WriteLine($"{entry.Key}'s status has changed to: {SubwayStatus.OnTime}");
                }

                else if (current.Status == SubwayStatus.OnTime && current.Status == SubwayStatus.Delayed)
                {
                    Console.WriteLine($"{entry.Key}'s status has changed to: {SubwayStatus.Delayed}");
                }
            }
        }
    }
}
