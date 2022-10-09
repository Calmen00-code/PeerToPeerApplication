using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServerThread
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = true)]
    internal class ServerThreadImplementation : ServerThreadInterface
    {
        List<Job> ServerThreadInterface.AllJobs(string ipAddress)
        {
            string dir = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString();
            string fileName = "binjobs_" + ipAddress + ".csv";
            string path = Directory.GetParent(dir) + @"\" + fileName;

            List<Job> jobs = new List<Job>();
            using (var reader = new StreamReader(path))
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
            return jobs;
        }

        List<Job> ServerThreadInterface.AvailableJobs(string ipAddress)
        {
            throw new NotImplementedException();
        }

        int ServerThreadInterface.NumOfJobs()
        {
            return 10;
        }
    }
}
