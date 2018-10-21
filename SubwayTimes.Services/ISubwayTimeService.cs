using SubwayTimes.Models;
using System;

namespace SubwayTimes.Services
{
    public interface ISubwayTimeService
    {
        SubwayInfo UpdateTimes(SubwayLineIds lineId, DateTime lastSince);
    }
}
