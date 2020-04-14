using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Learn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            IMain main = new S.Api.Program();
            main.Main(args);
        }
    }
}
