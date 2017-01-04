using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemonizer
{
    public class FileLogger : LogBase
    {

        private static FileLogger instance;

        public static FileLogger Instance
        {
            get 
            {
                if (instance ==null)
                {
                    instance = new FileLogger();
                }
                return instance;
            }
        }

        override public LogType LoggerType { get { return LogType.File; } }

        public static string LogPath { get; set; }
        public static string LogName { get; set; }
        public static string LogExtension { get; set; }
        public static long MaxLogSize { get; set; }

        static StreamWriter logStream;
        static int logNumber;

        private FileLogger()
        {
            LogExtension = ".log";
            MaxLogSize = 10000000; // 10 MB
            logNumber = 1;

            CreateLog();
        }

        string LogFullPath()
        {
            return Path.Combine(LogPath, LogName + logNumber.ToString() + LogExtension);
        }

        private void CreateLog()
        {
            try
            {
                // Find the last log file number
                while (File.Exists(LogFullPath()))
                {
                    ++logNumber;
                }
                
                // back up one so we are appending to the last log
                if (logNumber > 1)
                    --logNumber;

                FileStream fs = new FileStream(LogFullPath(), FileMode.Append);
                logStream = new StreamWriter(fs);
                logStream.AutoFlush = true;
                created = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to create log file: " + ex.Message);
            }
        }

        override protected void WriteMessage(Level LogLevel, string message, params object[] args)
        {
            try
            {
                logStream.WriteLine(TimeStamp() + String.Format(message, args));

                // Past max length
                if (logStream.BaseStream.Position > MaxLogSize)
                {
                    logStream.Close();
                    logNumber++;

                    FileStream fs = new FileStream(LogFullPath(), FileMode.Append);

                    logStream = new StreamWriter(fs);
                }

            }
            catch (Exception)
            {
                try
                {
                    Console.Error.WriteLine(TimeStamp() + String.Format(message, args));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Bad log message format: " + message + ": " + ex.Message);
                }
            }
        }

    }
}
