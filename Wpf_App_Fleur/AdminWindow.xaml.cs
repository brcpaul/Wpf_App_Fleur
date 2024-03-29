﻿using System;
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
using MySqlX.XDevAPI;

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
        private string selectedClientId;
        public AdminWindow(MySqlConnection connexion)
        {
            InitializeComponent();
            this.connexion = connexion;
            this.currentPage = null;
            this.commandeEtatFilter = "VINV";
            this.selectedClientId = "";
        }
        /// Permet de changer l'état des commandes à filtrer
        public void ChangedCommandeEtat(object sender, EventArgs e)
        {
            RadioButton ck = sender as RadioButton;

            commandeEtatFilter = "VINV";
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
        /// Permet d'afficher les commandes en utilisant les filtres renseignés
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

            // On utilise LEFT JOIN pour récuperer les infos des bouquets car pour une commande le champ id_bouquet provient soit de bouquet_perso soit de bouquet_standard
            // De la même manière, comme toutes les commandes ne sont pas forcément liées à des produits de composition_bouquet on utilise LEFT JOIN pour les récuperer
            // Pour les infos client et boutique qui sont récupérées pour chaque commande peut importe son type on utilise JOIN 

            command = new MySqlCommand(commande_text, this.connexion);
            command.Parameters.AddWithValue("@boutique", "%" + boutiqueFilterBox.Text + "%");
            if (dateStartFilterBox.SelectedDate.HasValue) command.Parameters.AddWithValue("@dateCommandeStart", dateStartFilterBox.SelectedDate.Value);
            else command.Parameters.AddWithValue("@dateCommandeStart", new DateTime(0));

            if (dateEndFilterBox.SelectedDate.HasValue) command.Parameters.AddWithValue("@dateCommandeEnd", dateEndFilterBox.SelectedDate.Value);
            else command.Parameters.AddWithValue("@dateCommandeEnd", DateTime.Today);


            dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            commandesDataGrid.ItemsSource = new DataView(dataTable);
        }
        /// <summary>
        /// Fonction qui permet de rechercher un produit sous le seuil d'alerte avec son id de produit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Search_ProductByName(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite, b.id_boutique FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique WHERE p.id_produit LIKE '%" + Search_ProductName.Text + "%' and s.quantite < 10; ", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            produitsrup_DataGrid.ItemsSource = new DataView(dataTable);
        }
        /// <summary>
        /// Fonction qui permet de trouver tous les produits spous le seuil d'alerte issus d'une boutique dont on aura donner son id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchProduitsBoutiqueByID(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT b.id_boutique, p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique " +
                "WHERE b.id_boutique LIKE '%" + SearchProduitsID.Text + "%' AND s.quantite < 10;", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            produitsrup_DataGrid.ItemsSource = new DataView(dataTable);
        }
        /// <summary>
        /// Fonction qui permet de rechercher un client à partir de son nom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchClientByName(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client WHERE nom LIKE '%" + SearchClientName.Text + "%'", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientsDataGrid.ItemsSource = new DataView(dataTable);
        }
        /// <summary>
        /// Fonction qui permet de rechercher un client à partir de son id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchClientByID(object sender, RoutedEventArgs e)
        {
            MySqlCommand command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client WHERE id_client LIKE '%" + SearchClientID.Text + "%'", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientsDataGrid.ItemsSource = new DataView(dataTable);
        }

        /// <summary>
        /// Fonction qui permet de récupérer les commandes d'un client particulier, et les affiches dans une table à part. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectedClientChanged(object sender, EventArgs e)
        {
            selectedClientId = "";
            try
            {
                selectedClientId = ((DataRowView)clientsDataGrid.SelectedItem)?["id_client"]?.ToString();
            }
            catch
            {
                clientDataGrid.ItemsSource = null;
            }
            if (selectedClientId == "")
            {
                SupprimerClientButton.Visibility = Visibility.Hidden;
                return;
            }
            SupprimerClientButton.Visibility = Visibility.Visible;

            string commande_text = @"use fleur;
                SELECT id_commande,MIN(co.prix_tot) as 'Prix total', MIN(bs.composition) as 'Composition standard', GROUP_CONCAT(CONCAT(cb.quantite,' ',pr.nom)) as 'Composition perso', MIN(co.date_commande) as 'Date de commande', MIN(co.date_livraison) as 'Date de livraison', MIN(co.etat) as 'Etat',MIN(bo.adresse) as 'Boutique' FROM commande co
                LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
                LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
                JOIN client cl ON cl.id_client=co.id_client
                JOIN boutique bo ON co.id_boutique=bo.id_boutique
                LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
                LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
                WHERE co.id_client='" + selectedClientId + @"'
                GROUP BY id_commande";

            MySqlCommand command = new MySqlCommand(commande_text, this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientDataGrid.ItemsSource = new DataView(dataTable);
        }
        /// <summary>
        /// Enregistre les modifications faites sur un client directement depuis l'interface graphique
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClientEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            string clientId = ((DataRowView)clientsDataGrid.SelectedItem)["id_client"].ToString(); // Récupère l'id du client modifié 
            string modifiedColumn = (string)e.Column.Header; // Récupère la colonne modifiée
            string modifiedValue = ((TextBox)e.EditingElement).Text; // Et enfin sa valeur
            if (modifiedValue == "")
            {
                modifiedValue = null;
            }
            string commandeText = "UPDATE client set " + modifiedColumn + "=@value WHERE id_client=@clientId;";
            using (var cmd = new MySqlCommand(commandeText, connexion))
            {
                cmd.Parameters.AddWithValue("@column", modifiedColumn);
                cmd.Parameters.AddWithValue("@value", modifiedValue);
                cmd.Parameters.AddWithValue("@clientId", clientId);
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Client modifié !");
                }
                catch
                {
                    MessageBox.Show("Le client n'a pas pu être modifié...");
                    e.Cancel = true;
                    (sender as DataGrid).CancelEdit(DataGridEditingUnit.Cell);

                }
            }
        }
        /// <summary>
        /// Permet de supprimer un client de la base de données
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SupprimerClient(object sender, RoutedEventArgs e)
        {
            string commandeText = "DELETE FROM client WHERE id_client=@clientId;";
            using (var cmd = new MySqlCommand(commandeText, connexion))
            {
                cmd.Parameters.AddWithValue("@clientId", selectedClientId);
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Client supprimé !");
                }
                catch
                {
                    MessageBox.Show("Le client n'a pas pu être supprimé...");
                }
            }
            ShowClients();
        }
        /// <summary>
        ///  Récupère les clients et les affiche
        /// </summary>
        public void ShowClients()
        {
            MySqlCommand command = new MySqlCommand("SELECT id_client,nom,prenom,tel,mail,adresse_factu,statut FROM client;", this.connexion);
            DataTable dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());
            clientsDataGrid.ItemsSource = new DataView(dataTable);
        }
        /// <summary>
        /// Crée une chaine de caractères aléatoirement (utilisé pour générer des ID)
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        /// <summary>
        /// Créer une commande
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CreateCommande(object sender, RoutedEventArgs e)
        {
            string id_commande = RandomString(20); // Génére l'id de commande 
            string id_client = commandeClientBox.Text; // Récupère les valeurs de tous les champs de la nouvelle commande
            string id_boutique = commandeBoutiqueBox.Text;
            string adresse_livraison = commandeAdresseBox.Text;
            bool est_standard = commandeTypeBox.SelectedIndex == 0;
            string id_bouquet;
            if (est_standard)
            {
                id_bouquet = commandeBouquetStandardBox.Text;
            }
            else
            {
                // Si la commande est personalisée, on créé un nouveau bouquet perso et on l'ajoute à la table
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

            // Enfin on insert la commande dans sa table

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

        /// <summary>
        /// Cette fonction est appelée à chaque fois que le Tab de notre TabControl est changé
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.currentPage == (string)((TabItem)tabControl?.SelectedItem)?.Header) return;
            this.currentPage = (string)((TabItem)tabControl.SelectedItem).Header;
            //initialisation des données que l'on va réutiliser plusieurs fois après
            DataTable dataTable;
            MySqlCommand command;
            MySqlDataReader reader;
            switch (this.currentPage)
            {
                case "Clients":
                    ShowClients();
                    break;
                case "Etat des Stocks":
                    //requête sql permettant d'avoir accès au éléments des produits dont la quantité est inférieur à 10 (autrement dit accède à tous les éléments qui sont sous le seuil d'alerte)
                    command = new MySqlCommand("SELECT p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite, b.id_boutique FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique  WHERE s.quantite < 10;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    produitsrup_DataGrid.ItemsSource = new DataView(dataTable);
                    //requête sql qui permet d'afficher tous les produits 
                    command = new MySqlCommand("SELECT p.id_produit, p.nom, p.prix, p.disponibilite, s.quantite, b.id_boutique FROM produit p JOIN stock s ON p.id_produit = s.id_produit JOIN boutique b ON b.id_boutique = s.id_boutique;", this.connexion);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    produitsDataGrid.ItemsSource = new DataView(dataTable);
                    break;
                case "Commandes":
                    ShowCommandes();
                    break;
                case "Statistiques":
                    //Calcul du prix moyen du bouquet acheté
                    command = new MySqlCommand("select avg(prix) from bouquet_standard;", this.connexion);
                    reader = command.ExecuteReader();
                    if (reader.Read() && reader.FieldCount > 0)
                    {
                        prix_moyenTxt.Text = reader.GetValue(0).ToString();
                    }
                    reader.Close();
                    //Quel est le meilleur client du mois
                    command = new MySqlCommand("select id_client, SUM(prix_tot) as somme_total_depense from commande where Month(date_commande) = Month(now()) " +
                        "Group by id_client Order by somme_total_depense DESC limit 1; ", this.connexion);
                    reader = command.ExecuteReader();
                    if (reader.Read() && reader.FieldCount > 1)
                    {
                        best_cid_moisGrid.Text = reader.GetValue(0).ToString();
                        best_ctot_moisGrid.Text = reader.GetValue(1).ToString();
                    }
                    reader.Close();
                    //Quel est le meilleur client de l'année
                    command = new MySqlCommand("select id_client, SUM(prix_tot) as somme_total_depense from commande where Year(date_commande) = Year(now()) " +
                        "Group by id_client Order by somme_total_depense DESC limit 1;", this.connexion);
                    reader = command.ExecuteReader();
                    if (reader.Read() && reader.FieldCount > 1)
                    {
                        best_cid_anneeGrid.Text = reader.GetValue(0).ToString();
                        best_ctot_anneeGrid.Text = reader.GetValue(1).ToString();
                    }
                    reader.Close();
                    //Quel est le bouquet standard qui a eu le plus de succès ? 
                    command = new MySqlCommand("SELECT bs.nom, COUNT(*) AS total_commandes FROM bouquet_standard bs JOIN commande c ON bs.id_bs = c.id_bouquet " +
                        "WHERE c.est_standard = true GROUP BY bs.id_bs ORDER BY total_commandes DESC LIMIT 1; ", this.connexion);
                    reader = command.ExecuteReader();
                    if (reader.Read() && reader.FieldCount > 1)
                    {
                        bs_nom_sucessGrid.Text = reader.GetValue(0).ToString();
                        bs_nb_vendu_sucessGrid.Text = reader.GetValue(1).ToString();
                    }
                    reader.Close();
                    //Quel est le magasin qui a généré le plus de chiffre d’affaires ?
                    command = new MySqlCommand("SELECT c.id_boutique, SUM(c.prix_tot) AS chiffre_affaires FROM commande c GROUP BY c.id_boutique ORDER BY chiffre_affaires DESC LIMIT 1;", this.connexion);
                    reader = command.ExecuteReader();
                    if (reader.Read() && reader.FieldCount > 1)
                    {
                        best_idca_shopGrid.Text = reader.GetValue(0).ToString();
                        best_ca_shopGrid.Text = reader.GetValue(1).ToString();
                    }
                    reader.Close();
                    //Quelle est la fleur exotique la moins vendue ? 
                    command = new MySqlCommand("select nom, count(nom) as vente from produit natural join commande group by nom order by vente asc limit 1;", this.connexion);
                    reader = command.ExecuteReader();
                    if (reader.Read() && reader.FieldCount > 1)
                    {
                        less_sell_exoticnomGrid.Text = reader.GetValue(0).ToString();
                        less_sell_exoticGrid.Text = reader.GetValue(1).ToString();
                    }
                    command.Dispose();
                    reader.Close();
                    break;
                case "Déconnexion":
                    //Permet de revenir au formulaire de connexion si le vendeur souhaite se déconnecter de la page actuelle
                    MainWindow main = new MainWindow();
                    main.Show();
                    Window.GetWindow(this).Close();
                    break;
                default:
                    break;
            };
        }
        /// <summary>
        /// Fonction qui calcule le prix moyen des bouquets achetés
        /// </summary>
        /// <param name="connection"></param>
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
        /// <summary>
        /// Fonction qui permet d'exporter une requête sql en format Json une fois qu'on a cliqué sur le bouton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clickExportJson(object sender, RoutedEventArgs e)
        {
            string name_file = "exportJson.json";
            //requête sql pour obtenir les clients n’ayant pas commandé depuis plus de 6 mois
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
        /// <summary>
        /// Fonction qui permet d'exporter en format Xml les données issues d'une requête sql une fois qu'on a cliquer sur le bouton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clickExportXml(object sender, RoutedEventArgs e)
        {
            string name_file = "exportXml.xml";
            //requête sql qui permet d'obtenir les clients ayant commandé plusieurs fois durant le dernier mois 
            string mysql_query = @"SELECT c.id_client, c.nom, c.prenom, c.tel, c.mail, c.adresse_factu, c.num_carte, c.statut FROM client c
                        INNER JOIN commande cmd ON c.id_client = cmd.id_client 
                        WHERE cmd.date_commande >= DATE_SUB(NOW(), INTERVAL 1 MONTH)
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
