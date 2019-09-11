using System;

namespace repl.app1
{
  class Program
  {
    static void Main(string[] args) => repl.cli1.CLI.MainEntryPoint(args, consoleWindowWidth: System.Console.WindowWidth);
  }
}