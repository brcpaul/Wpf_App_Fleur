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
using System.Data.Common;

namespace Wpf_App_Fleur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BtnConnectionClick(object sender, RoutedEventArgs e)
        {
            // Informations de connexion à la base de données
            string port = "3306";
            string server = "localhost";
            string database = "fleurs";

            string id_admin = "root";
            string password_admin = "root";

            // Chaînes de connexion pour se connecter à la base de données
            string connectionString_admin = $"server={server};port={port};database={database};uid={id_admin};password={password_admin}";


            //Recuperation de l'identifiant et du mot de passe sasis
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if(username == id_admin && password==password_admin)
            {
                MySqlConnection connection_admin = new MySqlConnection(connectionString_admin);
                connection_admin.Open();
                AdminWindow admin_window = new AdminWindow(connection_admin);
                Window.GetWindow(this).Content = admin_window;
            } else
            {
                MessageBox.Show("Identifiants érronés");
            }
        }

        private void BtnQuitterClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
