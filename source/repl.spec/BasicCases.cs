using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//using static System.Console;

namespace repl.spec
{
  class InputX
  {
    public int N;
    public void f1()
    {
      //WriteLine($"{nameof(N)}: {N}");
    }
  }

  [TestClass]
  public class BasicCases
  {
    [TestMethod]
    public void basic()
    {
      //Arrange
      var inputX_instance = new InputX();
      var current_level = new nutility.InputReplLevel<InputX>("UC-1");
      current_level.ClassChilds.Add("c2", typeof(InputX));
      current_level.Instances.Add("x1", inputX_instance);

      var input_lines = new[] { "?" };
      var reader = new StringReader(asTextContent(input_lines));
      var writer = new StringWriter();
      var repl = new nutility.REPL<InputX> { Reader = reader, Writer = writer };

      //Act
      repl.Loop(current_level);

      //Assert
      Assert.IsTrue($"{writer}".Contains("x1"));
    }
    private string asTextContent(IEnumerable<string> lines) => $"{lines?.Aggregate(new StringWriter(), (whole, next) => { whole.WriteLine(next); return whole; })}";
  }

  [TestClass]
  public class CLI_1_BasicCases
  {
    [TestMethod]
    public void basic()
    {
      //Arrange
      var asFound = Console.Out;
      try
      {
        var inputRoot_instance = new cli1.RootInput();
        var current_level = new nutility.InputReplLevel<cli1.RootInput>("UC-2");
        current_level.ClassChilds.Add("c2", typeof(cli1.RootInput));
        current_level.Instances.Add("x1", inputRoot_instance);

        var input_lines = new[] { "-f1 -n=-132" };
        var reader = new StringReader(asTextContent(input_lines));
        var writer = new StringWriter();
        Console.SetOut(writer);
        var repl = new nutility.REPL<cli1.RootInput> { Reader = reader, Writer = writer };

        //Act
        repl.Loop(current_level);

        //Assert
        Assert.IsTrue($"{writer}".Contains("N: -132"));
      }
      finally
      {
        Console.SetOut(asFound);
      }
    }

    [TestMethod,Ignore]
    public void entry_point()
    {
      var asFound = Console.Out;
      try
      {
        //Arrange
        var input_lines = new[] { "-f1", "-n=3" };
        var writer = new StringWriter();
        Console.SetOut(writer);

        //Act
        repl.cli1.CLI.MainEntryPoint(input_lines);

        //Assert
        Assert.IsTrue($"{writer}".Contains("N: 3"));
      }
      finally
      {
        Console.SetOut(asFound);
      }
    }

    private string asTextContent(IEnumerable<string> lines) => $"{lines?.Aggregate(new StringWriter(), (whole, next) => { whole.WriteLine(next); return whole; })}";
  }
}