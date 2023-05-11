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
        public void ShowCommandes(object sender=null, RoutedEventArgs e=null)
        {
            MySqlCommand command;
            DataTable dataTable;
            RadioButton ck = sender as RadioButton;

            string etatCommande="VINV";
            switch (ck.Content)
            {
                case "Commande standard à vérifier":
                    etatCommande = "VINV";
                    break;
                case "Commande personnalisée à vérifier":
                    etatCommande = "CPAV";
                    break;
                case "Commande complète":
                    etatCommande = "CC";
                    break;
                case "Commande à livrer":
                    etatCommande = "CAL";
                    break;
                case "Commande livrée":
                    etatCommande = "CAL";
                    break;
            }

            command = new MySqlCommand("SELECT * FROM commande WHERE etat='"+etatCommande+"';", this.connexion);
            dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            commandesDataGrid.ItemsSource = new DataView(dataTable);
        }
        public void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable dataTable;
            MySqlCommand command;
            switch (((TabItem)tabControl.SelectedItem).Header)
            {
                case "Clients":
                    command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    clientsDataGrid.ItemsSource = new DataView(dataTable);
                    break;
            };

        }
    }
}
