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

namespace TP_IRGv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public Situation current = new Situation();
        public List<Situation> Situations = new List<Situation>();
        public List<Situation> usedQs = new List<Situation>();
        public Random rint = new Random();
        public int selected = 0;
        public void FetchSituations()
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
            FetchSituations();
            InitializeComponent();

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
