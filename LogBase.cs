using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemonizer
{
    abstract public class LogBase
    {
        public enum Level : int
        {
            Error,
            Warn,
            Info,
            Debug
        }

        public Level LogLevel { get; set; }

        protected static bool created;

        protected string TimeStamp()
        {
            {
                return DateTime.Now.ToShortDateString()
                    + " " + DateTime.Now.ToShortTimeString() + ":";
            }
        }

        abstract protected void WriteMessage(Level LogLevel, string message, params object[] args);
        public void Debug(string message, params object[] args)
        {
            if (LogLevel <= Level.Debug)
            {
                WriteMessage(Level.Debug, message, args);
            }
        }
        public void Info(string message, params object[] args)
        {
            if (LogLevel <= Level.Info)
            {
                WriteMessage(Level.Info, message, args);
            }
        }
        public void Warn(string message, params object[] args)
        {
            if (LogLevel <= Level.Warn)
            {
                WriteMessage(Level.Warn, message, args);
            }
        }
        public void Error(string message, params object[] args)
        {
            if (LogLevel <= Level.Error)
            {
                WriteMessage(Level.Error, message, args);
            }
        }
    }
}
