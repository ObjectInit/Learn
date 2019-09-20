namespace Learn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
           
            IMain main = new FakerExt.Program();
            main.Main(args);
        }
    }
}
