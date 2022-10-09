using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    public class Job
    {
        private string pythonCode;
        private string jobID;
        private string clientIP;
        private string clientPort;

        public string PythonCode { get { return pythonCode; } set { pythonCode = value; } }
        public string JobID { get { return jobID; } set { jobID = value; } }
        public string ClientIP { get { return clientIP; } set { clientIP = value; } }
        public string ClientPort { get { return clientPort; } set { clientPort = value; } }
    }
}
