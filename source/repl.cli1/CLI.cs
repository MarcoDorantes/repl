using System;
using System.Linq;
using System.IO;

using static System.Console;

namespace repl.cli1
{
  public class RootInput
  {
    public int N;
    public void f1()
    {
      WriteLine($"{nameof(N)}: {N}");
    }
  }

  public class InputUC2 : IDisposable
  {
    public int N;
    public void f2()
    {
      WriteLine($"{nameof(N)}: {N}");
    }
    public void Dispose()
    {
      WriteLine($"Dispose {nameof(InputUC2)}");
    }
  }
  public class CLI
  {
    public static void MainEntryPoint(string[] args, TextReader reader = null, TextWriter writer = null)
    {
      try
      {
        if (reader == null)
        {
          reader = Console.In;
        }
        if (writer == null)
        {
          writer = Console.Out;
        }
        var repl_level = new nutility.InputReplLevel<RootInput>("MVP");
        repl_level.ClassChilds.Add("UC2", typeof(InputUC2));

        var repl = new nutility.REPL<RootInput> { Reader = reader, Writer = writer };
        repl.Loop(repl_level);
        return;

        writer.WriteLine($"Hello, {Environment.UserDomainName}\\{Environment.UserName} !!!");
        writer.WriteLine($"PID: {System.Diagnostics.Process.GetCurrentProcess().Id} Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId} Culture: {System.Threading.Thread.CurrentThread?.CurrentUICulture?.Name}, {System.Threading.Thread.CurrentThread?.CurrentCulture?.Name}\n");
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