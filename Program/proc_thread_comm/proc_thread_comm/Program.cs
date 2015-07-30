using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace proc_thread_comm
{
    class Program
    {
        static void Main(string[] args)
        {
            //show the parameter of the program
            Help();

            Hashtable runArgs = new Hashtable();
            runArgs.Add("rabbitMQ_Server", "localhost");
            runArgs.Add("ProcessName", "..\\..\\..\\..\\ProcessClass\\ProcessClass\\bin\\Release\\ProcessClass.exe");
            runArgs.Add("T2P_Chan", "QT2PChan");
            runArgs.Add("P2T_Chan", "QP2TChan");

            if (!ParseArgs(runArgs, args)) {
                return;
            }

            //check if runArgs is legal or not
            if(!CheckIfLegalArgs(runArgs)) {
                return;
            }

            object value = runArgs["ProcessName"];
            string ProcessName = value.ToString();

            value = runArgs["rabbitMQ_Server"];
            string rabbitMQ_Server = value.ToString();

            value = runArgs["T2P_Chan"];
            string T2P_Chan = value.ToString();

            value = runArgs["P2T_Chan"];
            string P2T_Chan = value.ToString();

            //start T
            var threadInstance = new ThreadClass(rabbitMQ_Server, T2P_Chan, P2T_Chan);
            threadInstance.Config();
            //Thread T = new Thread(() => threadInstance.Start("30"));
            Thread T = new Thread(() => threadInstance.Start());
            T.Start();

            //start P
            Process P = new Process();
            P.StartInfo.FileName = ProcessName;
            P.StartInfo.Arguments = "-rabbitMQ_Server " + rabbitMQ_Server + " -T2P_Chan " + T2P_Chan + " -P2T_Chan " + P2T_Chan;
            P.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            P.Start();

            //wait for thread complete
            T.Join();
            threadInstance.Close();

            //wait for process complete
            P.WaitForExit();

        }

        public static bool ParseArgs(Hashtable runArgs, string[] args)
        {
            //parse arg
            string key = "";
            bool prevIsKey = false;
            foreach (string s in args) {

                if (prevIsKey == true)
                {
                    runArgs[key] = s;
                    prevIsKey = false;
                }
                else {
                    Regex regex = new Regex(@"^-");
                    Match match = regex.Match(s);

                    if (match.Success)
                    {
                        regex = new Regex(@"^-(\w+)$");
                        match = regex.Match(s);

                        if (match.Success)
                        {
                            prevIsKey = true;
                            key = match.Groups[1].Value;
                            if (!runArgs.ContainsKey(key))
                            {
                                Console.WriteLine("Parameter is not supported: " + s);
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Parameter format is not correct: " + s);
                            return false;
                        }
                    }
                    else {
                        Console.WriteLine("Parameter type should be passed first: " + s);
                        return false;
                    }
                }
            }

            //show args
            Console.WriteLine("Parameters:");
            foreach (DictionaryEntry entry in runArgs)
            {
                Console.WriteLine("{0}: {1}", entry.Key, entry.Value);
            }
            Console.WriteLine();

            return true;
        }

        public static bool CheckIfLegalArgs(Hashtable runArgs)
        {
            object value = runArgs["ProcessName"];
            string ProcessName = value.ToString();
            if (!File.Exists(ProcessName))
            {
                Console.WriteLine("File not existed: " + runArgs["ProcessName"]);
                return false;
            }

            return true;
        }

        public static void Help() {
            Console.WriteLine("-rabbitMQ_Server: " + "specify rabbitMQ Server");
            Console.WriteLine("-ProcessName: " + "Name of the Window Process (P)");
            Console.WriteLine("-T2P_Chan: " + "Thread (T) to Process (P) Channel");
            Console.WriteLine("-P2T_Chan: " + "Process (P) to Thread (T) Channel");
            Console.WriteLine();
        }
    }
}
