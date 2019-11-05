using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace WindowsService1
{
    class FileService
    {
        static public bool WriteToJson(IEnumerable<Person> personList)
        {
            if (personList.IsNullOrEmpty())
            {
                LoggingService.WriteToLog("List is null or empty");
                return false;
            }
            string fileLocation = ConfigurationManager.AppSettings.Get("ReplicaFileLocation");
            string json;
            try
            {
                if (File.Exists(@fileLocation))
                {
                    json = File.ReadAllText(@fileLocation);
                }
                else
                {
                    Directory.CreateDirectory(@fileLocation.Replace(@fileLocation.Split('\\').Last(),""));
                    FileStream fileStream = File.Create(@fileLocation);
                    fileStream.Close();
                    json = @"[]";
                }
                JArray jsonObj = JArray.Parse(json);
                foreach (Person person in personList)
                {
                    person.isWritten = true;
                    jsonObj.Add(((JObject)JToken.FromObject(person)).ToString());
                }
                File.WriteAllText(@fileLocation, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                LoggingService.WriteToLog(ex.Message);
                return false;
            }
            return true;
        }
    }
}
