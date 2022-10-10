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
        void AvailableJobs(string ipAddress, out List<Job> outJobs);

        [OperationContract]
        void AllJobs(string ipAddress, out List<Job> outJobs);

        [OperationContract]
        int NumOfJobs(string ipAddress);

        [OperationContract]
        void UpdateJob(Job job, Peer peer, string performerIPAddress, string performerPort);
    }
}
