using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using System.IO;

namespace Daemonizer
{
    public partial class ProcessService : ServiceBase, IServiceProcess
    {
        class RunningProcess
        {
            public Process Process { get; set; }
            public ScheduledEvent Event { get; set; }
            public bool Exited { get; set; }
        }

        class PendingEvent
        {
            public Config EventConfig { get; set; }
            public ScheduledEvent Event { get; set; }
        }

        const int TIMER_INTERVAL_MS = 30000;
        const int WAIT_FOR_EXIT_MS = 5000;

        List<Config> configs = new List<Config>();
        List<RunningProcess> processes = new List<RunningProcess>();
        Timer timer;
        DayOfWeek dayOfTheWeek;
        List<PendingEvent> eventItems = new List<PendingEvent>();
        AppConfig appConfig;

        //FileLogger logger;
        EventLogger logger;
        FileSystemWatcher watcher;

        public ProcessService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {       
            dayOfTheWeek = DateTime.Now.DayOfWeek;
            
            appConfig = AppConfig.Load("GlobeCo", "Daemonizer");
            string appDataPath = appConfig.BaseDirectory;
            string logDirectory = Path.Combine(appDataPath, appConfig.LogDirectoryName);
            string configDirectory = Path.Combine(appDataPath, appConfig.ConfigDirectoryName);

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);


            //FileLogger.LogPath = logDirectory;
            //FileLogger.LogName = "daemonizer"; // extension is set automatically
            //FileLogger.LogLevel = FileLogger.Level.Info;

           // logger = FileLogger.Instance;

            logger = EventLogger.Instance;

            LoadNewSchedule();
            EvaluateSchedule();
            
            timer = new Timer();
            timer.Interval = TIMER_INTERVAL_MS;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            BeginWatch();

            logger.Info("Daemonizer service started");
        }

        protected override void OnStop()
        {
            foreach(RunningProcess rp in processes)
            {
                if (!rp.Exited)
                    StopProcess(rp.Process);    
            }

            EndWatch();

            logger.Info("Daemonizer service stopped");
        }

        Process StartProcess(Config config, ScheduledEvent schEvent)
        {
            logger.Info(String.Format("Starting process {0} {1}", 
                config.ExeName, schEvent.CommandLine));

            Process process = new Process();
            process.Exited += Process_Exited;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.StartInfo.UseShellExecute = config.UseShellExecute;
            process.StartInfo.CreateNoWindow = config.CreateNoWindow;
            process.StartInfo.FileName = config.ExeName;
            process.StartInfo.Arguments = schEvent.CommandLine;
            process.StartInfo.WorkingDirectory = config.WorkingDirectory;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            if (!process.Start())
            {
                logger.Error(String.Format("Failed to start process {0}", config.ExeName));
                return null;
            }
            else
            {
                return process;
            }
        }

        void StopProcess(Process process)
        {
            logger.Info(String.Format("Stopping process {0}", process.ProcessName));
            process.CloseMainWindow();
            process.WaitForExit(WAIT_FOR_EXIT_MS);
            if (!process.HasExited)
            {
                logger.Warn("Process did not exit, killing process!");
                process.Kill();
                process.WaitForExit();
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Process process = (Process)sender;

            logger.Info(String.Format("Process {0} has exited with code {1}", 
                process.ProcessName, process.ExitCode));

            for (int inx = 0; inx < processes.Count; inx++)
            {
                if (processes[inx].Process == process)
                {
                    processes[inx].Exited = true;
                    break;
                }
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Debug(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Error(e.Data);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            
            // Check all scheduled items
            EvaluateSchedule();

            // Check all running processes
            EvaluateProcesses();
            timer.Enabled = true;
        }

        private void LoadNewSchedule()
        {
            eventItems.Clear();

            logger.Info("Stopping previous running processes");
            // Stop any running processes
            foreach (RunningProcess rp in processes)
            {
                StopProcess(rp.Process);
            }

            string configDirectory = Path.Combine(appConfig.BaseDirectory, appConfig.ConfigDirectoryName);
            DirectoryInfo info = new DirectoryInfo(configDirectory);

            configs.Clear();
            foreach (FileInfo fileInfo in info.GetFiles())
            {
                if (fileInfo.Extension == ".conf")
                {
                    Config config = Config.Load(fileInfo.FullName);
                    if (config != null)
                    {
                        configs.Add(config);
                        logger.Info(String.Format("Loaded {0}", fileInfo.Name));
                    }
                }
            }

            logger.Info("Loading new schedule");
            // Load new daily schedule
            foreach (Config config in configs)
            {
                foreach (ScheduledEvent schEvent in config.Schedule)
                {
                    PendingEvent pe = new PendingEvent
                    {
                        EventConfig = config,
                        Event = schEvent
                    };
                    eventItems.Add(pe);   
                }
            }
        }

        private void EvaluateSchedule()
        {
            var time = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var purgeList = new List<PendingEvent>();

            foreach(PendingEvent pendEvent in eventItems)
            {
                if (DateTime.Now.DayOfWeek == pendEvent.Event.DayOfTheWeek)
                {
                    var startTime = pendEvent.Event.StartHour * 60 + pendEvent.Event.StartMinute;
                    var endTime = pendEvent.Event.EndHour * 60 + pendEvent.Event.EndMinute;
                    if (time >= startTime && time <= endTime)
                    {
                        Process process = StartProcess(pendEvent.EventConfig, pendEvent.Event);

                        if (process != null)
                        {
                            RunningProcess rp = new RunningProcess
                            {
                                Process = process,
                                Event = pendEvent.Event,
                                Exited = false
                            };
                            processes.Add(rp);
                            purgeList.Add(pendEvent);
                        }
                    }
                }
            }

            // Remove all completed items for the scheduled events
            foreach(PendingEvent pendEvent in purgeList)
            {
                eventItems.Remove(pendEvent);
            }
        }

        void EvaluateProcesses()
        {
            var time = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var purgeList = new List<RunningProcess>();

            foreach (RunningProcess rp in processes)
            {
                var endTime = rp.Event.EndHour * 60 + rp.Event.EndMinute;

                // Past time or next day
                if (time > endTime)
                {
                    if (!rp.Exited)
                    {
                        StopProcess(rp.Process);
                    }
                    else
                    {
                        purgeList.Add(rp);
                    }
                }
                else if (rp.Exited) // application has terminated
                {
                    if (rp.Event.AutoRestart)
                    {
                        // Restart process or remove from list
                        if (rp.Process.Start())
                            rp.Exited = false;
                        else
                            purgeList.Add(rp);
                    }
                    else
                        purgeList.Add(rp);
                }
            }

            // Remove all exited processes
            foreach (RunningProcess rp in purgeList)
            {
                processes.Remove(rp);
            }
        }

        private void BeginWatch()
        {
            if (watcher == null)
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Path.Combine(appConfig.BaseDirectory, appConfig.ConfigDirectoryName);
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                watcher.Filter = "*.conf";
                watcher.Changed += new FileSystemEventHandler(OnWatchChanged);
                watcher.Created += new FileSystemEventHandler(OnWatchChanged);
            }
            watcher.EnableRaisingEvents = true;
        }

        private void EndWatch()
        {
            watcher.EnableRaisingEvents = false;
        }

        private void OnWatchChanged(object source, FileSystemEventArgs e)
        {
            // Conf file change detected, do appropriate things here.
            logger.Info("Config file change detected in file {0}", e.Name);
            LoadNewSchedule();
        }

        public void StartService(string[] args)
        {
            OnStart(args);
        }

        public void StopService()
        {
            OnStop();
        }
    }
}
