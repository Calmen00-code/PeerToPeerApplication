using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private static readonly string WEB_SERVER_API = "http://localhost:65119/";
        private string ipAddress;

        public UserWindow(string IPAddress)
        {
            InitializeComponent();
            this.ipAddress = IPAddress;
        }

        private void PulishButton_Click(object sender, RoutedEventArgs e)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo di = new DirectoryInfo(dir);
            string fileName = "jobs_" + ipAddress + ".csv";
            string path = di.Parent.FullName + @fileName;

            int currentLine;
            try
            {
                currentLine = System.IO.File.ReadAllLines(path).Length;
            }
            catch (IOException)
            {
                currentLine = 0;
            }

            using (StreamWriter sw = File.AppendText(path))
            {
                // currentLine is treated as the job ID
                // PythonCodeTextBox.Text is the python script which will be executed by other peer/node
                // None is specified as newly created task is not allocated to any of the peer/node yet
                sw.WriteLine(currentLine + "," + PythonCodeTextBox.Text +  ",None");
            }
            MessageBox.Show("Job added to " + ipAddress);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            RestClient restClient = new RestClient(WEB_SERVER_API);

            RestRequest getRequest = new RestRequest("api/clients/{id}/", Method.Get);
            getRequest.AddUrlSegment("id", ipAddress);
            RestResponse getResponse = restClient.Execute(getRequest);

            if (getResponse.IsSuccessful)
            {
                ClientAPI client = JsonConvert.DeserializeObject<ClientAPI>(getResponse.Content);

                RestRequest updateRequest = new RestRequest("api/clients/update/{id}/", Method.Put);
                updateRequest.AddUrlSegment("id", ipAddress);

                ClientAPI updateClient = new ClientAPI();
                updateClient.IP_Address = client.IP_Address;
                updateClient.Port = client.Port;
                updateClient.Idle = false;

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
