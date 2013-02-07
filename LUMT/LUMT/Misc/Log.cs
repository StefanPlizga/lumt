// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Text;
using System.IO;
using LUMT.App;

namespace LUMT.Misc
{
    class Log
    {
        private static TextWriter logFile;
        private static bool logFileOpen = false;

        public static void LogFileInit()
        {
            try
            {
                logFile = new StreamWriter(String.Format("{0}-{1}-{2}.txt", LumtGlobals.ApplicationShortName, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss")), false);
                logFileOpen = true;
            }
            catch (Exception ex)
            {
                WriteErrorEventOnScreen(String.Format("Log File Init error: {0}", ex.Message));
            }
        }

        public static void LogFileClose()
        {
            try
            {
                if (logFileOpen)
                {
                    logFile.Flush();
                    logFile.Close();
                    logFileOpen = false;
                }
            }
            catch (Exception ex)
            {
                WriteErrorEventOnScreen(String.Format("Log File Close error: {0}", ex.Message));
            }

        }

        private static void WriteErrorEventOnScreen(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Application is being terminated.");
            Environment.Exit(1);
        }

        public static void WriteLogEntry(string action, string fileMessage, string consoleMessage = "")
        {
            try
            {
                if (logFile != null)
                {
                    DateTime now = DateTime.Now;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(now.ToString("yyyy"));
                    sb.Append("-");
                    sb.Append(now.ToString("MM"));
                    sb.Append("-");
                    sb.Append(now.ToString("dd"));
                    sb.Append(" ");
                    sb.Append(now.ToString("HH:mm:ss"));
                    sb.Append("\t");
                    sb.Append(action);
                    sb.Append("\t");
                    sb.Append(fileMessage);
                    logFile.WriteLine(sb.ToString());
                    logFile.Flush();

                    if (consoleMessage.Length > 0)
                    {
                        Console.WriteLine(consoleMessage);
                    }
                }
                else
                {
                    WriteErrorEventOnScreen(String.Format("The log file is not initialized"));
                }
            }
            catch (Exception e)
            {
                WriteErrorEventOnScreen(String.Format("WriteLogFileEntry error: {0}", e.Message));
            }
        }
    }
}
