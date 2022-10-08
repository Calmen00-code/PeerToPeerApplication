using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerDatabase
{
    public class JobList
    {
        private List<Job> jobs;

        public JobList() 
        {
            jobs = new List<Job>();
        }

        public List<Job> Jobs { get { return jobs; } } 

        public void AddJob(Job job)
        {
            jobs.Add(job);
        }

        public void DeleteJob(Job job)
        {
            jobs.Remove(job);
        }

        public int NumJobs()
        { 
            return jobs.Count;
        }

    }
}
