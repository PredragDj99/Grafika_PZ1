using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using WpfApp1.Model;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Drawing.Pen;
//using Point = WpfApp1.Model.Point;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Drawing.Size;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //pokupi poziciju klika
        public double poX, poY;
        //za poligon
        public List<double> koordinatePoX = new List<double>();
        public List<double> koordinatePoY = new List<double>();
        //undo redo clear
        List<UIElement> obrisaniListaZaBrojanje = new List<UIElement>();
        List<UIElement> ponovoIscrtaj = new List<UIElement>();
        public int numberChildren = 0;
        //za clear
        List<UIElement> saMape = new List<UIElement>();

        //tacke
        List<Point> kvadratici = new List<Point>();
        public List<PowerEntity> listaElemenataIzXML = new List<PowerEntity>();
        public List<LineEntity> listaVodova = new List<LineEntity>();
        public List<LineEntity> vodDuplikat = new List<LineEntity>();
        public Dictionary<Point, PowerEntity> dictTackaElement = new Dictionary<Point, PowerEntity>();
        public int checkMinMax = 1;
        public double noviX, noviY, praviX,praviY, praviXmin, praviXmax, praviYmin, praviYmax;
        public double razlikaMinMaxX, razlikaMinMaxY;
        //za presecanje vodova
        public List<Polyline> listaPresekaVodova = new List<Polyline>();
        public List<Rectangle> listaPresecnihTacaka = new List<Rectangle>();
        
        public MainWindow()
        {
            InitializeComponent();

            //Pravim matricu
            Point rt;
            
            for (int i = 0; i < 900; i++) // za vece ucitava dugo
            {
                for (int j = 0; j < 600; j++)
                {
                    rt = new Point(i, j);
                    kvadratici.Add(rt);
                }
            }

            UcitavanjeElemenata();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            //Crtanje elemenata koji treba da se nalaze u mrezi
            foreach (var element in listaElemenataIzXML)
            {
                ToLatLon(element.X, element.Y, 34, out noviX, out noviY);
                MestoNaCanvasu(noviX, noviY, out praviX, out praviY);

                //za svaki slucaj RECT da ne okine edit na elipsu
                Rectangle rect = new Rectangle();
                rect.Fill = element.Boja;
                rect.ToolTip = element.ToolTip; //za hover prikaze info o elementu
                rect.Width = 2;
                rect.Height = 2;

                /*
                Point mojaTacka=new Point(0,0);
                Point pomocnaTacka = new Point(praviX, praviY);
                if (kvadratici.Contains(t))
                {
                    mojaTacka = pomocnaTacka;
                }
                */
                Point mojaTacka = kvadratici.Find(pomocnaTacka => pomocnaTacka.X == praviX && pomocnaTacka.Y == praviY);
                
                int brojac = 1;
                if (!dictTackaElement.ContainsKey(mojaTacka))
                {
                    dictTackaElement.Add(mojaTacka, element);
                }
                else
                {
                    bool flag = false;
                    while (true)
                    {
                        for (double iksevi = praviX - brojac * 3; iksevi <= praviX + brojac * 3; iksevi += 3) //opet na oba 3 da se ne bi preklapali
                        {
                            if (iksevi < 0)
                                continue;
                            for (double ipsiloni = praviY - brojac * 3; ipsiloni <= praviY + brojac * 3; ipsiloni += 3)
                            {
                                if (ipsiloni < 0)
                                    continue;
                                mojaTacka = kvadratici.Find(t => t.X == iksevi && t.Y == ipsiloni);
                                if (!dictTackaElement.ContainsKey(mojaTacka))
                                {
                                    dictTackaElement.Add(mojaTacka, element);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                                break;
                        }
                        if (flag)
                            break;

                        brojac++;
                    }
                }               
                
                //ovde sam nesto smuljao jer se slika rotira cudno, ali je proslo
                Canvas.SetBottom(rect, mojaTacka.X);
                Canvas.SetLeft(rect, mojaTacka.Y);
                canvas.Children.Add(rect);               
            }

            //crtanje vodova
            foreach (LineEntity line in listaVodova)
            {
                Point start, end;
                pronadiTacke(line, out start, out end);

                if (start.X != end.X)
                {
                    
                    Polyline polyline = new Polyline();
                    polyline.Stroke = Brushes.LightSkyBlue;
                    polyline.StrokeThickness = 0.5;

                    //srednji point da budu pod pravim uglom
                    // 1 da bi bilo u sredini
                    Point p1 = new Point(1 + start.Y, 600 - 1 - start.X);
                    Point p2 = new Point(1 + start.Y, 600 - 1 - end.X);
                    Point p3 = new Point(1 + end.Y, 600 - 1 - end.X);
                    polyline.Points.Add(p1);
                    polyline.Points.Add(p2);
                    polyline.Points.Add(p3);
                    // polyline.Points.Add(new Point(1+start.Y, 600-1-start.X));
                    // polyline.Points.Add(new Point(1+start.Y, 600-1-end.X));
                    // polyline.Points.Add(new Point(1+end.Y, 600-1-end.X));
                    polyline.ToolTip = "Line\nID: " + line.Id + " Name: " + line.Name;                    

                    polyline.MouseRightButtonDown += promeniBoju_MouseRightButtonDown;
                    
                    //ako ima presek nacrtaj
                    mozdaPresek(p1, p2, p3, polyline);
                    canvas.Children.Add(polyline);
                }
            }

            //obrisi duplikate tj. vodove koji se preklapaju sa nekim drugim vodom
            foreach (Polyline pl in listaPresekaVodova)
            {
                if (!canvas.Children.Contains(pl))
                {
                    canvas.Children.Remove(pl);
                }
            }

            // ovo mi treba za undo i redo da ne bih clearovao pocetnu mapu
            numberChildren = canvas.Children.Count;
        }

        private void mozdaPresek(Point p1,Point p2, Point p3, Polyline polyline)
        {
            //linija preklapanja
            Line l1 = new Line();
            Line l2 = new Line();
            l1.X1 = p1.X;
            l1.Y1 = p1.Y;
            l1.X2 = p2.X;
            l1.Y2 = p2.Y;
            l2.X1 = p2.X;
            l2.Y1 = p2.Y;
            l2.X2 = p3.X;
            l2.Y2 = p3.Y;

            l1.Fill = Brushes.DarkRed;
            l2.Fill = Brushes.DarkRed;
            l1.StrokeThickness = 1;
            l1.Stroke = Brushes.DarkRed;
            l2.StrokeThickness = 1;
            l2.Stroke = Brushes.DarkRed;

            //da li postoji neki element ciji je jedan LINE isti kao od nekog drugog
            foreach (UIElement el in canvas.Children)
            {
                if (el.GetType() == typeof(Polyline))
                {
                    //preklapanje vodova jer imaju tu liniju istu
                    Polyline pl = (Polyline)el;
                    if (pl.Points.Contains(p1) && pl.Points.Contains(p2))
                    {
                        listaPresekaVodova.Add(pl);
                    }
                    if (pl.Points.Contains(p2) && pl.Points.Contains(p3))
                    {
                        listaPresekaVodova.Add(pl);
                    }

                    //presecne tacke
                    //polyline.Stroke  ima puno varijanti
                    //polyline.TouchesDirectlyOver(pl);
                    //if (polyline.TouchesOver)
                }
            }
            //za presecne tacke
            /*
             Rectangle rect = new Rectangle();
            rect.Fill = Brushes.DarkRed;
            rect.Width = 10;
            rect.Height = 10;
            rect.Margin = new Thickness(p1.X,p1.Y,0,0);
            Rectangle rect2 = new Rectangle();
            rect2.Fill = Brushes.DarkRed;
            rect2.Width = 10;
            rect2.Height = 10;
            rect2.Margin = new Thickness(p2.X, p2.Y, 0, 0);
             */
        }

        private void promeniBoju_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Polyline mojVod = (Polyline)sender;
            Point p1 = new Point();
            Point p2 = new Point();
            p1 = mojVod.Points.First();
            p2 = mojVod.Points.ElementAt(mojVod.Points.Count-1);

            Rectangle r = new Rectangle();
            r.Fill = Brushes.Gold;
            r.Width = 3;
            r.Height = 3;
            Canvas.SetBottom(r, 600-1.5-p1.Y);
            Canvas.SetLeft(r, -1.5+p1.X);
            canvas.Children.Add(r);

            Rectangle r2 = new Rectangle();
            r2.Fill = Brushes.Gold;
            r2.Width = 3;
            r2.Height = 3;
            Canvas.SetBottom(r2, 600-1.5-p2.Y);
            Canvas.SetLeft(r2, -1.5+p2.X);
            canvas.Children.Add(r2);
        }

        //trazim tacke za vod
        private void pronadiTacke(LineEntity le, out Point start, out Point end)
        {
            PowerEntity elem;

            elem = listaElemenataIzXML.Find(x => x.Id == le.FirstEnd);
            start = dictTackaElement.Where(x => x.Value == elem).First().Key;

            elem = listaElemenataIzXML.Find(x => x.Id == le.SecondEnd);
            end = dictTackaElement.Where(x => x.Value == elem).First().Key;
        }

        // eventualno ovde da se uzmu min i max granica NS https://www.findlatitudeandlongitude.com/?lat=36.127041&lon=-115.1766&zoom=&map_type=ROADMAP
        private void Provera(double noviX, double noviY)
        {
            //treba mi zbog skaliranja
            if(checkMinMax == 1)
            {
                praviXmax = noviX;
                praviYmax = noviY;
                praviXmin = noviX;
                praviYmin = noviY;

                checkMinMax++;
            }
            else
            {
                //proveraMAX
                if (noviX > praviXmax)
                {
                    praviXmax = noviX;
                }

                if(noviY > praviYmax)
                {
                    praviYmax = noviY;
                }

                //proveraMIN
                if (noviX < praviXmin)
                {
                    praviXmin = noviX;
                }

                if (noviY < praviYmin)
                {
                    praviYmin = noviY;
                }
            }

            razlikaMinMaxX = (praviXmax - praviXmin) * 100;
            razlikaMinMaxY = (praviYmax - praviYmin) *100; 
        }

        // daje mi koordinate za canvas
        private void MestoNaCanvasu(double noviX, double noviY, out double praviX, out double praviY)
        {
            //razvlacim sliku preko canvasa
            double odstojanjeX = 200 / razlikaMinMaxX;
            double odstojanjeY = 200 / razlikaMinMaxY;

            praviX = Math.Round((noviX - praviXmin) * 100 * odstojanjeX); //uspelo, ali se preklapaju
            praviY = Math.Round((noviY - praviYmin) * 100 * odstojanjeY);

            //na kraju da bi se se tacke toliko pomerile po datoj osi, tj. onaj razmak
            // pa se nece preklapati
            praviX = praviX * 3; //---ovo ce mi biti po visini
            praviY = praviY * 3; //--- ovo ce mi biti po sirini
            //                        jer sam obrnuo u iscrtavanju
        }

        private void UcitavanjeElemenata()
        {
            // ---------Ucitavam elemente iz xml-a
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");
            XmlNodeList nodeList;

            //substations - trafostanice
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity subEn = new SubstationEntity();
                subEn.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                subEn.Name = node.SelectSingleNode("Name").InnerText;
                subEn.X = double.Parse(node.SelectSingleNode("X").InnerText);
                subEn.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                subEn.ToolTip = "Substation\nID: " + subEn.Id + "  Name: " + subEn.Name;
                listaElemenataIzXML.Add(subEn);

                ToLatLon(subEn.X, subEn.Y, 34, out noviX, out noviY);
                Provera(noviX, noviY);
            }

            //svicevi
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity sw = new SwitchEntity();
                sw.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sw.Name = node.SelectSingleNode("Name").InnerText;
                sw.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sw.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                sw.Status = node.SelectSingleNode("Status").InnerText;
                sw.ToolTip = "Switch\nID: " + sw.Id + "  Name: " + sw.Name + " Status: " + sw.Status;
                listaElemenataIzXML.Add(sw);

                ToLatLon(sw.X, sw.Y, 34, out noviX, out noviY);
                Provera(noviX, noviY);
            }

            //nodovi
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nod = new NodeEntity();
                nod.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nod.Name = node.SelectSingleNode("Name").InnerText;
                nod.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nod.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                nod.ToolTip = "Node\nID: " + nod.Id + "  Name: " + nod.Name;
                listaElemenataIzXML.Add(nod);

                ToLatLon(nod.X, nod.Y, 34, out noviX, out noviY);
                Provera(noviX, noviY);
            }

            //ucitavanje vodova ->  u xml first end i second end
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity l = new LineEntity();
                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    l.IsUnderground = true;
                }
                else
                {
                    l.IsUnderground = false;
                }
                l.R = float.Parse(node.SelectSingleNode("R").InnerText);
                l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                l.LineType = node.SelectSingleNode("LineType").InnerText;
                l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                // da li postoje firstEnd i secondEnd medju entitetima
                // ako ne ignorisi vod
                if (listaElemenataIzXML.Any(x => x.Id == l.FirstEnd))
                {
                    if (listaElemenataIzXML.Any(x => x.Id == l.SecondEnd))
                    {
                        listaVodova.Add(l);
                    }
                }

                //brisanje duplikata
                while (listaVodova.Any(x => x.Id != l.Id && x.FirstEnd == l.FirstEnd && x.SecondEnd == l.SecondEnd))
                {
                    vodDuplikat = listaVodova.FindAll(x => x.Id != l.Id && x.FirstEnd == l.FirstEnd && x.SecondEnd == l.SecondEnd);
                    foreach (LineEntity dupli in vodDuplikat)
                    {
                        listaVodova.Remove(dupli);
                    }
                    vodDuplikat.Clear();
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Drugi deo projekta funkcionalnost sa oblicima,undo,redo...

        private void LeviPromeniNesto_Click(object sender, MouseButtonEventArgs e)
        {
            //Update za kliknut objekat
            if (e.OriginalSource is Ellipse)
            {
                Ellipse ClickedRectangle = (Ellipse)e.OriginalSource;
                
                //otvori ovu elipsu
                canvas.Children.Remove(ClickedRectangle);
                
                //za textBlock
                string bojaTeksta="Black", samTekst="nekiTekst";
                foreach(FrameworkElement item in canvas.Children)
                {
                    if (item.Name == ClickedRectangle.Name + "eltb") //treba mi item.Name, a Name(F12) vodi na FrameworkElement
                    {
                        canvas.Children.Remove(item);
                        bojaTeksta = ((TextBlock)item).Foreground.ToString();
                        samTekst = ((TextBlock)item).Text;
                        break;
                    } 
                }

                EditElipsa editElipsa = new EditElipsa(ClickedRectangle.Height, ClickedRectangle.Width, ClickedRectangle.StrokeThickness, ClickedRectangle.Fill, bojaTeksta,samTekst);
                editElipsa.Show();

            }
            else if(e.OriginalSource is Polygon)
            {
                Polygon ClickedRectangle = (Polygon)e.OriginalSource;

                canvas.Children.Remove(ClickedRectangle);

                //za textBlock
                string bojaTeksta = "Black", samTekst = "nekiTekst";
                foreach (FrameworkElement item in canvas.Children)
                {
                    if (item.Name == ClickedRectangle.Name + "pgtb") //treba mi item.Name, a Name(F12) vodi na FrameworkElement
                    {
                        canvas.Children.Remove(item);
                        bojaTeksta = ((TextBlock)item).Foreground.ToString();
                        samTekst = ((TextBlock)item).Text;
                        break;
                    }
                }

                EditPolygon editPoligon = new EditPolygon(ClickedRectangle.StrokeThickness, ClickedRectangle.Fill.ToString(), bojaTeksta, samTekst, ClickedRectangle.Points);
                editPoligon.Show();
            }
            else if (e.OriginalSource is TextBlock)
            {
                TextBlock ClickedRectangle = (TextBlock)e.OriginalSource;

                string slova = ClickedRectangle.Name;
                slova = slova.Substring(8, slova.Length - 8);
                if (slova != "pgtb" && slova != "eltb")
                {
                    //otvori ovaj tekst
                    canvas.Children.Remove(ClickedRectangle);
                    EditText editTekst = new EditText(ClickedRectangle.FontSize, ClickedRectangle.Foreground, ClickedRectangle.Text);
                    editTekst.Show();
                }
            }
        }

        private void LeviPoligon_Click(object sender, MouseButtonEventArgs e)
        {
            Poligon poligonCrtez = new Poligon();

            //grupisem da samo 1 moze da se izabere
            int i = 1;

            if (EllipseChecked.IsChecked == true && PolygonChecked.IsChecked == true || EllipseChecked.IsChecked == true && TextChecked.IsChecked == true ||
                EllipseChecked.IsChecked == true && PolygonChecked.IsChecked == true && TextChecked.IsChecked == true ||
                PolygonChecked.IsChecked == true && TextChecked.IsChecked == true)
            {
                i = 2;
                MessageBox.Show("Selektujte iskljucivo jednu opciju", "Greska!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (i==1 && PolygonChecked.IsChecked==true && koordinatePoX.Count >= 3 )
            {
                poligonCrtez.Show();
            }
            else if(PolygonChecked.IsChecked == true)
            {
                MessageBox.Show("Morate izvrsiti desni klik bar 3 puta ako zelite da dodate nov poligon", "Greska!", MessageBoxButton.OK,MessageBoxImage.Information);
                koordinatePoX.Clear();
                koordinatePoY.Clear();
            }
        }

        private void Right_ClickBiloGde(object sender, MouseButtonEventArgs e)
        {
            //grupisem da samo 1 moze da se izabere
            int i = 1;

            if (EllipseChecked.IsChecked == true && PolygonChecked.IsChecked == true || EllipseChecked.IsChecked == true && TextChecked.IsChecked == true ||
                EllipseChecked.IsChecked == true && PolygonChecked.IsChecked == true && TextChecked.IsChecked == true ||
                PolygonChecked.IsChecked == true && TextChecked.IsChecked == true)
            {
                i = 2;
                MessageBox.Show("Selektujte iskljucivo jednu opciju", "Greska!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (i == 1)
            {
                if (EllipseChecked.IsChecked == true)
                {
                    Elipsa elipsaCrtez = new Elipsa();

                    //Pokupio ih je
                    poX = Mouse.GetPosition(canvas).X;
                    poY = Mouse.GetPosition(canvas).Y;

                    elipsaCrtez.Show();
                }
                else if (PolygonChecked.IsChecked == true)
                {
                    poX = Mouse.GetPosition(canvas).X;
                    poY = Mouse.GetPosition(canvas).Y;

                    koordinatePoX.Add(poX);
                    koordinatePoY.Add(poY);
                }
                else if (TextChecked.IsChecked == true)
                {
                    AddText dodajTekstCrtez = new AddText();

                    poX = Mouse.GetPosition(canvas).X;
                    poY = Mouse.GetPosition(canvas).Y;

                    dodajTekstCrtez.Show();
                }
            }
        }

        //da bi obrisao tekst unutar elementa mora 2 puta undo
        private void Undo_Click(object sender, RoutedEventArgs e)
        {     
            
            if (canvas.Children.Count > 0)
            {
                obrisaniListaZaBrojanje.Add(canvas.Children[canvas.Children.Count - 1]);
                canvas.Children.Remove(canvas.Children[canvas.Children.Count - 1]);
            }
            
            if (canvas.Children.Count != numberChildren)
            {
                for(int i = 0; i < ponovoIscrtaj.Count; i++)
                {
                    if(ponovoIscrtaj[i] != null)
                        canvas.Children.Add(ponovoIscrtaj[i]);
                }
            }   
            
            for(int i=0; i<ponovoIscrtaj.Count; i++)
            {
                ponovoIscrtaj[i] = null;
            }
        }

        //da bi vratio tekst unutar elementa mora 2 puta redo
        private void RedO_Click(object sender, RoutedEventArgs e)
        {       
            
            if(obrisaniListaZaBrojanje.Count > 0)
            {
                //vraca na prethodnu i onda je brise sa liste
                canvas.Children.Add(obrisaniListaZaBrojanje[obrisaniListaZaBrojanje.Count -1]);
                obrisaniListaZaBrojanje.RemoveAt(obrisaniListaZaBrojanje.Count-1);
            }
        }
        
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            //brise samo one iscrtane objekte, a ne celu mapu
            if (canvas.Children.Count > 0)
            {
                // da ne bih obrisao mapu
                foreach(UIElement jedanOdElemenata in canvas.Children)
                {
                    saMape.Add(jedanOdElemenata);
                }

                // cuvam one koje zelim da crtam ponovo
                if (canvas.Children.Count > numberChildren)
                {
                    for(int i = numberChildren; i<canvas.Children.Count; i++)
                    {
                        ponovoIscrtaj.Add(canvas.Children[i]);
                    }
                }
                canvas.Children.Clear();

                //ponovo crtam pocetnu mapu
                for (int i = 0; i < numberChildren; i++)
                {
                    canvas.Children.Add(saMape[i]);
                }
                saMape.Clear();

                numberChildren = canvas.Children.Count;
            }
        }

        //From UTM to Latitude and longitude in decimal
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
    }
}
