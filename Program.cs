using System.Threading;

namespace JabamiYumeko
{
    class Program
    {
        static void Main(string[] args)
        {
            MonitorTask maid = new MonitorTask();
            maid.Start();
            while (true)
            {
                Thread.Sleep(1000);
            }

        }
    }
}
