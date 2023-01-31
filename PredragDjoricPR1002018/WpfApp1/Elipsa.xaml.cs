using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Elipsa.xaml
    /// </summary>
    public partial class Elipsa : Window
    {
        public Elipsa()
        {
            InitializeComponent();
            cmbColor.ItemsSource = typeof(Colors).GetProperties();
            cmbColor2.ItemsSource = typeof(Colors).GetProperties();
        }

        private void NacrtajElipsu_Click(object sender, RoutedEventArgs e)
        {
            double heightt=1, widthh=1, debljinaKonturneLinijeee=1;
            double parsiranjePoluprecnikVisina, parsiranjePoluprecnikSirina,parsiranjeKonturneLinije;

            if (validate())
            {
                //Width i Height su precnici znaci ovde se koristi 2*uneto
                if (!double.TryParse(poluprecnikHeight.Text, out parsiranjePoluprecnikVisina)){}
                heightt = parsiranjePoluprecnikVisina * 2;


                if (!double.TryParse(poluprecnikWidth.Text, out parsiranjePoluprecnikSirina)) { }
                widthh = parsiranjePoluprecnikSirina * 2;

                if (!double.TryParse(debljinaKonturneLinije.Text, out parsiranjeKonturneLinije)) { }
                debljinaKonturneLinijeee = parsiranjeKonturneLinije;

                
                Ellipse currentDot = new Ellipse();
                currentDot.StrokeThickness = debljinaKonturneLinijeee;
                Canvas.SetZIndex(currentDot, 3);
                currentDot.Height = heightt;
                currentDot.Width = widthh;
                string boja = cmbColor.SelectedItem.ToString();
                // dobijem System.Windows.Media.Color Blue -> 27 viska
                boja = boja.Substring(27, boja.Length-27); 
                //tekstUnutarElipse.Text = boja;
                currentDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja));
                currentDot.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja));

                //Dodeljujem ime zbog ono poslednjeg zahteva za edit
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                var stringChars = new char[8];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                currentDot.Name = finalString;
                //kraj

                
                // pozicija klika misa
                double left = ((MainWindow)Application.Current.MainWindow).poX;
                double top = ((MainWindow)Application.Current.MainWindow).poY;
                currentDot.Margin = new Thickness(left,top,0,0);

                //tekst i boja unutar elipse
                TextBlock prosledjujemTekst = new TextBlock();
                prosledjujemTekst.Margin = new Thickness(left, top+(heightt/2), 0, 0);
                prosledjujemTekst.FontSize = 10;
                //sada ce tekst biti na elipsi jer sam stavio na 4 layer
                Canvas.SetZIndex(prosledjujemTekst, 4);

                string tekstZaElipsu = "";
                if (tekstUnutarElipse.Text.Length != 0)
                {
                    tekstZaElipsu = tekstUnutarElipse.ToString();
                    tekstZaElipsu = tekstZaElipsu.Substring(33, tekstZaElipsu.Length - 33);
                }

                if (cmbColor2.SelectedItem != null)
                {
                    string boja2 = cmbColor2.SelectedItem.ToString();
                    boja2 = boja2.Substring(27, boja2.Length - 27);
                    prosledjujemTekst.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja2));
                }
                //Prosledjivanje teksta
                prosledjujemTekst.Text = tekstZaElipsu;
                prosledjujemTekst.Name = finalString + "eltb";

                ((MainWindow)Application.Current.MainWindow).canvas.Children.Add(prosledjujemTekst);
                ((MainWindow)Application.Current.MainWindow).canvas.Children.Add(currentDot);
           
                this.Close();         
            }
            else
            {
                MessageBox.Show("Popunite sva obavezna polja","Greska!",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            
        }

        private bool validate()
        {
            bool result = true;

            double parsiranjePoluprecnikVisina;
            double parsiranjePoluprecnikSirina;

            //Visina
            if (poluprecnikHeight.Text.Trim().Equals(""))
            {
                result = false;
                poluprecnikHeight.BorderBrush = Brushes.Red;
                poluprecnikHeight.BorderThickness = new Thickness(1);
                lblHeightGreska.Content = "Ne moze biti prazno";
            }
            else if(poluprecnikHeight.Text.Length > 10)
            {
                result = false;
                poluprecnikHeight.BorderBrush = Brushes.Red;
                poluprecnikHeight.BorderThickness = new Thickness(1);
                lblHeightGreska.Content = "Morate uneti manji broj";
            }
            else if (!double.TryParse(poluprecnikHeight.Text, out parsiranjePoluprecnikVisina))
            {
                result = false;
                poluprecnikHeight.BorderBrush = Brushes.Red;
                poluprecnikHeight.BorderThickness = new Thickness(1);
                lblHeightGreska.Content = "Unesite broj bez karaktera";
            }
            else if (parsiranjePoluprecnikVisina <= 0)
            {
                result = false;
                poluprecnikHeight.BorderBrush = Brushes.Red;
                poluprecnikHeight.BorderThickness = new Thickness(1);
                lblHeightGreska.Content = "Mora biti veci od 0";
            }
            else
            {
                poluprecnikHeight.BorderBrush = Brushes.Green;
                lblHeightGreska.Content = string.Empty;
            }

            //Sirina
            if (poluprecnikWidth.Text.Trim().Equals(""))
            {
                result = false;
                poluprecnikWidth.BorderBrush = Brushes.Red;
                poluprecnikWidth.BorderThickness = new Thickness(1);
                lblWidthGreska.Content = "Ne moze biti prazno";
            }
            else if (poluprecnikWidth.Text.Length > 10)
            {
                result = false;
                poluprecnikWidth.BorderBrush = Brushes.Red;
                poluprecnikWidth.BorderThickness = new Thickness(1);
                lblWidthGreska.Content = "Morate uneti manji broj";
            }
            else if (!double.TryParse(poluprecnikWidth.Text, out parsiranjePoluprecnikSirina))
            {
                result = false;
                poluprecnikWidth.BorderBrush = Brushes.Red;
                poluprecnikWidth.BorderThickness = new Thickness(1);
                lblWidthGreska.Content = "Unesite broj bez karaktera";
            }
            else if (parsiranjePoluprecnikSirina <= 0)
            {
                result = false;
                poluprecnikWidth.BorderBrush = Brushes.Red;
                poluprecnikWidth.BorderThickness = new Thickness(1);
                lblWidthGreska.Content = "Mora biti veci od 0";
            }
            else
            {
                poluprecnikWidth.BorderBrush = Brushes.Green;
                lblWidthGreska.Content = string.Empty;
            }

            //Debljina konturne linije
            double parsiranjeKonturneLinije;

            if (debljinaKonturneLinije.Text.Trim().Equals(""))
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Ne moze biti prazno";
            }
            else if (debljinaKonturneLinije.Text.Length > 10)
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Morate uneti manji broj";
            }
            else if (!double.TryParse(debljinaKonturneLinije.Text, out parsiranjeKonturneLinije))
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Unesite broj bez karaktera";
            }
            else if (parsiranjeKonturneLinije <= 0)
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Mora biti veci od 0";
            }
            else
            {
                debljinaKonturneLinije.BorderBrush = Brushes.Green;
                lblKonturnaLinijaGreska.Content = string.Empty;
            }

            //boja
            if(cmbColor.SelectedItem == null)
            {
                result = false;
                cmbColor.BorderBrush = Brushes.Red;
                cmbColor.BorderThickness = new Thickness(1);
                lblcmbGreska.Content = "Morate izabrati boju";
            }
            else
            {
                cmbColor.BorderBrush = Brushes.Green;
                lblcmbGreska.Content = string.Empty;
            }

            return result;
        }

        private void cmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbColor.SelectedItem != null)
            {
                try
                {
                    var izabrana = (PropertyInfo)cmbColor.SelectedItem;
                    var boja = (Color)izabrana.GetValue(null, null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Boja nije izabrana", "Greska!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
        }
    }
}
