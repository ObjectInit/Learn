namespace Learn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
           
            IMain main = new RestSharp.RestFulService.Program();
            main.Main(args);
        }
    }
}
