using APIClasses;
using RestSharp;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private static readonly string WEB_SERVER_API = "http://localhost:65119/";

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RestClient restClient = new RestClient(WEB_SERVER_API);
            RestRequest restRequest = new RestRequest("api/clients/add", Method.Post);
            
            ClientAPI clientAPI = new ClientAPI();
            clientAPI.IP_Address = IpAddressTextBox.Text;
            clientAPI.Port = PortTextBox.Text;
            clientAPI.Idle = true;
            restRequest.AddJsonBody(clientAPI);

            RestResponse restResponse = restClient.Execute(restRequest);

            if (restResponse.IsSuccessful)
            {
                MessageBox.Show("IP address registered!");
            }
            else
            {
                MessageBox.Show("Something went wrong while registering IP address");
            }
        }
    }
}
