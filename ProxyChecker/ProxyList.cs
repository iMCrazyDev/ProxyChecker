using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProxyChecker
{
    internal class ProxyList
    {
        private List<Leaf.xNet.ProxyClient> proxyClients;
        private object lockerProxy;
        private int indexOfCurrentProxyFromPool;

        public ProxyList()
        {
            proxyClients = new List<Leaf.xNet.ProxyClient>();
            lockerProxy = new object();
            indexOfCurrentProxyFromPool = 0;
        }

        public int ReadFromFile(string fileName, Leaf.xNet.ProxyType proxyType)
        {
            try
            {
                var fileLines = File.ReadAllLines(fileName).ToList();

                fileLines.ForEach(line =>
                {
                    Leaf.xNet.ProxyClient proxyClient;
                    if (Leaf.xNet.ProxyClient.TryParse(proxyType, line, out proxyClient)) 
                    {
                        proxyClients.Add(proxyClient);
                    }
                });

                return fileLines.Count;
            }
            catch { }

            return 0;
        }

        public Leaf.xNet.ProxyClient GetProxyClient()
        {
            lock (lockerProxy)
            {
                if (indexOfCurrentProxyFromPool == proxyClients.Count)
                {
                    indexOfCurrentProxyFromPool = 0;
                }

                return proxyClients[indexOfCurrentProxyFromPool++];
            }
        }
    }
}
