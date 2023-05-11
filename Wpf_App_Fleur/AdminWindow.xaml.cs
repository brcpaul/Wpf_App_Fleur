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
    /// Logique d'interaction pour AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Page
    {
        private MySqlConnection connexion;
        public AdminWindow(MySqlConnection connexion)
        {
            InitializeComponent();
            this.connexion = connexion; 
        }

        public void ClientsClick(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT id_client as 'Id Client',nom,prenom,tel,mail,adresse_factu,statut FROM client;",this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            dataGrid.ItemsSource = new DataView(dataTable);
        }
    }
}
