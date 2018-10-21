using System.Collections.Generic;
using transit_realtime;

namespace SubwayTimes.Services
{
    public interface ISubwayTimeService
    {
        IList<FeedEntity> GetTimes();
    }
}
