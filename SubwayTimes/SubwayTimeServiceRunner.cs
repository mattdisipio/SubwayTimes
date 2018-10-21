using MTAServiceStatus;
using MTAServiceStatus.Models;
using SubwayTimes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SubwayTimes
{
    public class SubwayTimeServiceRunner
    {
        private readonly MTASubwayStatus _subwayStatusService;
        private Timer timer = null;
        private Dictionary<string, SortedSet<SubwayInfo>> subwayTimes = new Dictionary<string, SortedSet<SubwayInfo>>();

        public SubwayTimeServiceRunner(MTASubwayStatus subwayStatusService)
        {
            _subwayStatusService = subwayStatusService;
        }

        /// <summary>
        /// Main runner for the application
        /// </summary>
        public void Run()
        {
            ShowCommands();

            Task.Run(() => GetMtaInfo());

            WaitForInput();           
        }

        /// <summary>
        /// Waits for input once the function that grabs the mta info every 30 seconds has been setup. Based on the input, runs logic giving the user what they request.
        /// </summary>
        private void WaitForInput()
        {
            while (true)
            {
                var input = Console.ReadLine();
                var args = input.Split(null);
                var command = args.First().ToUpper();

                if (!CommandConstantsArray.Contains(command))
                {
                    LogErrorToConsole($"Command {command} does not exist! Type info for a list of valid commands.");
                    continue;
                }

                if (command != CommandConstants.Info && command != CommandConstants.Quit && args.Length != 2)
                {
                    LogErrorToConsole($"Bad Number of arguments for command: {command}");
                    continue;
                }

                var subwayLine = args.Last().ToUpper();

                switch (command)
                {
                    case CommandConstants.Quit:
                        Environment.Exit(0);
                        break;
                    case CommandConstants.Info:
                        ShowCommands();
                        break;
                    //Because we use a sortedset, we just need to grab the most recent item for the line the user enters and display the status.
                    case CommandConstants.Status:
                        if (!subwayTimes.ContainsKey(subwayLine))
                        {
                            LogErrorToConsole($"No data for line: {subwayLine}");
                            continue;
                        }

                        var subwayHistory = subwayTimes[subwayLine];

                        var info = subwayHistory.First();

                        Console.WriteLine($"{subwayLine} is currently {info.GetStatus()}");
                        break;
                    //similarly for uptime, we first retrieve the last item in the sorted set which would be the oldest. We keep a hold of that date to determine
                    // how long we've been collecting data. We iterate through the rest of the set, calculating time between entries. 
                    //If the entry is delayed, we calculate the time span
                    //that the entry was in that status for, adding it to the overall delayed time and presenting the uptime at the end.
                    case CommandConstants.Uptime:
                        if (!subwayTimes.ContainsKey(subwayLine))
                        {
                            LogErrorToConsole($"No data for line: {subwayLine}");
                            continue;
                        }
                        var currentHistory = subwayTimes[subwayLine];
                        var dateTimeForRequest = DateTime.UtcNow;

                        var totalTime = dateTimeForRequest.Subtract(currentHistory.Last().DateForRecord).TotalHours;
                        SubwayInfo prevEntry = null;
                        var TotalTimeDelayed = 0.0;

                        foreach (var entry in currentHistory)
                        {
                            if (prevEntry != null)
                            {
                                if (entry.LineInfo.Status == ServiceStatus.DELAYS)
                                {
                                    TotalTimeDelayed += prevEntry.DateForRecord.Subtract(entry.DateForRecord).TotalHours;
                                    continue;
                                }
                            }

                            prevEntry = entry;

                            if (entry.LineInfo.Status == ServiceStatus.DELAYS)
                            {
                                TotalTimeDelayed += dateTimeForRequest.Subtract(entry.DateForRecord).TotalHours;
                                continue;
                            }

                        }

                        //total time should never be zero, that's why there isn't a check here.
                        var timeDelayed = ((1 - (TotalTimeDelayed / totalTime)) * 100).ToString("0.##"); ;

                        Console.WriteLine($"Total uptime: {timeDelayed}%");

                        break;
                    default:
                        LogErrorToConsole($"Bad argument provided for command: {command}");
                        break;
                }
            }
        }

        /// <summary>
        /// Shows the commands available to the program.
        /// </summary>
        private void ShowCommands()
        {
            Console.Clear();
            Console.WriteLine("Subway Service Monitor Commands");
            Console.WriteLine("This application will monitor the MTA status page and alert the user to any delays for a certain line");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("uptime [line]");
            Console.WriteLine("Gets the uptime of the provided line from the when you started running the program. Will display to the user a percentage of uptime.");
            Console.WriteLine();
            Console.WriteLine("status [line]");
            Console.WriteLine("Gets the current status of the provided line from the MTA.");
            Console.WriteLine();
            Console.WriteLine("info");
            Console.WriteLine("Shows this command dialog.");
            Console.WriteLine();
            Console.WriteLine("quit");
            Console.WriteLine("Quits the application.");
            Console.WriteLine();
        }

        /// <summary>
        /// Uses the <see cref="MTAServiceStatus"/> plugin to get mta info from the status page for subway lines.
        /// </summary>
        private void GetMtaInfo()
        {
            timer = new Timer(async (e) =>
            {
                var lines = await _subwayStatusService.GetLinesAsync();
                foreach (var line in lines)
                {
                    UpdateTimetable(line);
                }

                PipeToConsole();

            }, false, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Takes a subway line item and adds it to the dictionary. If the item exists, it adds it to that entries sorted set. If the entry does not, it creates
        /// that entry in the dictionary.
        /// </summary>
        /// <param name="item"><see cref="SubwayLine"/></param>
        private void UpdateTimetable(SubwayLine item)
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

            subwayTimes.Add(item.Name, new SortedSet<SubwayInfo> { newItem });
        }

        /// <summary>
        /// Writes any relevant status changes to the console.
        /// </summary>
        private void PipeToConsole()
        {
            foreach(var entry in subwayTimes)
            {
                var history = entry.Value;

                if(history.Count < 2)
                {
                    continue;
                }

                var current = history.ElementAt(0);

                var previous = history.ElementAt(1);

                if(current.LineInfo.Status == ServiceStatus.DELAYS && previous.LineInfo.Status != ServiceStatus.DELAYS)
                {
                    Console.WriteLine($"Line {entry.Key}'s status has changed from {SubwayStatus.OnTime} to {SubwayStatus.Delayed}");
                }

                else if (current.LineInfo.Status != ServiceStatus.DELAYS && previous.LineInfo.Status == ServiceStatus.DELAYS)
                {
                    Console.WriteLine($"Line {entry.Key}'s status has changed from {SubwayStatus.Delayed} to {SubwayStatus.OnTime}");
                }
            }
        }

        /// <summary>
        /// Logs any errors to the console in red so they alert the user.
        /// </summary>
        /// <param name="message"></param>
        private void LogErrorToConsole(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

      
        /// <summary>
        /// An array of the all the items in <see cref="CommandConstants"/> - we use this to check whether the command entered is valid.
        /// </summary>
        private string[] CommandConstantsArray = typeof(CommandConstants).GetFields(BindingFlags.Public | BindingFlags.Static |
            BindingFlags.FlattenHierarchy)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
        .Select(x => (string)x.GetRawConstantValue()).ToArray();
    }
}
