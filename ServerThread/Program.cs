using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Peer Server IP: " + args[0]);
            Console.WriteLine("Peer Server Port: " + args[1]);
            ServiceHost host;
            NetTcpBinding tcp = new NetTcpBinding();

            host = new ServiceHost(typeof(ServerThreadImplementation));
            string address = "net.tcp://" + args[0] + ":" + args[1] + "/ServerThread";
            host.AddServiceEndpoint(typeof(ServerThreadInterface), tcp, address);

            host.Open();
            Console.WriteLine("\nSystem Online");
            Console.ReadLine();
            host.Close();
        }
    }
}
