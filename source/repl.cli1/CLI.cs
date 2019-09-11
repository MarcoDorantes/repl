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
  class InputUC3
  {
    public int N;
    public void f3()
    {
      WriteLine($"{nameof(N)}: {N}");
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
        if (Environment.UserInteractive)
        {
          WriteLine($"Hello, {Environment.UserDomainName}\\{Environment.UserName} !!!");
          WriteLine($"PID: {System.Diagnostics.Process.GetCurrentProcess().Id} Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId} Culture: {System.Threading.Thread.CurrentThread?.CurrentUICulture?.Name}, {System.Threading.Thread.CurrentThread?.CurrentCulture?.Name}\n");
        }
        /*if (!(args?.Any() == true) || args?.First().Contains("?") == true)
        {
          WriteLine($"Working with {GetHostProcessName()}:");
          nutility.Switch.ShowUsage(typeof(RootInput));
        }
        else*/
        {
          var mvp = new nutility.InputClassReplLevel("MVP", typeof(RootInput));
          var use_case_hierarchy = new nutility.Tree<string, nutility.InputClassReplLevel> { Value = mvp };
          var uc2 = new nutility.InputClassReplLevel("UC2", typeof(InputUC2));
          use_case_hierarchy[uc2.ID] = new nutility.Tree<string, nutility.InputClassReplLevel> { Value = uc2, Parent = use_case_hierarchy };
          var uc3 = new nutility.InputClassReplLevel("UC3", typeof(InputUC3));
          use_case_hierarchy[uc3.ID] = new nutility.Tree<string, nutility.InputClassReplLevel> { Value = uc3, Parent = use_case_hierarchy };

          var repl = new nutility.REPL { Reader = reader, Writer = writer };
          repl.Loop(use_case_hierarchy);
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