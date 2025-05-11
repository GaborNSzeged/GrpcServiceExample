using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //LoggerProxy.Proxy.Init();

            LoggerProxy.Proxy.Log("data1.csv", "kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            LoggerProxy.Proxy.Log("data2.csv", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            LoggerProxy.Proxy.Log("data3.csv", "wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");

            // This dispose will close the pipe and when the channel disconnected then it will stop the LoggerClient.
            Thread.Sleep(3000);
            LoggerProxy.Proxy.Dispose2();


            //var loggerClient = LoggerClient.LoggerClient.Instance;

            //try
            //{
            //    loggerClient.SendContent("data1.csv", "kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            //    loggerClient.SendContent("data2.csv", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            //    loggerClient.SendContent("data3.csv", "wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
            //    loggerClient.SendContent("data4.csv", "qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq");
            //}
            //finally
            //{
            //    loggerClient.Dispose();
            //}
        }
    }
}
