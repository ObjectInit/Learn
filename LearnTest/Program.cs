using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace LearnTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IMain main = new Owin.Program();
            main.Main(args);
        }
    }
}
