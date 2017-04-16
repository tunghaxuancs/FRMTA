using System;
using System.IO;
using System.Linq;

namespace FR.Client
{
    public class Log
    {
        private string fileLogName;
        public Log()
        {
            fileLogName = HelperFeature.pathLogApp + DateTime.Now.Date.ToFileName() + ".log";
        }
        public Log(bool isAppLog)
        {
            if (isAppLog) fileLogName = HelperFeature.pathLogApp + DateTime.Now.Date.ToFileName() + ".log";
            else fileLogName = HelperFeature.pathLogSensor + DateTime.Now.Date.ToFileName() + ".log";
        }
        public string GetFileLogName()
        {
            return fileLogName;
        }
        public void WriteLog(string log)
        {
            string logData = DateTime.Now.ToString() + ": " + log;
            using (StreamWriter sw = new StreamWriter(fileLogName, true))
            {
                sw.WriteLine(logData);
            }
        }
        public string ReadLog(string fileLog)
        {
            string logData = string.Empty;
            using (StreamReader sr = new StreamReader(fileLog))
            {
                logData = sr.ReadToEnd();
            }
            return logData;
        }
        public string ReadLastLineLog()
        {
            return File.ReadLines(fileLogName).Last();
        }
        public void Delete(string fileLog)
        {
            using (StreamWriter sw = new StreamWriter(fileLogName, false))
            {
                sw.WriteLine("");
            }
        }
    }
}