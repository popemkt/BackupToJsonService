using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1
{
    class LoggingService
    {
        static public void WriteToLog(string message)
        {
            string logLocation = ConfigurationManager.AppSettings.Get("LogFileLocation");
            try
            {
                string directory = @logLocation.Replace(@logLocation.Split('\\').Last(), "");
                Directory.CreateDirectory(directory);
                File.AppendAllLines(@logLocation, new string[] { DateTime.Now + ": " + message });
            }
            catch (Exception) { }
        }
    }
}
