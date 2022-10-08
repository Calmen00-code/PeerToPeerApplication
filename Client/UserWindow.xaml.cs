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

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(PythonCodeTextBox.Text, "None");
            }
        }
    }
}
