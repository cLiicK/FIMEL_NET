using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Fimel.Utils
{
    public class Logger
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static string _bandejaLogs = config["RutaLogs"];

        public static void Log(string texto)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<strong>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</strong><br/>");
                stringBuilder.AppendLine("     " + texto);
                stringBuilder.AppendLine("<br/><br/>");
                if (!Directory.Exists(_bandejaLogs))
                {
                    Directory.CreateDirectory(_bandejaLogs);
                }

                string arg = $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}.log";
                string path = Path.Combine(_bandejaLogs, arg);
                File.AppendAllText(path, stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void Log(string filePrefix, string texto)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<strong>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</strong><br/>");
                stringBuilder.AppendLine("     " + texto);
                stringBuilder.AppendLine("<br/><br/>");
                if (!Directory.Exists(_bandejaLogs))
                {
                    Directory.CreateDirectory(_bandejaLogs);
                }

                string arg = $"{filePrefix}_{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}.log";
                string path = Path.Combine(_bandejaLogs, arg);
                File.AppendAllText(path, stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void LogRuteo(string texto)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<strong>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</strong><br/>");
                stringBuilder.AppendLine("     " + texto);
                stringBuilder.AppendLine("<br/><br/>");
                if (!Directory.Exists(_bandejaLogs))
                {
                    Directory.CreateDirectory(_bandejaLogs);
                }

                string arg = $"seguimiento_{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}.log";
                string path = Path.Combine(_bandejaLogs, arg);
                File.AppendAllText(path, stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void LogLiteral(string texto)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(texto);
                if (!Directory.Exists(_bandejaLogs))
                {
                    Directory.CreateDirectory(_bandejaLogs);
                }

                string arg = $"reg_{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}.log";
                string path = Path.Combine(_bandejaLogs, arg);
                File.AppendAllText(path, stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
