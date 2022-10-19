using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using ServerThread;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Client
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private static readonly string WEB_SERVER_API = "http://localhost:65119/";

        private ServerThreadInterface channel;
        private string currentIpAddress;
        private string currentPort;

        public UserWindow(string IPAddress, string port)
        {
            InitializeComponent();
            this.currentIpAddress = IPAddress;
            this.currentPort = port;
            PeerValueTextBox.Text = IPAddress;

            // Initialising Server Thread
            Process process = new Process();
            string path = System.IO.Path.Combine(AppDomain.
                CurrentDomain.BaseDirectory
                .SolutionFolder(),
                @"ServerThread\bin\Debug\ServerThread.exe");
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = currentIpAddress + " " + currentPort;
            process.Start();

            // Initialising Network Thread
            Thread networkThread = new Thread(NetworkThread);
            networkThread.Start();
        }

        public void NetworkThread()
        {
            while (true)
            {
                Job job;
                ClientAPI clientAPI;
                NetworkJob(out job, out clientAPI);

                // Debug here
                if (job == null) { System.Diagnostics.Debug.WriteLine("job is null"); }
                if (clientAPI == null) { System.Diagnostics.Debug.WriteLine("client is null"); }
                // Debug end here

                if (job != null && clientAPI != null)
                {
                    // Connect to the client server and update the job status in the client's job file
                    ChannelFactory<ServerThread.ServerThreadInterface> channelFactory;
                    NetTcpBinding tcp = new NetTcpBinding();

                    string URL = "net.tcp://" + clientAPI.IP_Address + ":" + clientAPI.Port + "/ServerThread";
                    channelFactory = new ChannelFactory<ServerThread.ServerThreadInterface>(tcp, URL);
                    channel = channelFactory.CreateChannel();

                    /** 
                     * Peer is essentially the same as ClientAPI, to avoid reference looping, that is why 
                     * creating a peer here to avoid it. Otherwise, ServerThread will also need to reference 
                     * which is not ideal here 
                     */
                    Peer peer = new Peer();
                    peer.IP_Address = clientAPI.IP_Address;
                    peer.Port = clientAPI.Port;
                    System.Diagnostics.Debug.WriteLine("Peer IP: " + peer.IP_Address);
                    System.Diagnostics.Debug.WriteLine("Peer Port: " + peer.Port);
                    // Set current login peer to perform peer's (client's) job
                    channel.UpdateJob(job, peer, currentIpAddress, currentPort, "");

                    // do the job and update the result
                    string result = ExecuteJob(job);
                    channel.UpdateJob(job, peer, currentIpAddress, currentPort, result);
                    updateCompletedJob();
                }
                Thread.Sleep(10000);
            }
        }

        private void updateCompletedJob()
        {
            RestClient restClient = new RestClient(WEB_SERVER_API);

            RestRequest getRequest = new RestRequest("api/clients/{id}/", Method.Get);
            getRequest.AddUrlSegment("id", currentIpAddress);
            RestResponse getResponse = restClient.Execute(getRequest);

            if (getResponse.IsSuccessful)
            {
                ClientAPI client = JsonConvert.DeserializeObject<ClientAPI>(getResponse.Content);

                RestRequest updateRequest = new RestRequest("api/clients/update/{id}/", Method.Put);
                updateRequest.AddUrlSegment("id", currentIpAddress);

                ClientAPI clientAPI = new ClientAPI();
                clientAPI.IP_Address = client.IP_Address;
                clientAPI.Port = client.Port;
                clientAPI.Idle = client.Idle;
                clientAPI.CompletedJob = client.CompletedJob + 1;

                updateRequest.AddJsonBody(clientAPI);

                RestResponse updateResponse = restClient.Execute(updateRequest);

                if (updateResponse.IsSuccessful)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        NumberJobTextBoxValue.Text = clientAPI.CompletedJob.ToString();
                        System.Diagnostics.Debug.WriteLine("Completed Job increase: " + clientAPI.CompletedJob.ToString());
                    });
                }
                else
                {
                    MessageBox.Show("Error while updating the client completed job number");
                }
            }
            else
            {
                MessageBox.Show("Error while finding IP address given");
            }

        }

        private string ExecuteJob(Job job)
        {
            this.Dispatcher.Invoke(() =>
            {
                byte[] encodedBytes = Convert.FromBase64String(job.PythonCode);
                CurrentJobStatusTextBoxValue.Text = Encoding.UTF8.GetString(encodedBytes);
            });

            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Position = 0;
                engine.Runtime.IO.SetOutput(stream, Encoding.Default);
                string result = "";
                try
                {
                    byte[] encodedBytes = Convert.FromBase64String(job.PythonCode);
                    string code = Encoding.UTF8.GetString(encodedBytes);

                    engine.Execute(code, scope);
                    result = Encoding.Default.GetString(stream.ToArray());
                }
                catch (Exception e)
                {
                    result = e.GetType().Name + ": " + e.Message;
                }

                return result;
            }

        }

        // Query all the clients and look for any available job
        // Otherwise, return null for both outJob and outClient if there is no available job at the moment
        private void NetworkJob(out Job outJob, out ClientAPI outClient)
        {
            RestClient restClient = new RestClient(WEB_SERVER_API);
            RestRequest restRequest = new RestRequest("api/clients", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);
            Job retJob = null;
            ClientAPI retClientAPI = null;

            if (restResponse.IsSuccessful)
            {
                List<ClientAPI> clients = JsonConvert.DeserializeObject<List<ClientAPI>>(restResponse.Content);
                List<Job> jobs;
                foreach (ClientAPI client in clients)
                {
                    // Current client cannot perform its own task, therefore we skip it.
                    if (!client.IP_Address.Equals(currentIpAddress))
                    {
                        // Try connecting to the server, if server is offline, EndpointNotFoundException will be thrown
                        try
                        {
                            // Connecting to the client server
                            ChannelFactory<ServerThread.ServerThreadInterface> channelFactory;
                            NetTcpBinding tcp = new NetTcpBinding();

                            string URL = "net.tcp://" + client.IP_Address + ":" + client.Port + "/ServerThread";
                            channelFactory = new ChannelFactory<ServerThread.ServerThreadInterface>(tcp, URL);
                            channel = channelFactory.CreateChannel();
                            channel.AvailableJobs(client.IP_Address, out jobs);
                            printAllJobs(client.IP_Address, jobs);

                            // if there is an available jobs, allocate it to the current peer
                            if (jobs != null && jobs.Count > 0)
                            {
                                retJob = jobs[0];
                                retClientAPI = client;
                                break;
                            }
                        }
                        catch (EndpointNotFoundException)
                        {
                            System.Diagnostics.Debug.WriteLine("Server " + client.IP_Address + " is inactive");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Error occured while getting clients");
            }
            outJob = retJob;
            outClient = retClientAPI;

            // Debug here
            if (outClient != null)
            {
                System.Diagnostics.Debug.WriteLine("outClient: " + outClient.IP_Address);
            }
            // Debug ends here
        }

        private void printAllJobs(string ipAddress, List<Job> jobs)
        {
            if (jobs != null)
            {
                System.Diagnostics.Debug.WriteLine("Available jobs for: " + ipAddress);
                foreach (Job job in jobs)
                {
                    byte[] encodedBytes = Convert.FromBase64String(job.PythonCode);
                    string code = Encoding.UTF8.GetString(encodedBytes);
                    System.Diagnostics.Debug.WriteLine(code + " , " + job.ClientIP + " , " + job.ClientPort);
                }
            }
        }

        private void PulishButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo di = new DirectoryInfo(dir);
            string fileName = "jobs_" + ipAddress + ".csv";
            string path = di.Parent.FullName + @fileName;
            */

            int currentLine;
            try
            {
                currentLine = System.IO.File.ReadAllLines(@"D:\.NET\Distributed Computing\PartB\PeerToPeerApplication\job_" + this.currentIpAddress + ".csv").Length;

                // currentLine = System.IO.File.ReadAllLines(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + currentIpAddress + ".csv").Length;
            }
            catch (IOException)
            {
                System.Diagnostics.Debug.WriteLine("enter");
                currentLine = 0;
            }

            using (StreamWriter sw = File.AppendText(@"D:\.NET\Distributed Computing\PartB\PeerToPeerApplication\job_" + currentIpAddress + ".csv"))
            //            using (StreamWriter sw = File.AppendText(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + currentIpAddress + ".csv"))
            {
                // currentLine is treated as the job ID
                // PythonCodeTextBox.Text is the python script which will be executed by other peer/node
                // None for IP is specified as newly created task is not allocated to any of the peer/node yet
                // None for Port is specified as newly created task is not allocated to any of the peer/node yet
                // None for result column as it has not been executed yet
                sw.WriteLine(currentLine + "," + PythonCodeTextBox.Text + ",None,None,None");
            }
            MessageBox.Show("Job added to " + currentIpAddress);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            RestClient restClient = new RestClient(WEB_SERVER_API);

            RestRequest getRequest = new RestRequest("api/clients/{id}/", Method.Get);
            getRequest.AddUrlSegment("id", currentIpAddress);
            RestResponse getResponse = restClient.Execute(getRequest);

            if (getResponse.IsSuccessful)
            {
                ClientAPI client = JsonConvert.DeserializeObject<ClientAPI>(getResponse.Content);

                RestRequest updateRequest = new RestRequest("api/clients/update/{id}/", Method.Put);
                updateRequest.AddUrlSegment("id", currentIpAddress);

                ClientAPI updateClient = new ClientAPI();
                updateClient.IP_Address = client.IP_Address;
                updateClient.Port = client.Port;
                updateClient.Idle = false;
                updateClient.CompletedJob = 0;

                updateRequest.AddJsonBody(updateClient);

                RestResponse updateResponse = restClient.Execute(updateRequest);
                if (updateResponse.IsSuccessful)
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Something went wrong while logging out...");
                }

            }
            else
            {
                MessageBox.Show("Something went wrong while logging out...");
            }
        }
    }
}
