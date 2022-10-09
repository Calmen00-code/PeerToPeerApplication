using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    [ServiceContract]
    public interface ServerThreadInterface
    {
        [OperationContract]
        List<Job> AvailableJobs(string ipAddress);

        [OperationContract]
        List<Job> AllJobs(string ipAddress);
    }
}
