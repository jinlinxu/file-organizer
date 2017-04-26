using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MDOrganizer
{
    class Logger
    {
        private StreamWriter errorLogger = null;
        private StreamWriter logger = null;
        private long errorCount = 0;
        private static Logger _instance = null;

        private Logger(string logDir)
        {
            string errorPath = "error.err";
            string logPath = "conversion.log";
            if (!string.IsNullOrEmpty(logDir))
            {
                if (!Directory.Exists(logDir))
                {
                    Console.WriteLine("Creating Log directory");
                    Directory.CreateDirectory(logDir);
                }
                errorPath = System.IO.Path.Combine(logDir, errorPath);
                logPath = System.IO.Path.Combine(logDir, logPath);
            }
            errorLogger = new StreamWriter(errorPath);
            logger = new StreamWriter(logPath);
        }

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger(Logger.Path);
                }
                return _instance;
            }
        }

        public static string Path
        {
            get;
            set;
        }
        public void LogError(string message)
        {
            ++errorCount;
            string msg = "Error : " + message;
            Console.WriteLine(msg);
            errorLogger.WriteLine(msg);
            errorLogger.Flush();
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
            logger.WriteLine(message);
            logger.Flush();
        }

        public void Close()
        {
            Console.WriteLine("************Number of Errors = " + errorCount);
            Console.WriteLine("Error details are present at your target directoy");
            errorLogger.Close();
            logger.Close();
        }
    }
}
