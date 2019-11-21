namespace Learn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
           
            IMain main = new AutoMapper.Simple.Program();
            main.Main(args);
        }
    }
}
