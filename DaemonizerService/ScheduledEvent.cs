using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemonizer
{
    public class ScheduledEvent
    {
        public DayOfWeek StartDayOfTheWeek { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public DayOfWeek EndDayOfTheWeek { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public bool AutoRestart { get; set; }
        public string CommandLine { get; set; }
    }
}
