using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemonizer
{
    public class EventLogger : LogBase
    {
        private static EventLogger instance;

        public static EventLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventLogger();
                }
                return instance;
            }
        }

        public static string sSource { get; set; }
        public static string sLog { get; set; }

        static EventLogger()
        {
            sSource = "Daemonizer";
            sLog = "Application";
        }

        private EventLogger()
        {
            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);
        }

        private EventLogEntryType getEventLogEntryType(Level LogLevel)
        {
            switch (LogLevel)
            {
                case Level.Debug:
                case Level.Info:
                default:
                    return EventLogEntryType.Information;
                case Level.Warn:
                    return EventLogEntryType.Warning;
                case Level.Error:
                    return EventLogEntryType.Error;
            }
        }

        override protected void WriteMessage(Level LogLevel, string message, params object[] args)
        {
            EventLog.WriteEntry(sSource, string.Format(message, args), getEventLogEntryType(LogLevel));
        }
    }
}
