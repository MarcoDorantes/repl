using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace nutility
{
  public class InputReplLevel<T> where T : class, new()
  {
    public InputReplLevel()
    {
      InputClass = typeof(T);
      ClassChilds = new Dictionary<string, Type>();
      Instances = new Dictionary<string, T>();
    }
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
      if (Reader == null)
      {
        Reader = Console.In;
      }
      if (Writer == null)
      {
        Writer = Console.Out;
      }

      do
      {
        Writer.Write("> ");
        var input = Reader.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) break;
        var args = nutility.SystemArgumentParser.Parse(input);
        //var opts = new nutility.Switch(args);
        if (input.StartsWith("?"))
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
        }
        else
        {
          nutility.Switch.AsType<T>(args);
        }
      } while (true);
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