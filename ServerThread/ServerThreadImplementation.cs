using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServerThread
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = true)]
    internal class ServerThreadImplementation : ServerThreadInterface
    {
        void ServerThreadInterface.AllJobs(string ipAddress, out List<Job> outJobs)
        {
            /*
            string dir = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString();
            string fileName = "binjobs_" + ipAddress + ".csv";
            string path = Directory.GetParent(dir) + @"\" + fileName;
            */

            List<Job> jobs = new List<Job>();
            try
            {

                using (var reader = new StreamReader(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + ipAddress + ".csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(',');
                        Job job = new Job();
                        job.JobID = values[0];
                        job.PythonCode = values[1];
                        job.ClientIP = values[2];
                        job.ClientPort = values[3];

                        jobs.Add(job);
                    }
                }
                outJobs = jobs;
            }
            catch (IOException)
            {
                System.Diagnostics.Debug.WriteLine(ipAddress + " File not found");
                outJobs = null;
            }
        }

        void ServerThreadInterface.AvailableJobs(string ipAddress, out List<Job> availableJobs)
        {
            List<Job> jobs = new List<Job>();
            try
            {
                using (var reader = new StreamReader(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + ipAddress + ".csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(',');
                        if (values[2].Equals("None"))
                        {
                            Job job = new Job();
                            job.JobID = values[0];
                            job.PythonCode = values[1];
                            job.ClientIP = values[2];
                            job.ClientPort = values[3];

                            jobs.Add(job);
                        }
                    }
                }
                availableJobs = jobs;
            }
            catch (IOException)
            {
                System.Diagnostics.Debug.WriteLine(ipAddress + " File not found");
                availableJobs = null;
            }
        }

        int ServerThreadInterface.NumOfJobs(string ipAddress)
        {
            try
            {
                /*
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo di = new DirectoryInfo(dir);
                string fileName = "binjobs_" + ipAddress + ".csv";
                string path = di.Parent.FullName + @fileName;
                */
                int num = System.IO.File.ReadAllLines(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + ipAddress + ".csv").Length;
                return num;
            }
            catch (IOException)
            {
                System.Diagnostics.Debug.WriteLine(ipAddress + " File not found");
                return 0;
            }
        }

        // Adding the IP address and the Port number of the peer that is currently working on the job
        // so that the job will not be allocated by other peer
        void ServerThreadInterface.UpdateJob(Job job, Peer peer, string performerIPAddress, string performerPort)
        {
            string text = File.ReadAllText(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + peer.IP_Address + ".csv");
            using (StreamReader sr = new StreamReader(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + peer.IP_Address + ".csv"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');

                    if (parts[0].Equals(job.JobID))
                    {
                        text = text.Replace(parts[2], performerIPAddress);
                        text = text.Replace(parts[3], performerPort);
                        System.Diagnostics.Debug.WriteLine("text replacement: " + text);
                    }
                }
            }

            File.WriteAllText(@"C:\Users\calme\OneDrive\Desktop\Assignment2\PeerToPeerApplication\job_" + peer.IP_Address + ".csv", text);
        }
    }
}
