using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace nutility
{
  public class InputReplLevel<T> where T : class, new()
  {
    public InputReplLevel(string id)
    {
      ID = id;
      InputClass = typeof(T);
      ClassChilds = new Dictionary<string, Type>();
      Instances = new Dictionary<string, T>();
    }

    public string ID;
    public Type InputClass;
    public Dictionary<string, Type> ClassChilds;
    public Dictionary<string, T> Instances;
  }

  public class REPL<T> where T : class, new()
  {
    public TextWriter Writer { get; set; }
    public TextReader Reader { get; set; }

    public void Loop(InputReplLevel<T> current_level)
    {
      #region Input validation
      if (current_level == null)
      {
        throw new ArgumentNullException($"{nameof(current_level)}",$"{nameof(current_level)} cannot be null.");
      }
      if (Reader == null)
      {
        Reader = Console.In;
      }
      if (Writer == null)
      {
        Writer = Console.Out;
      }
      #endregion

      do
      {
        Writer.Write("> ");
        var input = Reader.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) break;
        input = input.Trim();
        var args = nutility.SystemArgumentParser.Parse(input);
        var opts = new nutility.Switch(args);
        if (input.StartsWith("?"))
        {
          ShowUsage(input, current_level);
        }
        else if (opts.IndexedArguments?.Count == 2 && opts.IndexedArguments?.First()?.ToLower() == "new" && !string.IsNullOrWhiteSpace(opts.IndexedArguments[1]))
        {
          var key = opts.IndexedArguments[1];
          current_level.Instances.Add(key, new T());
        }
        else
        {
          nutility.Switch.AsType<T>(args);
        }
      } while (true);
    }

    private void ShowUsage(string input, InputReplLevel<T> current_level)
    {
      Writer.WriteLine($"Current input class: {current_level.InputClass.FullName}");
      if (input == "??")
      {
        ShowUsage(current_level.InputClass);
      }
      Writer.WriteLine($"\r\nCurrent input child classes ({current_level.ClassChilds.Count}):");
      foreach (var id in current_level.ClassChilds.Keys)
      {
        Writer.WriteLine($"\t{id} ({current_level.ClassChilds[id].FullName})");
      }
      Writer.WriteLine($"\r\nCurrent input instances ({current_level.Instances.Count}):");
      foreach (var id in current_level.Instances.Keys)
      {
        Writer.WriteLine($"\t{id} ({current_level.Instances[id].GetType().FullName})");
      }
      Writer.WriteLine("\r\nGeneral commands: ? ?? new");
      if (input == "??")
      {
        Writer.WriteLine("\t? ?? (this help)");
        Writer.WriteLine("\tnew <id> (new instance of current input class)");
      }
    }

    private void ShowUsage(Type type)
    {
      var writer = new System.IO.StringWriter();
      nutility.Switch.ShowUsage(type, writer);
      var reader = new System.IO.StringReader($"{writer}");
      do
      {
        var line = reader.ReadLine();
        if (line == null) break;
        Writer.WriteLine($"\t{line}");
      } while (true);
    }
  }
}