using APIClasses;
using Newtonsoft.Json;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static readonly string WEB_SERVER_API = "http://localhost:65119/";
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            RestClient restClient = new RestClient(WEB_SERVER_API);

            RestRequest getRequest = new RestRequest("api/clients/{id}/", Method.Get);
            getRequest.AddUrlSegment("id", IpAddressTextBox.Text);
            RestResponse getResponse = restClient.Execute(getRequest);

            if (getResponse.IsSuccessful)
            {
                ClientAPI client = JsonConvert.DeserializeObject<ClientAPI>(getResponse.Content);

                RestRequest updateRequest = new RestRequest("api/clients/update/{id}/", Method.Put);
                updateRequest.AddUrlSegment("id", IpAddressTextBox.Text);

                ClientAPI clientAPI = new ClientAPI();
                clientAPI.IP_Address = client.IP_Address;
                clientAPI.Port = client.Port;
                clientAPI.Idle = true;

                updateRequest.AddJsonBody(clientAPI);

                RestResponse updateResponse = restClient.Execute(updateRequest);

                if (updateResponse.IsSuccessful)
                {
                    UserWindow userWindow = new UserWindow(client.IP_Address, client.Port);
                    userWindow.Show();
                }
                else
                {
                    MessageBox.Show("Error while updating the client idle status to TRUE");
                }
            }
            else
            {
                MessageBox.Show("Error while finding IP address given");
            }
        }
    }
}
