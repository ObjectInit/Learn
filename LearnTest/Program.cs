namespace Learn.Console
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
