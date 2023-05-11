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
                    etatCommande = "CL";
                    break;
            }

            string commande_text = @"use fleur;
                SELECT id_commande, MIN(co.id_client) as 'Id client', MIN(cl.prenom) as 'Prénom client', MIN(cl.nom) as 'Nom client',MIN(co.prix_tot) as 'Prix total', MIN(bs.composition) as 'Composition standard', GROUP_CONCAT(CONCAT(cb.quantite,' ',pr.nom)) as 'Composition perso', MIN(co.date_commande) as 'Date de commande', MIN(co.date_livraison) as 'Date de livraison', MIN(co.etat) as 'Etat',MIN(bo.adresse) as 'Boutique' FROM commande co
                LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
                LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
                JOIN client cl ON cl.id_client=co.id_client
                JOIN boutique bo ON co.id_boutique=bo.id_boutique
                LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
                LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
                WHERE etat='"+etatCommande+@"'
                GROUP BY id_commande";

            command = new MySqlCommand(commande_text, this.connexion);
            dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            command.Dispose();
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
                    command.Dispose();  
                    clientsDataGrid.ItemsSource = new DataView(dataTable);
                    break;
            };

        }

        private void commandesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
