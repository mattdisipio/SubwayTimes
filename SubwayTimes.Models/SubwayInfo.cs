using System;

namespace SubwayTimes.Models
{
    public class SubwayInfo : IComparable
    {
        public SubwayLineIds LineId { get; set; }
        public string Status { get; set; }
        public double TripInfoTime { get; set; }

        public int CompareTo(object obj)
        {
            var other = (SubwayInfo)obj;

            if (TripInfoTime < other.TripInfoTime)
            {
                return 1;
            }

            return -1;
        }
    }
}
