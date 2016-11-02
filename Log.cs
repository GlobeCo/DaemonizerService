using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemonizer
{
    public static class Log
    {
        public enum Level : int
        {
            Debug,
            Info,
            Warn,
            Error
        }

        public static string LogPath { get; set; }
        public static string LogName { get; set; }
        public static string LogExtension { get; set; }
        public static Level LogLevel { get; set; }
        public static long MaxLogSize { get; set; }

        static bool created;
        static StreamWriter logStream;
        static int logNumber;

        static Log()
        {
            LogPath = Path.GetTempPath();
            LogName = "application";
            LogExtension = ".log";
            LogLevel = Level.Info;
            MaxLogSize = 10000000; // 10 MB
            logNumber = 1;
        }

        static string LogFullPath()
        {
            return Path.Combine(LogPath, LogName + logNumber.ToString() + LogExtension);
        }

        static void CreateLog()
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

        static string TimeStamp()
        {
            return DateTime.Now.ToShortDateString() 
                + " " + DateTime.Now.ToShortTimeString() + ":" ;
        }

        static void WriteMessage(string message, params object[] args)
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

        public static void Debug(string message, params object[] args)
        {
            if (LogLevel >= Level.Debug)
            { 
                if (!created)
                {
                    CreateLog();
                }

                WriteMessage(message, args);
            }
        }

        public static void Info(string message, params object[] args)
        {
            if (LogLevel >= Level.Info)
            {
                if (!created)
                {
                    CreateLog();
                }

                WriteMessage(message, args);
            }
        }

        public static void Warn(string message, params object[] args)
        {
            if (LogLevel >= Level.Warn)
            {
                if (!created)
                {
                    CreateLog();
                }

                WriteMessage(message, args);
            }
        }

        public static void Error(string message, params object[] args)
        {
            if (!created)
            {
                CreateLog();
            }

            WriteMessage(message, args);   
        }
    }
}
