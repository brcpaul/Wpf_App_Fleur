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
        private string currentPage;
        private string commandeEtatFilter;
        private static Random random = new Random();
        public AdminWindow(MySqlConnection connexion)
        {
            InitializeComponent();
            this.connexion = connexion;
            this.currentPage = null;
            this.commandeEtatFilter = "VINV";
        }
        public void ChangedCommandeEtat(object sender, EventArgs e)
        {
            RadioButton ck = sender as RadioButton;

            commandeEtatFilter= "VINV";
            switch (ck.Content)
            {
                case "Commande standard à vérifier":
                    commandeEtatFilter = "VINV";
                    break;
                case "Commande personnalisée à vérifier":
                    commandeEtatFilter = "CPAV";
                    break;
                case "Commande complète":
                    commandeEtatFilter = "CC";
                    break;
                case "Commande à livrer":
                    commandeEtatFilter = "CAL";
                    break;
                case "Commande livrée":
                    commandeEtatFilter = "CL";
                    break;
            }
            ShowCommandes();
        }
        public void ShowCommandes(object sender = null, RoutedEventArgs e = null)
        {
            MySqlCommand command;
            DataTable dataTable;

            if (boutiqueFilterBox == null) return;

            string commande_text = @"use fleur;
                SELECT id_commande, MIN(co.id_client) as 'Id client', MIN(cl.prenom) as 'Prénom client', MIN(cl.nom) as 'Nom client',MIN(co.prix_tot) as 'Prix total', MIN(bs.composition) as 'Composition standard', GROUP_CONCAT(CONCAT(cb.quantite,' ',pr.nom)) as 'Composition perso', MIN(co.date_commande) as 'Date de commande', MIN(co.date_livraison) as 'Date de livraison', MIN(co.etat) as 'Etat',MIN(bo.adresse) as 'Boutique' FROM commande co
                LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
                LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
                JOIN client cl ON cl.id_client=co.id_client
                JOIN boutique bo ON co.id_boutique=bo.id_boutique
                LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
                LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
                WHERE etat='" + commandeEtatFilter + @"' AND bo.adresse LIKE @boutique AND co.date_commande>=@dateCommandeStart AND co.date_commande <= @dateCommandeEnd
                GROUP BY id_commande";

            command = new MySqlCommand(commande_text, this.connexion);
            command.Parameters.AddWithValue("@boutique", "%" + boutiqueFilterBox.Text + "%");
            if (dateStartFilterBox.SelectedDate.HasValue) command.Parameters.AddWithValue("@dateCommandeStart",dateStartFilterBox.SelectedDate.Value);
            else command.Parameters.AddWithValue("@dateCommandeStart",new DateTime(0));

            if (dateEndFilterBox.SelectedDate.HasValue) command.Parameters.AddWithValue("@dateCommandeEnd", dateEndFilterBox.SelectedDate.Value);
            else command.Parameters.AddWithValue("@dateCommandeEnd", DateTime.Today);


            dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            commandesDataGrid.ItemsSource = new DataView(dataTable);
        }
        public void Search_ProductByName(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite, b.id_boutique FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique WHERE p.id_produit LIKE '%" + Search_ProductName.Text + "%' and s.quantite < 3; ", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            produitsrup_DataGrid.ItemsSource = new DataView(dataTable);
        }
        public void SearchProduitsBoutiqueByID(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT b.id_boutique, p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique " +
                "WHERE b.id_boutique LIKE '%" + SearchProduitsID.Text + "%' AND s.quantite < 3;", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            produitsrup_DataGrid.ItemsSource = new DataView(dataTable);
        }
        public void SearchClientByName(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client WHERE nom LIKE '%" + SearchClientName.Text + "%'", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientsDataGrid.ItemsSource = new DataView(dataTable);
        }
        public void SearchClientByID(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client WHERE id_client LIKE '%" + SearchClientID.Text + "%'", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientsDataGrid.ItemsSource = new DataView(dataTable);
        }
        public void SelectedClientChanged(object sender, EventArgs e)
        {
            string client_id="";
            try
            {
                client_id = ((DataRowView)clientsDataGrid.SelectedItem)?["id_client"]?.ToString();
            } 
            catch {
                clientDataGrid.ItemsSource=null;
            }
            if (client_id=="") return;

            string commande_text = @"use fleur;
                SELECT id_commande,MIN(co.prix_tot) as 'Prix total', MIN(bs.composition) as 'Composition standard', GROUP_CONCAT(CONCAT(cb.quantite,' ',pr.nom)) as 'Composition perso', MIN(co.date_commande) as 'Date de commande', MIN(co.date_livraison) as 'Date de livraison', MIN(co.etat) as 'Etat',MIN(bo.adresse) as 'Boutique' FROM commande co
                LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
                LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
                JOIN client cl ON cl.id_client=co.id_client
                JOIN boutique bo ON co.id_boutique=bo.id_boutique
                LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
                LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
                WHERE co.id_client='" + client_id + @"'
                GROUP BY id_commande";

            MySqlCommand command = new MySqlCommand(commande_text, this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientDataGrid.ItemsSource = new DataView(dataTable);
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void CreateCommande(object sender, RoutedEventArgs e)
        {
            string id_commande = RandomString(20);
            string id_client = commandeClientBox.Text;
            string id_boutique = commandeBoutiqueBox.Text;
            string adresse_livraison = commandeAdresseBox.Text;
            bool est_standard = commandeTypeBox.SelectedIndex==0;
            string id_bouquet;
            if (est_standard)
            {
                id_bouquet = commandeBouquetStandardBox.Text;
            } else
            {
                id_bouquet = RandomString(20);
                string description = commandeDescriptionBox.Text;
                int prixMax = Convert.ToInt32(commandePrixMaxBox.Text);
                string commandeBp = "INSERT INTO bouquet_perso(id_bp,description_bp,prix_max) VALUES(@id_bp, @description_bp,@prix_max)";
                using (var cmd = new MySqlCommand(commandeBp, connexion))
                {
                    cmd.Parameters.AddWithValue("@id_bp", id_bouquet);
                    cmd.Parameters.AddWithValue("@description_bp", description);
                    cmd.Parameters.AddWithValue("@prix_max", prixMax);
                    cmd.ExecuteNonQuery();
                }
            }
            string commandeText = "INSERT INTO commande(id_commande,id_client,id_boutique,est_standard,id_bouquet,adresse_livraison,date_commande) VALUES(@id_commande,@id_client,@id_boutique,@est_standard,@id_bouquet,@adresse_livraison,NOW())";
            using (var cmd = new MySqlCommand(commandeText, connexion))
            {
                cmd.Parameters.AddWithValue("@id_commande", id_commande);
                cmd.Parameters.AddWithValue("@id_client", id_client);
                cmd.Parameters.AddWithValue("@id_boutique", id_boutique);
                cmd.Parameters.AddWithValue("@est_standard", est_standard);
                cmd.Parameters.AddWithValue("@id_bouquet", id_bouquet);
                cmd.Parameters.AddWithValue("@adresse_livraison", adresse_livraison);
                cmd.ExecuteNonQuery();
            }

        }
        public void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.currentPage == (string)((TabItem)tabControl?.SelectedItem)?.Header) return;
            this.currentPage = (string)((TabItem)tabControl.SelectedItem).Header;
            DataTable dataTable;
            MySqlCommand command;
            switch (this.currentPage)
            {
                case "Clients":
                    command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    clientsDataGrid.ItemsSource = new DataView(dataTable);
                    break;
                case "Etat des Stocks":
                    command = new MySqlCommand("SELECT p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite, b.id_boutique FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique  WHERE s.quantite < 3;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    produitsrup_DataGrid.ItemsSource = new DataView(dataTable);
                    command = new MySqlCommand("SELECT p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite, b.id_boutique FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    produitsDataGrid.ItemsSource = new DataView(dataTable);
                    break;
                case "Commandes":
                    ShowCommandes();
                    break;
                case "Statistiques":
                    command = new MySqlCommand("select avg(prix) from bouquet_standard;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    prix_moyenTxt.ItemsSource = new DataView(dataTable);
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
                    command = new MySqlCommand("SELECT bs.nom, COUNT(*) AS total_commandes FROM bouquet_standard bs JOIN commande c ON bs.id_bs = c.id_bouquet " +
                        "WHERE c.est_standard = true GROUP BY bs.id_bs ORDER BY total_commandes DESC LIMIT 1; ", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    bs_sucessGrid.ItemsSource = new DataView(dataTable);
                    command = new MySqlCommand("SELECT c.id_boutique, SUM(c.prix_tot) AS chiffre_affaires FROM commande c GROUP BY c.id_boutique ORDER BY chiffre_affaires DESC LIMIT 1;", this.connexion);
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
        private void btn_clickExportJson(object sender, RoutedEventArgs e)
        {
            string name_file = "";
            string mysql_query = @"SELECT c.id_client, c.nom, c.prenom, c.tel, c.mail, c.adresse_factu, c.num_carte, c.statut FROM client c
                       LEFT JOIN commande cmd ON c.id_client = cmd.id_client
                       WHERE cmd.date_commande IS NULL OR cmd.date_commande < DATE_SUB(NOW(), INTERVAL 6 MONTH);";
            using (FileStream file = File.Create(name_file))
            {
                var options_js = new JsonWriterOptions { Indented = true };
                using (Utf8JsonWriter js_writer = new Utf8JsonWriter(file, options_js))
                {
                    js_writer.WriteStartArray();
                    MySqlCommand command = new MySqlCommand(mysql_query, connexion);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        js_writer.WriteStartObject();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            //object columnValue = reader.GetValue(i);
                            string columnValue = reader.GetValue(i).ToString();
                            js_writer.WriteString(columnName, columnValue);
                        }
                        js_writer.WriteEndObject();
                    }
                    js_writer.WriteEndArray();
                    command.Dispose();
                    reader.Close();
                }
            }
        }
        private void btn_clickExportXml(object sender, RoutedEventArgs e)
        {
            string name_file = "";
            string mysql_query = @"SELECT c.id_client, c.nom, c.prenom, c.tel, c.mail, c.adresse_factu, c.num_carte, c.statut FROM client c
                        INNER JOIN commande cmd ON c.id_client = cmd.id_client 
                        WHERE cmd.date_commande >= DATE_SUB(NOW(), INTERVAL 3 MONTH)
                        GROUP BY c.id_client HAVING COUNT(*) > 1;";
            string[] clients_keys = { "ID_Client", "Nom", "Prenom", "Telephone", "Email", "Adresse_de_Facturation", "Numero_de_Carte", "Statut" };
            MySqlCommand command = new MySqlCommand(mysql_query, connexion);
            MySqlDataReader reader = command.ExecuteReader();

            using (XmlWriter xml_writer = XmlWriter.Create(name_file))
            {
                xml_writer.WriteStartDocument();
                xml_writer.WriteStartElement("Clients");
                while (reader.Read())
                {
                    xml_writer.WriteStartElement("Client");
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        xml_writer.WriteElementString(clients_keys[i], reader.GetString(i));
                    }
                    xml_writer.WriteEndElement();
                }
                xml_writer.WriteEndElement();
                xml_writer.WriteEndDocument();
            }
            command.Dispose();
            reader.Close();
        }

    }
}
