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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            /*
            string videoPath = "C:/Users/TRAVAIL/Desktop/BDDI/Wpf_App_Fleur/video_fleur4.mp4";
            videoPlayer.Source = new Uri(videoPath, UriKind.RelativeOrAbsolute);
            videoPlayer.LoadedBehavior = MediaState.Manual;
            videoPlayer.MediaEnded += video_media_ended;
            */
        }
        /*
        private void video_media_ended(object sender, RoutedEventArgs e)
        {
            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Play();
        }*/
        private void BtnConnectionClick(object sender, RoutedEventArgs e)
        {
            // Informations de connexion à la base de données
            string port = "3306";
            string server = "localhost";
            string database = "fleur";

            string id_admin = "root";
            string password_admin = "root";

            string id_utili = "bozo";
            string password_utili = "bozo";
            // Chaînes de connexion pour se connecter à la base de données
            string connectionString_admin = $"server={server};port={port};database={database};uid={id_admin};password={password_admin}";
            string connectionString_utili = $"server={server};port={port};database={database};uid={id_utili};password={password_utili}";

            MySqlConnection connection_admin = new MySqlConnection(connectionString_admin);
            MySqlConnection connection_utili = new MySqlConnection(connectionString_utili);

            /*string exportPath = @"C:\Users\TRAVAIL\Desktop\BDDI\Fleur_BDDI\";
            string exportXml = "export_xml.xml";
            string exportJson = "export_json.js";
            if (!Directory.Exists(exportPath))
            {
                Console.WriteLine("File path does not match!");
                System.Environment.Exit(1);
            }*/

            //Recuperation de l'identifiant et du mot de passe sasis
            string username = "root";//txtUsername.Text;
            string password = "root";//txtPassword.Password;

            if(username == "root" && password=="root")
            {
                connection_admin.Open();
                AdminWindow admin_window = new AdminWindow(connection_admin);
                Window.GetWindow(this).Content = admin_window;
                //Close();
            }
            else if (username == "bozo" && password=="bozo")
            {
                BozoWindow bozo_window = new BozoWindow(connection_utili);
                Window.GetWindow(this).Content = bozo_window;
                //Close();
            }
            else
            {
                connection_utili.Open();
                MySqlCommand command = connection_utili.CreateCommand();
                command.CommandText = $"";
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (username == Convert.ToString(reader.GetValue(0)) && password == Convert.ToString(reader.GetValue(1)))
                    {
                        ClientWindow client_window = new ClientWindow(connection_utili);
                        Window.GetWindow(this).Content = client_window;
                        //Close();
                    }
                }
                connection_utili.Close();
                command.Dispose();
            }
        }
        /*
        private void checkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            name.PasswordChar = '\0';
        }
        private void checkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            name.PasswordChar = '*';
        }
        */
    }
}
