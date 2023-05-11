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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using System.Data;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace Wpf_App_Fleur
{
    /// <summary>
    /// Logique d'interaction pour ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Page
    {
        private MySqlConnection connexion;
        public ClientWindow(MySqlConnection connexion)
        {
            InitializeComponent();
            MySqlCommand command = connexion.CreateCommand();
            command.CommandText = "select * from client";
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                //id_client.Text = Convert.ToString(reader.GetValue(0));
                id_client.Text = reader.GetValue(0).ToString();
                nom_client.Text = reader.GetValue(1).ToString();
                prenom_client.Text = reader.GetValue(2).ToString();
                tel_client.Text = reader.GetValue(3).ToString();
                mail_client.Text = reader.GetValue(4).ToString();
                addresse_client.Text = reader.GetValue(5).ToString();
                statut_client.Text = reader.GetValue(6).ToString();
            }
            command.Dispose();
        }
        private void BtnQuitterClick(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            Window.GetWindow(this).Close();
        }

    }
}
