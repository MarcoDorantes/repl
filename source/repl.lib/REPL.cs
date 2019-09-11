using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace nutility
{
  public class prev_InputReplLevel<T> where T : class, new()
  {
    public prev_InputReplLevel(string id)
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

  public class prev_REPL<T> where T : class, new()
  {
    public TextWriter Writer { get; set; }
    public TextReader Reader { get; set; }

    /* Spec cases for units and whole interaction:
 dir = list current input class ID/Name
 _   = list current input class Usage
 dir = list current input child classes
 _   = add/create new input instance of the current input class or of the given/explicitly-qualified path/level input class
 _   = clone input instance of the current input instance or of the given/explicitly-qualified path/level input instance
 dir = list current input child instances
 _   = change to or make-current one of the childs (class or instance)
 _   = change to or make-current the parent of the current class or instance
 (args1) = process args on current class or instance
 (args2) = process args on class or instance of a given/explicit path/level
 record, store(script) and replay
*/

    public void Loop(prev_InputReplLevel<T> current_level)
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

      //var current_path = new Stack<string>();
      //current_path.Push(current_repl.Value.Name);

      do
      {
        Writer.WriteLine();
        Writer.Write($"{System.Reflection.MethodInfo.GetCurrentMethod().Name}: ");
        Writer.Write("> ");

        //Writer.WriteLine();
        //Writer.Write($"{path(current_path)}: ");

        var input = Reader.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input)) break;
        var args = nutility.SystemArgumentParser.Parse(input);
        var opts = new nutility.Switch(args);
        if (args?.Any() == false || opts?.Any() == false) continue;
        if (input.StartsWith("?"))
        {
          ShowCurrentLevel(input, current_level);
        }
        else if (opts.IndexedArguments?.Count == 2 && opts.IndexedArguments?.First() == "new" && !string.IsNullOrWhiteSpace(opts.IndexedArguments[1]))
        {
          @new(opts.IndexedArguments[1], current_level);
        }
        else if (opts.IndexedArguments?.Count == 2 && opts.IndexedArguments?.First() == "delete" && !string.IsNullOrWhiteSpace(opts.IndexedArguments[1]))
        {
          delete(opts.IndexedArguments[1], current_level);
        }
        else if (opts.IndexedArguments?.Count == 1 && opts.IndexedArguments?.First() == "cls")
        {
          clear();
        }
        else
        {
          var childInputInstanceKey = args.Any() && current_level.Instances.ContainsKey(args.First()) ? args.First() : null;
          var childInputClassKey = args.Any() && current_level.ClassChilds.ContainsKey(args.First()) ? args.First() : null;
          var target_args = args.SkipWhile((arg, index) => index < 1).ToArray();
          if (childInputInstanceKey != null)
          {
            if (target_args?.Any() == true)
            {
              nutility.Switch.AsType(target_args, current_level.Instances[childInputInstanceKey]);
            }
            else
            {
              //TODO change current
            }
          }
          else if (childInputClassKey != null)
          {
            if (target_args?.Any() == true)
            {
              nutility.Switch.AsType(target_args, current_level.ClassChilds[childInputClassKey]);
            }
            else
            {
              //TODO change current
            }
          }
          else
          {
            nutility.Switch.AsType<T>(args);
          }
        }
      } while (true);
    }

    private string path(Stack<string> stack)
    {
      var result = "";
      if (stack.Count > 0)
      {
        result = $"{stack.ToArray().Reverse().Aggregate(new StringBuilder(), (w, n) => w.AppendFormat("/{0}", n))}";
      }
      return result;
    }

    private void @new(string key, prev_InputReplLevel<T> current_level)
    {
      if (current_level.Instances.ContainsKey(key))
      {
        Writer.WriteLine($"{key} already identifies an existing instance.");
      }
      else
      {
        current_level.Instances.Add(key, new T());
        Writer.WriteLine($"\t{key} ({current_level.Instances[key].GetType().FullName})");
      }
    }
    private void delete(string key, prev_InputReplLevel<T> current_level)
    {
      if (current_level.Instances.ContainsKey(key))
      {
        var instance = current_level.Instances[key];
        var classname = instance?.GetType().FullName;
        IDisposable disposable = instance as IDisposable;
        if (disposable != null)
        {
          Writer.WriteLine($"\t{key} ({classname}) disposing...");
          disposable.Dispose();
          Writer.WriteLine($"\t{key} ({classname}) disposed.");
        }
        current_level.Instances.Remove(key);
        Writer.WriteLine($"\t{key} ({classname}) removed.");
      }
      else
      {
        Writer.WriteLine($"{key} not found among existing instances.");
      }
    }
    private void ShowCurrentLevel(string input, prev_InputReplLevel<T> current_level)
    {
      Writer.WriteLine($"Current input class: {current_level.InputClass.FullName}");
      if (input.Contains("??"))
      {
        Writer.WriteLine($"Working with {current_level.InputClass.FullName}:");
        ShowUsage(current_level.InputClass);
      }
      Writer.WriteLine($"\r\nCurrent input child classes ({current_level.ClassChilds.Count}):");
      foreach (var id in current_level.ClassChilds.Keys)
      {
        Writer.WriteLine($"\t{id} ({current_level.ClassChilds[id].FullName})");
        if (input.Contains("???"))
        {
          ShowUsage(current_level.ClassChilds[id]);
        }
      }
      Writer.WriteLine($"\r\nCurrent input instances ({current_level.Instances.Count}):");
      foreach (var id in current_level.Instances.Keys)
      {
        Writer.WriteLine($"\t{id} ({current_level.Instances[id].GetType().FullName})");
      }
      Writer.WriteLine("\r\nGeneral commands: ? ?? ??? new delete cls");
      if (input == "??")
      {
        Writer.WriteLine("\t? ?? (this help)");
        Writer.WriteLine("\tnew <id> (new instance of current input class)");
        Writer.WriteLine("\tdelete <id> (remove an existing instance of current input class)");
        Writer.WriteLine("\tcls (clear console)");
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
    private void clear() => Console.Clear();
  }


  //Hierarchical approach?
  public class InputReplLevel
  {
    public InputReplLevel(string id)
    {
      #region Input validation
      if (string.IsNullOrWhiteSpace(id))
      {
        throw new ArgumentNullException($"{nameof(id)}", "The ID for input classes cannot be null or empty.");
      }
      #endregion

      ID = id;
    }

    public string ID;
  }
  public class InputClassReplLevel : InputReplLevel
  {
    public InputClassReplLevel(string id, Type type) : base(id)
    {
      #region Input validation
      if (!(type?.IsValueType == false) || type == typeof(string))
      {
        throw new ArgumentOutOfRangeException($"{nameof(type)}", "Input classes can be regular user-defined .NET Reference Class Types only, please. Thank you.");
      }
      #endregion

      InputClass = type;
      Instances = new Dictionary<string, object>();
    }

    public Type InputClass;
    public IDictionary<string, object> Instances;
  }

  public class InputInstanceReplLevel : InputReplLevel
  {
    public InputInstanceReplLevel(string id, object instance):base(id)
    {
      #region Input validation
      var type = instance?.GetType();
      if (!(type?.IsValueType == false) || type == typeof(string))
      {
        throw new ArgumentOutOfRangeException($"{nameof(type)}", "Input instances can be instances of regular user-defined .NET Reference Class Types only, please. Thank you.");
      }
      #endregion

      InputInstance = instance;
    }

    public object InputInstance;
  }

  public class REPL
  {
    private Dictionary<string, int> reserved;

    private const string HelpCommand = "?";
    private const string HelpExpandCommand = "??";
    private const string HelpDetailCommand = "???";
    private const string NewCommand = "new";
    private const string DeleteCommand = "delete";
    private const string ClearCommand = "cls";
    private const string GoUpCommand = "..";

    public REPL()
    {
      reserved = new Dictionary<string, int>
      {
        { HelpCommand, 0 },
        { HelpExpandCommand, 0 },
        { HelpDetailCommand, 0 },
        { NewCommand, 0 },
        { DeleteCommand, 0 },
        { ClearCommand, 0 },
        { GoUpCommand, 0 }
      };
    }

    public TextWriter Writer { get; set; }
    public TextReader Reader { get; set; }

    //public void Loop(nutility.Tree<string, InputReplLevel> tree)
    public void Loop(nutility.Tree<string, InputClassReplLevel> tree)
    {
      #region Input validation
      if (tree?.Value == null)
      {
        throw new ArgumentNullException($"{nameof(tree)}", $"{nameof(tree)} cannot be null.");
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

      var current_tree = tree;
      var current_path = new Stack<string>();
      current_path.Push(current_tree.Value.ID);
      do
      {
        Writer.WriteLine();
        Writer.Write($"{path(current_path)}: ");

        var input = Reader.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input)) break;
        var args = nutility.SystemArgumentParser.Parse(input);
        var opts = new nutility.Switch(args);
        if (args?.Any() == false || opts?.Any() == false) continue;
        if (input.StartsWith(HelpCommand))
        {
          ShowCurrentLevel(input, current_tree);
        }
        else if (opts.IndexedArguments?.Count == 2 && opts.IndexedArguments?.First() == NewCommand && !string.IsNullOrWhiteSpace(opts.IndexedArguments[1]))
        {
          @new(opts.IndexedArguments[1], current_tree.Value);
        }
        else if (opts.IndexedArguments?.Count == 2 && opts.IndexedArguments?.First() == DeleteCommand && !string.IsNullOrWhiteSpace(opts.IndexedArguments[1]))
        {
          delete(opts.IndexedArguments[1], current_tree.Value);
        }
        else if (opts.IndexedArguments?.Count == 1 && opts.IndexedArguments?.First() == GoUpCommand && current_tree != tree)
        {
          current_tree = current_tree.Parent;
          current_path.Pop();
        }
        else if (opts.IndexedArguments?.Count == 1 && opts.IndexedArguments?.First() == ClearCommand)
        {
          clear();
        }
        else
        {
          var childInputInstanceKey = args.Any() && current_tree.Value.Instances.ContainsKey(args.First()) ? args.First() : null;
          var childInputClassKey = args.Any() && current_tree.ContainsKey(args.First()) ? args.First() : null;
          var target_args = args.SkipWhile((arg, index) => index < 1).ToArray();
          if (childInputInstanceKey != null)
          {
            if (target_args?.Any() == true)
            {
              nutility.Switch.AsType(target_args, current_tree.Value.Instances[childInputInstanceKey]);
            }
            else
            {
              //TODO change current to new input instance
              //current_path.Peek()

              //current_tree = current_tree[childInputClassKey];
              //current_path.Push(current_tree.Value.ID);
            }
          }
          else if (childInputClassKey != null)
          {
            if (target_args?.Any() == true)
            {
              nutility.Switch.AsType(target_args, current_tree[childInputClassKey].Value.InputClass);
            }
            else
            {
                current_tree = current_tree[childInputClassKey];
                current_path.Push(current_tree.Value.ID);
            }
          }
          else
          {
            nutility.Switch.AsType(args, current_tree.Value.InputClass);
          }
        }
      } while (true);
    }

    private string path(Stack<string> stack)
    {
      var result = "";
      if (stack.Count > 0)
      {
        result = $"{stack.ToArray().Reverse().Aggregate(new StringBuilder(), (w, n) => w.AppendFormat("/{0}", n))}";
      }
      return result;
    }

    private void @new(string key, InputClassReplLevel current_level)
    {
      if (reserved.ContainsKey(key) || key.StartsWith("?"))
      {
        Writer.WriteLine($"For clarity, please choose another ID becasuse '{key}' is reserved.");
      }
      else if (current_level.Instances.ContainsKey(key))
      {
        Writer.WriteLine($"{key} already identifies an existing instance.");
      }
      else
      {
        current_level.Instances.Add(key, Activator.CreateInstance(current_level.InputClass));
        Writer.WriteLine($"\t{key} ({current_level.Instances[key].GetType().FullName})");
      }
    }
    private void delete(string key, InputClassReplLevel current_level)
    {
      if (current_level.Instances.ContainsKey(key))
      {
        var instance = current_level.Instances[key];
        var classname = instance?.GetType().FullName;
        IDisposable disposable = instance as IDisposable;
        if (disposable != null)
        {
          Writer.WriteLine($"\t{key} ({classname}) disposing...");
          disposable.Dispose();
          Writer.WriteLine($"\t{key} ({classname}) disposed.");
        }
        current_level.Instances.Remove(key);
        Writer.WriteLine($"\t{key} ({classname}) removed.");
      }
      else
      {
        Writer.WriteLine($"{key} not found among existing instances.");
      }
    }
    private void ShowCurrentLevel(string input, nutility.Tree<string, InputClassReplLevel> tree)
    {
      InputClassReplLevel current_level = tree.Value;

      Writer.WriteLine($"Current input class: {current_level.InputClass.FullName}");
      if (input.Contains(HelpExpandCommand))
      {
        Writer.WriteLine($"Working with {current_level.InputClass.FullName}:");
        ShowUsage(current_level.InputClass);
      }
      Writer.WriteLine($"\r\nCurrent input child classes ({tree.Count}):");
      foreach (var id in tree.Keys)
      {
        Writer.WriteLine($"\t{id} ({tree[id].Value.InputClass.FullName})");
        if (input.Contains(HelpDetailCommand))
        {
          ShowUsage(tree[id].Value.InputClass);
        }
      }
      Writer.WriteLine($"\r\nCurrent input instances ({current_level.Instances.Count}):");
      foreach (var id in current_level.Instances.Keys)
      {
        Writer.WriteLine($"\t{id} ({current_level.Instances[id].GetType().FullName})");
      }
      var commands = $"{reserved.Keys.Aggregate(new StringBuilder(" "), (whole, next) => whole.AppendFormat(" {0}", next))}";
      Writer.WriteLine($"\r\nGeneral commands: {commands.Substring(1)}");
      if (input == HelpExpandCommand)
      {
        Writer.WriteLine($"\t{HelpCommand} {HelpExpandCommand} {HelpDetailCommand} (this help)");
        Writer.WriteLine($"\t{GoUpCommand} (go up one level)");
        Writer.WriteLine("\t<input class id | input instance id> (go down to that input class or input instance level)");
        Writer.WriteLine($"\t{NewCommand} <id> (new instance of current input class)");
        Writer.WriteLine($"\t{DeleteCommand} <id> (remove an existing instance of current input class)");
        Writer.WriteLine($"\t{ClearCommand} (clear console)");
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
    private void clear() => Console.Clear();
  }
}