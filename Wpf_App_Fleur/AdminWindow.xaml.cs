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
            commandesDataGrid.ItemsSource = new DataView(dataTable);
        }
        public void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable dataTable;
            MySqlCommand command;
            switch (((TabItem)tabControl.SelectedItem).Header)
            {
                case "Clients ":
                    command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    clientsDataGrid.ItemsSource = new DataView(dataTable);
                    break;
                case "Statistiques":
                    command = new MySqlCommand("select avg(prix) from bouquet_standard;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    //prix_moyenTxt = new DataView(dataTable);
                    command = new MySqlCommand("select id_client, SUM(prix_tot) as somme_total_depense from commande where Month(date_commande) = Month(now()) " +
                        "Group by id_client Order by somme_total_depense DESC limit 1; ", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    best_c_moisGrid.ItemsSource = new DataView(dataTable);
                    command = new MySqlCommand("select id_client, SUM(prix_tot) as somme_total_depense from commande where Year(date_commande) = Year(now()) " +
                        "Group by id_client Order by somme_total_depense DESC limit 1;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    best_c_anneeGrid.ItemsSource = new DataView(dataTable);
                    command = new MySqlCommand("SELECT bs.nom, COUNT(c.id_bs) as nombre_commandes FROM bouquet_standard bs " +
                        "JOIN commande c ON bs.id_bs = c.id_bs " +
                        "GROUP BY bs.nom ORDER BY nombre_commandes DESC LIMIT 1;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    bs_sucessGrid.ItemsSource = new DataView(dataTable);
                    command = new MySqlCommand("SELECT b.id_boutique AS magasin, SUM(c.prix_tot) AS chiffre_affaires FROM commande c " +
                        "INNER JOIN boutique b ON id_boutique = b.id_boutique " +
                        "GROUP BY b.id_boutique ORDER BY chiffre_affaires DESC; ", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    best_ca_shopGrid.ItemsSource = new DataView(dataTable);
                    command = new MySqlCommand("select nom, count(nom) as vente from produit natural join commande group by nom order by vente asc limit 1;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    less_sell_exoticGrid.ItemsSource = new DataView(dataTable);
                    command.Dispose();
                    break;
                default:
                    break;
            };
        }
        public void prix_moyen_du_bouquet_achete(MySqlConnection connection)
        {
            MySqlCommand command = new MySqlCommand("select avg(prix) from bouquet_standard;", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            commandesDataGrid.ItemsSource = new DataView(dataTable);
            command.Dispose();
        }

        private void commandesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
