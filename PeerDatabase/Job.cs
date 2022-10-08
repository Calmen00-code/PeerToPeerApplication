using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerDatabase
{
    public class Job
    {
        private string pythonScript;

        public string PythonScript { set { pythonScript = value; } get { return pythonScript; } }
    }
}
