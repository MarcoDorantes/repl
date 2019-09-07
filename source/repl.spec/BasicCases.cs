using System;
using System.Linq;
using System.IO;
using System.Text;
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
            var current_level = new nutility.InputReplLevel<InputX>();
            current_level.ClassChilds.Add("c2", typeof(InputX));
            current_level.Instances.Add("x1", inputX_instance);

            var input_lines = new[] { "d", null };
            var reader = new StringReader($"{input_lines.Aggregate(new StringBuilder(), (w, n) => w.AppendFormat("{0}", n))}");
            var writer = new StringWriter();
            var repl = new nutility.REPL<InputX> { Reader = reader, Writer = writer };

            //Act
            repl.loop(current_level);

            //Assert
            Assert.IsTrue($"{writer.GetStringBuilder()}".Contains("x1"));
        }
    }
}