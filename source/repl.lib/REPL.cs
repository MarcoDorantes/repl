using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace nutility
{
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

    public string ID { get; private set; }
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
    }

    public Type InputClass { get; private set; }
  }

  public class InputInstanceReplLevel : InputReplLevel
  {
    public InputInstanceReplLevel(string id, object instance) : base(id)
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

    public object InputInstance { get; private set; }
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
        { GoUpCommand, 0 },
        { NewCommand, 0 },
        { DeleteCommand, 0 },
        { ClearCommand, 0 }
      };
      ConsoleWindowWidth = 20;
    }

    public TextWriter Writer { get; set; }
    public TextReader Reader { get; set; }

    public int ConsoleWindowWidth { get; set; }

    public void Loop(nutility.Tree<string, InputReplLevel> tree)
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
          @new(opts.IndexedArguments[1], current_tree);
        }
        else if (opts.IndexedArguments?.Count == 2 && opts.IndexedArguments?.First() == DeleteCommand && !string.IsNullOrWhiteSpace(opts.IndexedArguments[1]))
        {
          delete(opts.IndexedArguments[1], current_tree);
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
          var childInputInstanceKey = args.Any() && current_tree.Any(t => t.Key == args.First() && (t.Value.Value is InputInstanceReplLevel)) ? args.First() : null;
          var childInputClassKey = args.Any() && current_tree.Any(t => t.Key == args.First() && (t.Value.Value is InputClassReplLevel)) ? args.First() : null;
          var target_args = args.SkipWhile((arg, index) => index < 1).ToArray();
          if (childInputInstanceKey != null)
          {
            var current_level = current_tree[childInputInstanceKey].Value as InputInstanceReplLevel;
            if (target_args?.Any() == true && current_level?.InputInstance != null)
            {
              nutility.Switch.AsType(target_args, current_level.InputInstance);
            }
            else
            {
              current_tree = current_tree[childInputInstanceKey];
              current_path.Push(current_tree.Value.ID);
            }
          }
          else if (childInputClassKey != null)
          {
            var current_level = current_tree[childInputClassKey].Value as InputClassReplLevel;
            if (target_args?.Any() == true && current_level?.InputClass != null)
            {
              nutility.Switch.AsType(target_args, current_level.InputClass);
            }
            else
            {
                current_tree = current_tree[childInputClassKey];
                current_path.Push(current_tree.Value.ID);
            }
          }
          else
          {
            var current_instance_level = current_tree.Value as InputInstanceReplLevel;
            var current_class_level = current_tree.Value as InputClassReplLevel;
            if (current_instance_level != null)
            {
              nutility.Switch.AsType(args, current_instance_level.InputInstance);
            }
            else
            {
              nutility.Switch.AsType(args, current_class_level.InputClass);
            }
          }
        }
        Writer.WriteLine(new string('-', ConsoleWindowWidth - 1));
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

    private void @new(string key, nutility.Tree<string, InputReplLevel> tree)
    {
      if (reserved.ContainsKey(key) || key.StartsWith("?"))
      {
        Writer.WriteLine($"For clarity, please choose another id because '{key}' is reserved.");
      }
      else if (tree.ContainsKey(key))
      {
        Writer.WriteLine($"{key} already identifies an existing input class or input instance.");
      }
      else
      {
        InputClassReplLevel current_class = tree.Value as InputClassReplLevel;
        if (current_class == null)
        {
          Writer.WriteLine($"A new input instance is currently supported only on input class levels.");
        }
        else
        {
          var new_instance = Activator.CreateInstance(current_class.InputClass);
          var new_level = new nutility.InputInstanceReplLevel(key, new_instance);
          tree[new_level.ID] = new nutility.Tree<string, nutility.InputReplLevel> { Value = new_level, Parent = tree };
          Writer.WriteLine($"\t{key} ({current_class.InputClass.FullName})");
        }
      }
    }
    private void delete(string key, nutility.Tree<string, InputReplLevel> tree)
    {
      var instance_level = tree.ContainsKey(key) ? (tree[key].Value as InputInstanceReplLevel) : null;
      if (instance_level != null)
      {
        var instance = instance_level.InputInstance;
        var classname = instance?.GetType().FullName;
        IDisposable disposable = instance as IDisposable;
        if (disposable != null)
        {
          Writer.WriteLine($"\t{key} ({classname}) disposing...");
          disposable.Dispose();
          Writer.WriteLine($"\t{key} ({classname}) disposed.");
        }
        tree.Remove(key);
        Writer.WriteLine($"\t{key} ({classname}) removed.");
      }
      else
      {
        Writer.WriteLine($"{key} not found among existing instances at current input level.");
      }
    }
    private void ShowCurrentLevel(string input, nutility.Tree<string, InputReplLevel> tree)
    {
      var current_instance_level = tree.Value as InputInstanceReplLevel;
      var current_class_level = tree.Value as InputClassReplLevel;
      var level = current_instance_level == null ? "class" : "instance";
      var type = current_instance_level == null ? current_class_level.InputClass : current_instance_level.InputInstance.GetType();

      Writer.WriteLine($"Current input {level} type: {type.FullName}");
      if (input.Contains(HelpExpandCommand))
      {
        ShowUsage(type);
      }

      var childs_classes = tree.Where(t => t.Value.Value is InputClassReplLevel);
      Writer.WriteLine($"\r\nCurrent input child classes ({childs_classes.Count()}):");
      foreach (var child in childs_classes)
      {
        var child_class_level = child.Value.Value as InputClassReplLevel;
        Writer.WriteLine($"\r\n\t{child.Key} ({child_class_level?.InputClass.FullName})");
        if (input.Contains(HelpDetailCommand))
        {
          ShowUsage(child_class_level.InputClass, 2);
        }
      }

      var childs_instances = tree.Where(t => t.Value.Value is InputInstanceReplLevel);
      Writer.WriteLine($"\r\nCurrent input instances ({childs_instances.Count()}):");
      foreach (var child in childs_instances)
      {
        var child_instance_level = child.Value.Value as InputInstanceReplLevel;
        Writer.WriteLine($"\r\n\t{child.Key} ({child_instance_level.InputInstance.GetType().FullName})");
      }

      var commands = $"{reserved.Keys.Aggregate(new StringBuilder(" "), (whole, next) => whole.AppendFormat(" {0}", next))}";
      Writer.WriteLine($"\r\nGeneral commands: {commands.Substring(1)}");
      if (input.Contains(HelpExpandCommand))
      {
        Writer.WriteLine($"\t{HelpCommand} {HelpExpandCommand} {HelpDetailCommand} (this help)");
        Writer.WriteLine($"\t{GoUpCommand} (go up one level)");
        Writer.WriteLine("\t<input class id | input instance id> (go down to that input class or input instance level)");
        Writer.WriteLine($"\t{NewCommand} <id> (new instance of current input class)");
        Writer.WriteLine($"\t{DeleteCommand} <id> (remove an existing instance of current input class)");
        Writer.WriteLine($"\t{ClearCommand} (clear console)");
      }
    }

    private void ShowUsage(Type type, int tab_level = 1)
    {
      var identation = new string('\t', tab_level);
      var writer = new System.IO.StringWriter();
      nutility.Switch.ShowUsage(type, writer);
      var reader = new System.IO.StringReader($"{writer}");
      do
      {
        var line = reader.ReadLine();
        if (line == null) break;
        Writer.WriteLine($"{identation}{line}");
      } while (true);
    }

    private void clear() => Console.Clear();
  }
}