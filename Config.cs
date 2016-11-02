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
            Config config = null;
            try
            {
                if (File.Exists(file))
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
                }
                else
                {
                    Log.Warn("Config file not found");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error loading config file: {0}", ex);
            }

            return config;
        }

        public static void Save(Config config, string file)
        {
            try
            {
                string output = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(file, output);
            }
            catch (Exception ex)
            {
                Log.Error("Error saving config file", ex);
            }
        }
    }
}
