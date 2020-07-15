using Newtonsoft.Json;
using RiotSharp;
using RiotSharp.Endpoints.ChampionEndpoint;
using RiotSharp.Endpoints.ChampionMasteryEndpoint;
using RiotSharp.Endpoints.StaticDataEndpoint.Champion;
using RiotSharp.Endpoints.SummonerEndpoint;
using RiotSharp.Misc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace LeagueMasteryChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string version = "";
        static Dictionary<string, BitmapImage> champImages = new Dictionary<string, BitmapImage>();
        public List<Champ> champList { get; set; }

        public MasteryPodium masteryWindow;

        RiotApi riotApi = null;
        public MainWindow()
        {
            InitializeComponent();
            LocationChanged += MainWindow_LocationChanged;
            riotApi = RiotApi.GetDevelopmentInstance("YOUR_API_KEY");
            champList = new List<Champ>();
            masteryWindow = new MasteryPodium();
        }

        private void FillChampions(Summoner summoner)
        {
            using (WebClient client = new WebClient())
            {
                string result = client.DownloadString("https://ddragon.leagueoflegends.com/api/versions.json");
                List<string> jsonDecoded = JsonConvert.DeserializeObject<List<string>>(result);
                version = jsonDecoded.First();
            }

            if (version == "")
            {
                // show error
                MessageBox.Show("Failed to retrieve version");
                return;
            }

            List<ChampionMastery> masteries = riotApi.ChampionMastery.GetChampionMasteriesAsync(Region.Euw, summoner.Id).Result;

            ChampionListStatic list = riotApi.StaticData.Champions.GetAllAsync(version).Result;
            foreach (var staticChamp in list.Champions.Values)
            {
                Champ champ = new Champ();
                ChampionMastery mastery = null;
                try
                {
                    mastery = masteries.Single(x => x.ChampionId == staticChamp.Id);
                } catch {
                    champ = new Champ
                    {
                        Name = staticChamp.Name,
                        Id = staticChamp.Id,
                        summonerId = summoner.Id
                    };
                    champList.Add(champ);
                    continue;
                }

                champ = new Champ
                {
                    Name = staticChamp.Name,
                    Id = mastery.ChampionId,
                    masteryLevel = mastery.ChampionLevel,
                    masteryPoints = mastery.ChampionPoints,
                    masteryPointsUntilNextLvl = mastery.ChampionPointsUntilNextLevel,
                    summonerId = summoner.Id
                };
                champList.Add(champ);
            }

            Dispatcher.Invoke(() => { ChampionListBox.ItemsSource = champList; });
        }

        private void ChampionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox box = sender as ListBox;
            if (box == null)
                return;

            Champ item = box.SelectedItem as Champ;
            if (item == null)
                return;

            ChampIcon.Source = LoadChampImage(item.Name);
            e.Handled = true;
        }

        private BitmapImage LoadChampImage(string name)
        {
            BitmapImage img;
            if (champImages.TryGetValue(name, out img))
            {
                return img;
            }

            img = GetChampionImage(version, name + ".png");
            champImages.Add(name, img);
            return img;
        }

        private BitmapImage GetChampionImage(string version, string name)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    Stream stream = client.OpenRead($"http://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{name}");
                    if (stream.CanRead)
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = new System.IO.MemoryStream();
                        stream.CopyTo(image.StreamSource);
                        image.EndInit();

                        stream.Close();
                        stream.Dispose();
                        image.StreamSource.Close();
                        image.StreamSource.Dispose();
                        return image;
                    }
                }
            } catch {}
            return null;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DragMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return)
                return;

            TextBox box = sender as TextBox;
            if (box == null)
                return;

            foreach (Champ champ in ChampionListBox.Items)
            {
                if (champ.Name.ToLower().Replace("'", "") == box.Text.ToLower().Replace("'", ""))
                {
                    ChampionListBox.SelectedItem = champ;
                    ChampionListBox.ScrollIntoView(champ);
                    break;
                }
            }
        }

        private void SortingChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (box == null)
                return;

            if (champList == null)
                return;

            string type = (box.SelectedItem as ComboBoxItem).Uid;
            if (type == "Name")
            {
                champList.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            else if (type == "LowerMasteryPts")
            {
                champList.Sort((a, b) => a.masteryPoints.CompareTo(b.masteryPoints));
            }
            else if (type == "HigherMasteryPts")
            {
                champList.Sort((a, b) => b.masteryPoints.CompareTo(a.masteryPoints));
            }
            else if (type == "LowerMasteryLvl")
            {
                champList.Sort((a, b) => a.masteryLevel.CompareTo(b.masteryLevel));
            }
            else if (type == "HigherMasteryLvl")
            {
                champList.Sort((a, b) => b.masteryLevel.CompareTo(a.masteryLevel));
            }
            ChampionListBox.ItemsSource = null;
            ChampionListBox.ItemsSource = champList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                var dialog = new Dialog() { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    if (ParseSummoner(dialog.ResponseText))
                    {
                        return;
                    }
                }
                else
                {
                    this.Close();
                    return;
                }
            }
        }

        private bool ParseSummoner(string name)
        {
            try
            {
                Summoner summoner = riotApi.Summoner.GetSummonerByNameAsync(Region.Euw, name).Result;
                LoadingWindow win = new LoadingWindow() { Owner = this };
                win.Show();
                Task.Run(() => { FillChampions(summoner); UpdatePodium(); Dispatcher.Invoke(() => win.Close()); RefreshMastery(); });
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }

        private void RefreshMastery()
        {
            while (true)
            {
                Thread.Sleep(60000);

                // Refresh
                List<ChampionMastery> masteries = null;
                try
                {
                    masteries = riotApi.ChampionMastery.GetChampionMasteriesAsync(Region.Euw, champList.First().summonerId).Result;
                }
                catch
                {
                    continue;
                }

                ChampionMastery mastery = null;
                foreach (Champ champ in champList)
                {
                    try
                    {
                        mastery = masteries.Single(x => x.ChampionId == champ.Id);
                    }
                    catch { continue; }

                    if (champ.masteryLevel != mastery.ChampionLevel)
                    {
                        champ.masteryPointsUntilNextLvl = mastery.ChampionPointsUntilNextLevel;
                    }
                    champ.masteryLevel = mastery.ChampionLevel;
                    champ.masteryPoints = mastery.ChampionPoints;
                }

                UpdatePodium();

                Dispatcher.Invoke(() =>
                {
                    ChampionListBox.ItemsSource = null;
                    ChampionListBox.ItemsSource = champList;
                });
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            foreach (Window win in this.OwnedWindows)
            {
                win.Top = this.Top + ((this.ActualHeight - win.ActualHeight) / 2);
                win.Left = this.Left + ((this.ActualWidth - win.ActualWidth) / 2);
            }
        }

        private void PopoutMastery(object sender, RoutedEventArgs e)
        {
            masteryWindow.Owner = this;
            masteryWindow.Show();
        }

        private void UpdatePodium()
        {
            List<Champ> newList = new List<Champ>();
            foreach (var champ in champList)
            {
                if (champ.masteryLevel < 5)
                {
                    newList.Add(champ);
                }
            }

            newList.Sort((a, b) => b.masteryPoints.CompareTo(a.masteryPoints));

            Champ topChamp = newList[0];
            if (topChamp != null)
            {
                masteryWindow.Dispatcher.Invoke(() => {
                    masteryWindow.Top1Icon.Source = LoadChampImage(topChamp.Name);
                    masteryWindow.Top1Pts.Text = topChamp.masteryPoints + " points";
                    masteryWindow.Top1ToGo.Text = topChamp.masteryPointsUntilNextLvl + " to go";
                });
            }

            topChamp = newList[1];
            if (topChamp != null)
            {
                masteryWindow.Dispatcher.Invoke(() => {
                    masteryWindow.Top2Icon.Source = LoadChampImage(topChamp.Name);
                    masteryWindow.Top2Pts.Text = topChamp.masteryPoints + " points";
                    masteryWindow.Top2ToGo.Text = topChamp.masteryPointsUntilNextLvl + " to go";
                });
            }

            topChamp = newList[2];
            if (topChamp != null)
            {
                masteryWindow.Dispatcher.Invoke(() => {
                    masteryWindow.Top3Icon.Source = LoadChampImage(topChamp.Name);
                    masteryWindow.Top3Pts.Text = topChamp.masteryPoints + " points";
                    masteryWindow.Top3ToGo.Text = topChamp.masteryPointsUntilNextLvl + " to go";
                });
            }
        }
        //
    }
}
