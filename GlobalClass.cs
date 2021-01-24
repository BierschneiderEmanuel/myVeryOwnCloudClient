/*myVeryOwnCloud provides a encrypted mySQL based backend that is interfaced by a c# cookie aware web client.
Copyright(C) 2021 Emanuel Bierschneider

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;
using log4net;
using System.Text.RegularExpressions;
using System.Net.Mail;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace WindowsFormsApplication2
{
    class GlobalClass
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public volatile static string DebugLog = "EB";
        public static string lastline_of_log = "";
        private const int LOG_BUF_SIZE = 90000000;
        public static int lines_to_take_from_log { get { return lines_to_take_from_log_var; } set { lines_to_take_from_log_var = value; } }
        public static int lines_to_take_from_log_var = 20;

        public static string ReadEmailReceipt()
        {
            string tempFolderEmailReceipt = Path.GetTempPath();
            tempFolderEmailReceipt += "EmailReceipt.txt";
            return (ReadFileToPath(tempFolderEmailReceipt));
        }
        public static bool TestEmailReceipt()
        {
            string tempFolderEmailReceipt = Path.GetTempPath();
            tempFolderEmailReceipt += "EmailReceipt.txt";
            if (File.Exists(tempFolderEmailReceipt))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public static double ReadDoubleFileToPath(string FilePath)
        {
            string InputString = "";
            double returnvalue = 0.0;

            if (File.Exists(FilePath) == true)
            {
                try
                {
                    InputString = System.IO.File.ReadAllText(FilePath);
                    returnvalue = Convert.ToDouble(InputString);
                }
                catch (Exception ex)
                {
                    Log("### Exception @ ReadDoubleFileToPath ###" + ex.ToString());
                }
            }
            return (returnvalue);
        }

        public static void Log(string input)
        {   //Debug.WriteLine(input);
            log.Debug(input);
            logT(input);
            System.Diagnostics.Debug.WriteLine(input);
        }
        public static List<string> TakeLastLines(string text, int count)
        {
            List<string> lines = new List<string>();
            Match match = Regex.Match(text, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);

            while (match.Success && lines.Count < count)
            {
                lines.Insert(0, match.Value);
                match = match.NextMatch();
            }

            return lines;
        }

        public static void SaveAllOutput(bool UseSQL)
        {
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }

        public static string logT(string input)
        {
            if (DebugLog != null && input != null && (input.Length + DebugLog.Length) < (LOG_BUF_SIZE - 2)) //"\r\n"
            {
                DebugLog += (input + "\r\n");
            }
            else
            {
                 DebugLog = "EB";
            }
            return DebugLog;
        }
        public static string ReadFileToPath(string FilePath)
        {
            string InputString = "";

            if (File.Exists(FilePath) == true)
            {
                try
                {
                    InputString = System.IO.File.ReadAllText(FilePath);
                    InputString = InputString.Replace(System.Environment.NewLine, string.Empty);
                }
                catch (Exception ex)
                {
                    Log("### Exception @ ReadFileToPath ###" + ex.ToString());
                }
            }
            return (InputString);
        }

        public static async void WriteFileToPath(string input_path, string linestowrite)
        {
            string path = input_path;
            // This text is added only once to the file.
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path, true))
                {
                    await file.WriteAsync(linestowrite);
                }
            }
            catch (Exception ex)
            {
                GlobalClass.Log("ERROR @ WriteFileToPath" + ex.ToString());
            }
        }
        public static bool TestPasswordCloud()
        {
            string tempFolderPathApiPub = Path.GetTempPath();
            tempFolderPathApiPub += "PasswordCloud.txt";
            if (File.Exists(tempFolderPathApiPub))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
        public static bool RemovePasswordCloud()
        {
            string tempFolderPathApiPub = Path.GetTempPath();
            tempFolderPathApiPub += "PasswordCloud.txt";
            if (File.Exists(tempFolderPathApiPub))
            {
                File.Delete(tempFolderPathApiPub);
                return (true);
            }
            else
            {
                return (false);
            }
        }
        public static string ReadPasswordCloud()
        {
            string tempFolderApiPub = Path.GetTempPath();
            tempFolderApiPub += "PasswordCloud.txt";
            return (ReadFileToPath(tempFolderApiPub));
        }

        public async static void SavePasswordCloud(string NewApiPub)
        {
            string tempFolderPath = Path.GetTempPath();
            tempFolderPath += "PasswordCloud.txt";
            // This text is added only once to the file.
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(tempFolderPath, false))
                {
                    await file.WriteLineAsync(NewApiPub.ToString());
                }
            }
            catch (Exception ex) { GlobalClass.Log("ERROR @ WriteFileToPath PasswordCloud" + ex.ToString()); }
        }
        public static bool TestSecretCloud()
        {
            string tempFolderPathApiPri = Path.GetTempPath();
            tempFolderPathApiPri += "SecretCloud.txt";
            //if (File.Exists(@"C:\test.txt"))
            if (File.Exists(tempFolderPathApiPri))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
        public static bool RemoveSecretCloud()
        {
            string tempFolderPathApiPri = Path.GetTempPath();
            tempFolderPathApiPri += "SecretCloud.txt";
            if (File.Exists(tempFolderPathApiPri))
            {
                File.Delete(tempFolderPathApiPri);
                return (true);
            }
            else
            {
                return (false);
            }
        }
        public static string ReadSecretCloud()
        {
            string tempFolderApiPri = Path.GetTempPath();
            tempFolderApiPri += "SecretCloud.txt";
            return (ReadFileToPath(tempFolderApiPri));
        }

        public async static void SaveSecretCloud(string NewApiPri)
        {
            string tempFolderPath = Path.GetTempPath();
            tempFolderPath += "SecretCloud.txt";
            // This text is added only once to the file.
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(tempFolderPath, false))
                {
                    await file.WriteLineAsync(NewApiPri);
                }
            }
            catch (Exception ex) { GlobalClass.Log("ERROR @ WriteFileToPath NewApiPri" + ex.ToString()); }
        }
    }
}
