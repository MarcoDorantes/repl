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
        public TextWriter Writer;
        public TextReader Reader;

        public void Loop(InputReplLevel<T> current_level)
        {
            do
            {
                Writer.Write("> ");
                var input = Reader.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) break;
                var args = nutility.SystemArgumentParser.Parse(input);
                Writer.WriteLine($"Current input class: {current_level.InputClass.FullName}");
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
            } while (true);
        }
    }
}