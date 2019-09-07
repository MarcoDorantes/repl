using System;
using System.Linq;
using System.IO;

namespace repl.cli1
{
    public class RootInput { }

    public class CLI
    {
        public static void MainEntryPoint(string[] args, TextWriter writer = null)
        {
            try
            {
                if (writer == null)
                {
                    writer = Console.Out;
                }
                if (Environment.UserInteractive)
                {
                    writer.WriteLine($"Hello, {Environment.UserDomainName}\\{Environment.UserName} !!!");
                    writer.WriteLine($"PID: {System.Diagnostics.Process.GetCurrentProcess().Id} Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId} Culture: {System.Threading.Thread.CurrentThread?.CurrentUICulture?.Name}, {System.Threading.Thread.CurrentThread?.CurrentCulture?.Name}\n");
                }
                if (!(args?.Any() == true) || args?.First().Contains("?") == true)
                {
                    writer.WriteLine($"Working with {GetHostProcessName()}:");
                    nutility.Switch.ShowUsage(typeof(RootInput));
                }
                else
                {
                    nutility.Switch.AsType<RootInput>(args);
                }
            }
            catch (Exception ex)
            {
                for (int level = 0; ex != null; ex = ex.InnerException, ++level)
                {
                    writer.WriteLine($"\r\n[Level {level}] {ex.GetType().FullName}: {ex.Message} {ex.StackTrace}");
                }
            }

            string GetHostProcessName()
            {
                var result = Environment.GetCommandLineArgs()?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    result = System.IO.Path.GetFileNameWithoutExtension(result);
                }
                return result;
            }
        }
    }
}