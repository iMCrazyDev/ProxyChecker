using System;
using System.Collections.Generic;
using System.Threading;

namespace ProxyChecker
{
    internal class Program
    {
        const string proxyFileName = "proxy.txt";
        const int maxThreadsNumber = 100;
        const Leaf.xNet.ProxyType proxyType = Leaf.xNet.ProxyType.HTTP;
        const int IterationsAmount = 10;

        static void Main(string[] args)
        {
            Console.WriteLine($"Файл с прокси {proxyFileName}, тип {proxyType}");

            ProxyList proxyList = new ProxyList();
            int loadedProxyNumber = proxyList.ReadFromFile(proxyFileName, proxyType);

            Console.WriteLine($"Загружено {loadedProxyNumber} проксей"); 

            if(loadedProxyNumber == 0)
            {
                Console.ReadKey(); 
                return;
            }

            int threadsAmount = Math.Min(maxThreadsNumber, loadedProxyNumber);

            Console.WriteLine($"Запускаем {threadsAmount} потоков");

            var listThreads = StartThreads(threadsAmount, proxyList);

            while (listThreads.Exists(x => x.IsAlive))
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Итерации завершены");
            Console.ReadKey();
        }

        static List<Thread> StartThreads(int amount, ProxyList proxyList)
        {
            var list = new List<Thread>();
            for (int i = 0; i < amount; i++)
            {
                int index = i;
                var thread = new Thread(() => { ThreadBody(index, proxyList.GetProxyClient()); });
                list.Add(thread);
                thread.Start();
            }

            return list;
        }

        static string RequestUrl = "http://icanhazip.com/";

        static void ThreadBody(int index, Leaf.xNet.ProxyClient proxyClient)
        {
            Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest()
            {
                Proxy = proxyClient,
                ConnectTimeout = 10000
            };

            string tempText = $"Поток {index}, прокси {proxyClient.Host}:{proxyClient.Port}";
            
            for (int i = 0; i < IterationsAmount; i++)
            {
                try
                {
                    var page = httpRequest.Get(RequestUrl).ToString();
                    Console.WriteLine($"{tempText} Ответ сайта: {page}");

                }
                catch (Exception e) { Console.WriteLine(tempText + " Error " + e.Message); }
            }
        }
    }
}
