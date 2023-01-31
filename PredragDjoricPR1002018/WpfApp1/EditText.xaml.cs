using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for EditText.xaml
    /// </summary>
    public partial class EditText : Window
    {
        public EditText(double fontVelicina,System.Windows.Media.Brush brush, string samTekst)
        {
            InitializeComponent();
            cmbFontSize.ItemsSource = new List<double>() { 2, 3, 4, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            cmbColor.ItemsSource = typeof(Colors).GetProperties();

            txtUnos.Text = samTekst;
            cmbFontSize.SelectedItem = fontVelicina; //rade oba
            cmbFontSize.SelectedValue = fontVelicina;

            int l = -1;
            while (true)
            {
                l++;
                cmbColor.SelectedIndex = l;
                var nekaBoja = cmbColor.SelectedValue; //dobije ime boje
                string plz = nekaBoja.ToString();
                plz = plz.Substring(27, plz.Length - 27);

                var dobioSamVrednost = new SolidColorBrush((Color)ColorConverter.ConvertFromString(plz));
                if (dobioSamVrednost.ToString() == brush.ToString())
                {
                    break;
                }
                else if (l == 500)
                {
                    break;
                }
            }
        }

        private void NacrtajTekst(object sender, RoutedEventArgs e)
        {
            if (Vaalidate())
            {
                // pozicija misa
                double left = ((MainWindow)Application.Current.MainWindow).poX;
                double top = ((MainWindow)Application.Current.MainWindow).poY;

                //tekst i boja unutar elipse
                TextBlock prosledjujemTekst = new TextBlock();
                prosledjujemTekst.Margin = new Thickness(left, top, 0, 0);
                Canvas.SetZIndex(prosledjujemTekst, 4);

                //Dodeljujem ime zbog ono poslednjeg zahteva za edit
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                var stringChars = new char[8];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                prosledjujemTekst.Name = finalString;
                //kraj

                string ispisTeksta = "";
                if (txtUnos.ToString() != "")
                {
                    ispisTeksta = txtUnos.ToString();
                    ispisTeksta = ispisTeksta.Substring(33, ispisTeksta.Length - 33);
                }
                if (cmbColor.SelectedItem != null)
                {
                    string boja2 = cmbColor.SelectedItem.ToString();
                    boja2 = boja2.Substring(27, boja2.Length - 27);
                    prosledjujemTekst.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja2));
                }
                if (cmbFontSize.SelectedItem != null)
                {
                    int velicinaFonta = 2;
                    string fontic = cmbFontSize.SelectedItem.ToString();
                    try
                    {
                        Int32.TryParse(fontic, out velicinaFonta);
                    }
                    catch (Exception excep)
                    {
                        MessageBox.Show(excep.ToString(), "Greska", MessageBoxButton.OK);
                    }
                    prosledjujemTekst.FontSize = velicinaFonta;

                }
                //Prosledjivanje teksta
                prosledjujemTekst.Text = ispisTeksta;

                ((MainWindow)Application.Current.MainWindow).canvas.Children.Add(prosledjujemTekst);

                this.Close();
            }
            else
            {
                MessageBox.Show("Popunite sva obavezna polja", "Greska!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private bool Vaalidate()
        {
            bool result = true;

            //tekst
            if (txtUnos.Text.Trim().Equals(""))
            {
                result = false;
                txtUnos.BorderBrush = Brushes.Red;
                txtUnos.BorderThickness = new Thickness(1);
                lblTekstGreska.Content = "Ne moze biti prazno";
            }
            else if (txtUnos.Text.Length > 30)
            {
                result = false;
                txtUnos.BorderBrush = Brushes.Red;
                txtUnos.BorderThickness = new Thickness(1);
                lblTekstGreska.Content = "Morate uneti tekst";
            }
            else
            {
                txtUnos.BorderBrush = Brushes.Green;
                lblTekstGreska.Content = string.Empty;
            }

            //boja
            if (cmbColor.SelectedItem == null)
            {
                result = false;
                cmbColor.BorderBrush = Brushes.Red;
                cmbColor.BorderThickness = new Thickness(1);
                lblcmbBojaGreska.Content = "Morate izabrati boju";
            }
            else
            {
                cmbColor.BorderBrush = Brushes.Green;
                lblcmbBojaGreska.Content = string.Empty;
            }

            //velicina slova
            if (cmbFontSize.SelectedItem == null)
            {
                result = false;
                cmbFontSize.BorderBrush = Brushes.Red;
                cmbFontSize.BorderThickness = new Thickness(1);
                lblcmbVelicinaGreska.Content = "Morate izabrati opciju";
            }
            else
            {
                cmbFontSize.BorderBrush = Brushes.Green;
                lblcmbVelicinaGreska.Content = string.Empty;
            }

            return result;
        }

        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    MessageBox.Show("Izaberi boju", "Greska!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
        }


        private void CmbFontSize_Changed(object sender, TextChangedEventArgs e)
        {
            if (cmbFontSize.SelectedItem != null)
            {
                try
                {

                }
                catch (Exception)
                {
                    MessageBox.Show("Izaberi velicinu fonta", "Greska!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
        }
    }
}
