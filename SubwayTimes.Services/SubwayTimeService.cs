using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Net;
using transit_realtime;

namespace SubwayTimes.Services
{
    public class SubwayTimeService : ISubwayTimeService
    {
        private readonly string ApiKey = "2d9c8161496671ce89eab36d7745cf0c";
        private readonly string RealTimeFeedUrl = "http://datamine.mta.info/mta_esi.php?key={0}&feed_id=1";

        public IList<FeedEntity> GetTimes()
        {
            var req = WebRequest.Create(string.Format(RealTimeFeedUrl, ApiKey));
            FeedMessage feed = Serializer.Deserialize<FeedMessage>(req.GetResponse().GetResponseStream());

            return feed.entity;
            
        }
    }
}
