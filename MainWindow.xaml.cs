using System;
using System.IO;
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

using System.ComponentModel;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Data = Google.Apis.Sheets.v4.Data;
using Google.Apis.Auth.OAuth2.Responses;
using System.Windows.Ink;
using System.Runtime.InteropServices;

namespace TP_IRGv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        static string ApplicationName = "TP-IRGv2";

        public Situation current = new Situation();
        public List<Situation> Situations = new List<Situation>();
        public List<Situation> usedQs = new List<Situation>();
        public Random rint = new Random();
        public int selected = 0;

        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------
             ██████╗  ██████╗  ██████╗  ██████╗ ██╗     ███████╗     █████╗ ██████╗ ██╗
            ██╔════╝ ██╔═══██╗██╔═══██╗██╔════╝ ██║     ██╔════╝    ██╔══██╗██╔══██╗██║
            ██║  ███╗██║   ██║██║   ██║██║  ███╗██║     █████╗      ███████║██████╔╝██║
            ██║   ██║██║   ██║██║   ██║██║   ██║██║     ██╔══╝      ██╔══██║██╔═══╝ ██║
            ╚██████╔╝╚██████╔╝╚██████╔╝╚██████╔╝███████╗███████╗    ██║  ██║██║     ██║
             ╚═════╝  ╚═════╝  ╚═════╝  ╚═════╝ ╚══════╝╚══════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        All the bits for dealing with Google's API and OAuth2
        */

        static bool online = false;
        static string SheetID = "";
        static bool deltoken = true;
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string Directory = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        static UserCredential credential;

        static SheetsService Service()
        {
            //establishes a service with the API and passes authentication.
            //this is needed for any interraction with the API
            using (var stream = new FileStream(Directory + @"\credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "Tokens";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            { HttpClientInitializer = credential, ApplicationName = ApplicationName, });

            return service;
        }

        static List<string> Read(string range)
        {
            //assembles and executes API request
            SpreadsheetsResource.ValuesResource.GetRequest request = Service().Spreadsheets.Values.Get(SheetID, range);
            ValueRange response = new ValueRange();
            try { response = request.Execute(); }
            catch { MessageBox.Show("ERROR: Failed to read from" + range + " // " + response); List<string> x = new List<string>(); return x; };

            //interprets response
            List<string> output = new List<string>();
            if (response.Values != null && response.Values.Count > 0)
            {
                IList<IList<object>> b = response.Values;
                int row = 0;
                foreach (IList<object> x in b)
                {
                    foreach (string val in x)
                    {
                        output.Add(val.ToString());
                        row++;
                    }
                }
            }
            else { MessageBox.Show("Error: Sheet query returned null"); }
            return output;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure()
        {
            string[] rawfile = File.ReadAllLines("config.txt");
            for (int i = 0; i < rawfile.Length; i++)
            {
                string[] s = new string[3];
                s = rawfile[i].Split("=");
                rawfile[i] = s[1];
            }

            Status.Content = "Offline";

            try
            {
                if (rawfile[0] == "true") { online = true; SheetID = rawfile[1]; Status.Content = "Online"; }
                if (rawfile[2] == "false") { deltoken = false; }
            }
            catch (Exception) { MessageBox.Show("Could not configure properly (Check config.txt), Continuing in offline mode"); }
        }

        public void FetchSituations()
        {
            if (online)
            {
                List<string> temp = Read("Situations!G2");
                int total = int.Parse(temp[0]);

                temp = Read("Situations!A2:F" + (total + 1));

                for (int i = 0; i < temp.Count; i = i + 6)
                {
                    Situation x = new Situation();
                    x.situation = temp[i];
                    x.opt1 = temp[i + 1];
                    x.opt2 = temp[i + 2];
                    x.opt3 = temp[i + 3];
                    x.opt4 = temp[i + 4];
                    x.answer = int.Parse(temp[i + 5]);

                    Situations.Add(x);
                }
            }
            else { FetchSituationsFromFile(); }
        }

        public void FetchSituationsFromFile()
        {
            string[] rawfile = File.ReadAllLines("situations.txt");
            foreach (string data in rawfile)
            {
                Situation temp = new Situation();
                temp.Load(data);
                Situations.Add(temp);
            }
        }

        public void PresentNextSituation()
        {
            PresentSituation(Situations[rint.Next(0, Situations.Count - 1)]);
        }

        public void PresentSituation(Situation x)
        {
            current = x;
            Situation.Content = x.situation;
            option1.Content = x.opt1;
            option2.Content = x.opt2;
            option3.Content = x.opt3;
            option4.Content = x.opt4;
            Situations.Remove(x);
        }

        public Button FindOptionButton(int x)
        {
            if (x == 1) { return option1; }
            if (x == 2) { return option2; }
            if (x == 3) { return option3; }
            if (x == 4) { return option4; }
            else { MessageBox.Show("ERROR: FindOption(" + x.ToString() + ")", "ERROR"); return null; }
        }

        public void OptionSelect(int x, bool reset)
        {
            selected = x;
            Brush y = Submit.Background;
            option1.Background = y;
            option2.Background = y;
            option3.Background = y;
            option4.Background = y;
            if (!reset)
            {
                FindOptionButton(x).Background = Brushes.Gray;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Configure();
            FetchSituations();

            PresentNextSituation();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            PresentNextSituation();
        }

        private void option1_Click(object sender, RoutedEventArgs e)
        {
            OptionSelect(1, false);
        }

        private void option2_Click(object sender, RoutedEventArgs e)
        {
            OptionSelect(2, false);
        }

        private void option3_Click(object sender, RoutedEventArgs e)
        {
            OptionSelect(3, false);
        }

        private void option4_Click(object sender, RoutedEventArgs e)
        {
            OptionSelect(4, false);
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (selected == current.answer)
            {
                MessageBox.Show("Correct");
                PresentNextSituation();
            }
            else if (selected != 0)
            {
                MessageBox.Show("Incorrect");
            }
            OptionSelect(0, true);
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            if (deltoken)
            {
                try { File.Delete(Directory + @"\Tokens"); }
                catch (UnauthorizedAccessException) { MessageBox.Show("Token could not be deleted: Unauthorized"); }
            }
        }
    }

    public class Situation
    {
        private string raw;
        private string[] data = new string[6];

        public string situation;
        public string opt1;
        public string opt2;
        public string opt3;
        public string opt4;
        public int answer;

        public void Load(string input)
        {
            raw = input;
            data = input.Split(',');
            situation = data[0];
            opt1 = data[1];
            opt2 = data[2];
            opt3 = data[3];
            opt4 = data[4];
            answer = int.Parse(data[5]);
        }
    }
}
