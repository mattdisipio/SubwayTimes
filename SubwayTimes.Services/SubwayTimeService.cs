using ProtoBuf;
using SubwayTimes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using transit_realtime;

namespace SubwayTimes.Services
{
    public class SubwayTimeService : ISubwayTimeService
    {
        private readonly string ApiKey = "2d9c8161496671ce89eab36d7745cf0c";
        private readonly string OneLine = "http://datamine.mta.info/mta_esi.php?key={0}&feed_id=1";
        private readonly string ACEHLine = "http://datamine.mta.info/mta_esi.php?key={0}&feed_id=26";
        private DateTime LastUpdate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));

        public SubwayInfo UpdateTimes(SubwayLineIds lineId, DateTime lastSince)
        {
            var req = WebRequest.Create(string.Format(OneLine, ApiKey));
            var feed = Serializer.Deserialize<FeedMessage>(req.GetResponse().GetResponseStream());

            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = LastUpdate - origin;
            var millis = Math.Floor(diff.TotalSeconds);

            var ret = feed.entity.Where(x => x.vehicle != null && x.vehicle.timestamp > millis).ToList();

            
            var model = new SubwayInfo
            {
                LineId = lineId,
                Status = ret.Any(x => x.alert == null) ? SubwayStatus.OnTime : SubwayStatus.Delayed,
                TripInfoTime = millis
            };


            return model;
            
        }
    }
}
