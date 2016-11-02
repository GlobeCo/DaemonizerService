using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Daemonizer
{
    public class Config
    {
        public string ExeName { get; set; }
        public string WorkingDirectory { get; set; }
        public bool UseShellExecute { get; set; }
        public bool CreateNoWindow { get; set; }
  
        public IList<ScheduledEvent> Schedule { get; set; }

        public static Config Load(string file)
        {
            EventLogger logger = EventLogger.Instance;

            Config config = null;
            try
            {
                if (File.Exists(file))
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
                }
                else
                {
                    logger.Warn("Config file not found");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error loading config file: {0}", ex);
            }

            return config;
        }

        public static void Save(Config config, string file)
        {
            EventLogger logger = EventLogger.Instance;

            try
            {
                string output = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(file, output);
            }
            catch (Exception ex)
            {
                logger.Error("Error saving config file", ex);
            }
        }
    }
}
