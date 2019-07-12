namespace Learn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
           
            IMain main = new RequestWebApi.BySelfe.Program();
            main.Main(args);
        }
    }
}
