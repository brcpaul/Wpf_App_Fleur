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
        private void Navigation_List(object sender, SelectionChangedEventArgs e)
        {
            //TableControl
            Button button = (Button)sender;
            connexion.Open();
            MySqlCommand command = connexion.CreateCommand();
            //string buttonText = button.Content.ToString();
            switch (button.Content.ToString()) //ou buttonText
            {
                case "Accueil":
                    AdminWindow admin_window = new AdminWindow(connexion);
                    Window.GetWindow(this).Content = admin_window;
                    break;
                case "Clients":
                    command.CommandText = "Select * from clients";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        
                    }
                    break;
                case "Produits":
                    command.CommandText = "Select * from produits";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                    }
                    break;
                case "Etat des Stocks":
                    command.CommandText = "Select * from stocks";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                    }
                    break;
                case "Commandes à livrer":
                    command.CommandText = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                    }
                    break;
                case "Commandes en livraison":
                    command.CommandText = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                    }
                    break;
                case "Commandes livrées":
                    command.CommandText = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                    }
                    break;
                default:
                    break;
            }
            command.Dispose();
            connexion.Close();
        }
    }
}
