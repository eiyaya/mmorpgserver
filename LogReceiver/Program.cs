using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.WriteLine("CurrentDomain UnhandledException. {0}",
                    (eventArgs.ExceptionObject as Exception).ToString());
            };

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Console.WriteLine("TaskScheduler UnobservedTaskException. {0}", eventArgs.Exception.ToString());
            };
            LogWriters.Init();
            var receiver = new TcpReceiver();
            receiver.Initialize();
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}
