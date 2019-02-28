using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BotwoonIrcBot
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{E3B038EF-3664-436A-A260-2D92A5400CE7}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                for (int i = 0; i < args.Length; i++)
                    args[i] = args[i].ToLower();

                try
                {
                    if (args.Contains("/autostart"))
                        Application.Run(new MainForm(true));
                    else
                        Application.Run(new MainForm());
                }
                catch (Exception ex)
                {
                    Controller.LogError(ex, "General program error");
                    throw;
                }

                mutex.ReleaseMutex();
            }
        }
    }
}
