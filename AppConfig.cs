using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Daemonizer
{
    public class AppConfig
    {
        public string BaseDirectory { get; set; }
        public string LogDirectoryName { get; set; }
        public string ConfigDirectoryName { get; set; }

        public static AppConfig Load(string company, string app)
        {
            AppConfig config = null;
            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    company,
                    app);
                
                if (Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string file = Path.Combine(path, "config.json");

                if (File.Exists(file))
                {
                    config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(file));
                }
                else
                {
                    config = new AppConfig();
                    config.BaseDirectory = path;
                    config.LogDirectoryName = "logs";
                    config.ConfigDirectoryName = "configs";
                    Save(config, file);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error loading config file", ex);
            }

            return config;
        }

        public static void Save(AppConfig config, string file)
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
