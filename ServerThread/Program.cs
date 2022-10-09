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
            // you should write IP here
            Console.WriteLine("Server for peer: ");
            ServiceHost host;
            NetTcpBinding tcp = new NetTcpBinding();

            host = new ServiceHost(typeof(ServerThreadImplementation));
            host.AddServiceEndpoint(typeof(ServerThreadInterface), tcp, "net.tcp://0.0.0.0:8100/ServerThread");

            host.Open();
            Console.WriteLine("System Online");
            Console.ReadLine();
            host.Close();
        }
    }
}
