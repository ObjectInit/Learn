using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Learn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            IMain main = new ExpressionTree.Program_Visit();
            main.Main(args);
        }
    }
}
