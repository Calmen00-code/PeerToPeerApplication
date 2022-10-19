namespace ClientWeb.Models
{
    public class Peer
    {
        private string ipAdddress;
        private string port;
        private bool idle;
        private int completed;

        public string IP_Address { set { ipAdddress = value; } get { return ipAdddress; } }
        public string Port { set { port = value; } get { return port; } }
        public bool Idle { set { idle = value; } get { return idle; } }

        public int CompletedJob { set { completed = value; } get { return completed; } }
    }
}
