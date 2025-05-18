using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utilities
{
	public static class Logger
	{
		public static void LogMessage(string Message)
		{
			try
			{
                string logpath=AppDomain.CurrentDomain.BaseDirectory + "//" + AppDomain.CurrentDomain.FriendlyName + "ErrorLog.txt";
                StreamWriter writer = new StreamWriter(logpath, true);
				writer.WriteLine("{0}:{1}", DateTime.Now.ToString(), Message);
				writer.Close();
			}
			catch
			{

			}
		}

        public static void LogEvent(string Event)
        {
            try
            {

                StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "//" + AppDomain.CurrentDomain.FriendlyName + "EventLog.txt", true);
                writer.WriteLine("{0}:{1}", DateTime.Now.ToString(), Event);
                writer.Close();
            }
            catch
            {

            }
        }

	}
}
