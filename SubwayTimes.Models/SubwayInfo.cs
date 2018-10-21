using MTAServiceStatus.Models;
using System;

namespace SubwayTimes.Models
{
    public class SubwayInfo : IComparable
    {
        public string LineId { get; set; }
        public SubwayLine LineInfo { get; set; }
        public DateTime DateForRecord { get; set; }

        public int CompareTo(object obj)
        {
            var other = (SubwayInfo)obj;

            if (DateForRecord < other.DateForRecord)
            {
                return 1;
            }

            return -1;
        }
    }
}
