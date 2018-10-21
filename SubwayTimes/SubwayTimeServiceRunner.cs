using SubwayTimes.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubwayTimes
{
    public class SubwayTimeServiceRunner
    {
        private readonly ISubwayTimeService _subwayTimeService;

        public SubwayTimeServiceRunner(ISubwayTimeService subwayTimeService)
        {
            _subwayTimeService = subwayTimeService;
        }

        public void Run()
        {
            var subwayTimes = _subwayTimeService.GetTimes();
        }
    }
}
