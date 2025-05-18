using System;
using System.Collections.Generic;
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
using DrinkServiceContract;

using Utilities;
using System.Windows.Threading;
using DrinkServiceProxy;
using System.Timers;
using System.Threading;

//using System.Threading;

namespace DrinkKassaClient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        DrinkServiceAgent m_service;

        public delegate void MyMethodInvoker(DrinkButton label, Color color);
        public delegate void SetLabelTextInvoker(Label label, string Text);

        List<Guid> m_CurrentOrders = new List<Guid>();
        List<Drink> m_drinkList;

        int m_interval = 1;
        System.Timers.Timer dispatcherTimer = new System.Timers.Timer();

        public Window1(DrinkServiceAgent DrinkServiceAgent)
        {
            m_service = DrinkServiceAgent;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            InitializeComponent();
            lblStatus.Content = "Waiting for server...";


            LoadDrinks();
            dispatcherTimer.Interval = 1000;
            dispatcherTimer.Elapsed += new ElapsedEventHandler(dispatcherTimer_Elapsed);
            dispatcherTimer.Start();


            Logger.LogEvent("DrinkKassa started");
        }
        void CloseWithLicenseWarning()
        {
            dispatcherTimer.Stop();
            MessageBox.Show("Uw licentietijd is op. Indien u de applicatie wilt proberen kan u een gratis licentie activeren op het hoofdscherm");
            this.Close();
        }

        void dispatcherTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                SetLabelTextInvoker setlabeltextinvoker = new SetLabelTextInvoker(SetLabelText);
                bool LicenseValid = m_service.Exec(d => d.CanConnect());
                if (!LicenseValid)
                {
                    //this.Dispatcher.Invoke(new Action(CloseWithLicenseWarning));
                    this.Dispatcher.Invoke(setlabeltextinvoker, lblStatus, "Geen verbinding met server. Controleer of het splash scherm nog steeds open is.");
                }
                else
                {
                    DrinkConfig config = m_service.Exec(d => d.GetConfig());
                    List<Drink> newPrices = m_service.Exec(d => d.GetPrices());
                    DrinkStatus DrinkStatus = m_service.Exec(d => d.GetStatus());

                    int Interval = config.PriceInterval;
                    m_interval = Interval;

                    m_drinkList = newPrices;
                    this.Dispatcher.Invoke(new Action(RedrawPrices));

                    this.Dispatcher.Invoke(setlabeltextinvoker, lblStatus, "Seconden tot hercalculatie: " + DrinkStatus.SecondsLeft + "| tijd tot crash " + TimeSpan.FromSeconds(DrinkStatus.CrashSecondsLeft));
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message + "|" + ex.StackTrace);
                Console.Error.WriteLine("Error occured");
                Console.Error.Flush();
            }
        }


        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                Logger.LogMessage(ex.Message + "|" + ex.StackTrace);
                Console.Error.WriteLine("Error occured");
                Console.Error.Flush();
            }
            catch
            {

            }
        }

        private void LoadDrinks()
        {
            Logger.LogEvent("Loading drinks");
            m_drinkList = m_service.Exec(d => d.GetDrinkList());
            DrinkConfig conf = m_service.Exec(d => d.GetConfig());
            m_interval = conf.PriceInterval;

            for (int i = 1; i <= 12; i++)
            {
                Logger.LogEvent("Loading drink" + i);
                DrinkButton drinkButton = new DrinkButton();
                drinkButton.Margin = new Thickness(3);
                drinkButton.ManualChanged += new DrinkButton.ManualChangedDelegate(drinkButton_ManualChanged);
                drinkButton.Tag = i;
                drinkButton.MouseDoubleClick += new MouseButtonEventHandler(drinkButton_MouseDoubleClick);
                StackPanel.Children.Add(drinkButton);

                Drink drink = m_drinkList.SingleOrDefault(d => d.HotKey == i);
                if (drink == null)
                {
                    drinkButton.ResetFields(i);
                }
                else
                {
                    SetDrinkButton(drink, drinkButton);
                }

            }
        }

        void drinkButton_ManualChanged(DrinkButton sender, Drink drinkChanged)
        {

            try
            {
                m_service.Exec(d => d.SaveDrink(drinkChanged));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Er heeft zich een fout voorgedaan" + ex.Message);
                Logger.LogMessage(ex.Message);
            }


        }

        void proxy_SecondsLeftChanged(int SecondsLeft, int SecondsToCrash)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.Invoke(new Action<int, int>(proxy_SecondsLeftChanged), SecondsLeft, SecondsToCrash);
                return;
            }

            SetLabelTextInvoker setlabeltextinvoker = new SetLabelTextInvoker(SetLabelText);
            this.Dispatcher.Invoke(setlabeltextinvoker, lblStatus, "Seconden tot hercalculatie: " + SecondsLeft + "| tijd tot crash " + TimeSpan.FromSeconds(SecondsToCrash));

        }

        void SetLabelText(Label label, string Text)
        {
            label.Content = Text;
        }


        void SetDrinkButton(Drink drink, DrinkButton drinkButton)
        {
            if (drink == null)
            {
                drinkButton.ResetFields();
            }
            else
            {
                drinkButton.SetDrinkButtonLabels(drink);
            }
        }

        void drinkButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            EditDrink editDrinkControl = new EditDrink();
            int which = (int)((DrinkButton)sender).Tag;
            Drink drink = m_drinkList.SingleOrDefault(d => d.HotKey == which);
            editDrinkControl.HotKey = "F" + which;
            editDrinkControl.Owner = this;
            if (drink == null)
            {
                editDrinkControl.Title = "Nieuwe drank";
            }
            else
            {
                editDrinkControl.Title = "Editeer drank " + drink.DrinkName;
                editDrinkControl.MaxPrice = drink.MaxPrice;
                editDrinkControl.MinPrice = drink.MinPrice;
                editDrinkControl.NormalPrice = drink.NormalPrice;
                editDrinkControl.DrinkNaam = drink.DrinkName;
            }

            if (editDrinkControl.ShowDialog() == true)
            {
                if (drink == null)
                {
                    drink = new Drink();
                    drink.HotKey = which;
                    drink.CurrentPrice = editDrinkControl.NormalPrice;
                    drink.NextPrice = editDrinkControl.NormalPrice;

                    drink.ID = Guid.NewGuid();
                    m_drinkList.Add(drink);

                }
                drink.MaxPrice = editDrinkControl.MaxPrice;
                drink.MinPrice = editDrinkControl.MinPrice;
                drink.NormalPrice = editDrinkControl.NormalPrice;
                drink.CurrentPrice = editDrinkControl.NormalPrice;
                drink.NextPrice = editDrinkControl.NormalPrice;
                drink.DrinkName = editDrinkControl.DrinkNaam;


                try
                {
                    m_service.Exec(d => d.SaveDrink(drink));

                    SetDrinkButton(drink, ((DrinkButton)sender));

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Er heeft zich een fout voorgedaan" + ex.Message);
                    Logger.LogMessage(ex.Message);
                }


            }
            else if (editDrinkControl.Delete)
            {
                if (drink != null)
                {

                    try
                    {

                        m_service.Exec(d => d.DeleteDrink(drink.ID));
                        m_drinkList.Remove(drink);
                        SetDrinkButton(null, ((DrinkButton)sender));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Er heeft zich een fout voorgedaan" + ex.Message);
                        Logger.LogMessage(ex.Message);
                    }


                }
            }
        }



        void DrinkServiceProxy_NewPriceList(List<Drink> DrinkList)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.Invoke(new Action<List<Drink>>(DrinkServiceProxy_NewPriceList), DrinkList);
                return;
            }

            m_drinkList = DrinkList;
            RedrawPrices();
        }

        void RedrawPrices()
        {
            for (int i = 1; i <= 12; i++)
            {
                Drink drink = m_drinkList.SingleOrDefault(d => d.HotKey == i);
                if (drink != null)
                {
                    DrinkButton drinkButton = (DrinkButton)StackPanel.Children[i - 1];
                    SetDrinkButton(drink, drinkButton);
                }
            }
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            Config configControl = new Config();
            configControl.Owner = this;
            DrinkConfig drinkConfig = m_service.Exec(d => d.GetConfig());
            configControl.Sensibility = drinkConfig.Sensibility;
            configControl.SecondsBetween = drinkConfig.SecondsBetweenRecalculation;
            configControl.CrashSeconds = drinkConfig.CrashSeconds;
            configControl.BeursFuifFontSize = drinkConfig.FontSize;
            configControl.SecondsTillNextCrash = drinkConfig.SecondsTillNextCrash;
            configControl.PriceInterval = drinkConfig.PriceInterval;
            configControl.chkTurn.IsChecked = drinkConfig.TurnLogo;

            if (configControl.ShowDialog() == true)
            {
                drinkConfig.Sensibility = configControl.Sensibility;
                drinkConfig.SecondsBetweenRecalculation = configControl.SecondsBetween;
                drinkConfig.PriceInterval = configControl.PriceInterval;
                drinkConfig.SecondsTillNextCrash = configControl.SecondsTillNextCrash;
                drinkConfig.FontSize = configControl.BeursFuifFontSize;
                drinkConfig.CrashSeconds = configControl.CrashSeconds;
                drinkConfig.TurnLogo = configControl.chkTurn.IsChecked;

                m_service.Exec(d => d.SaveConfig(drinkConfig));
            }

        }

        private void ResetPrices_Click(object sender, RoutedEventArgs e)
        {
            m_service.Exec(d => d.ResetPoints());
        }

        private void SetBeursCrash_Click(object sender, RoutedEventArgs e)
        {
            m_service.Exec(d => d.SetCrash());
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            int KeyID = (int)e.Key - 89;
            //if keyid is 64, F10 was pressed
            if (KeyID == 67)
            {
                KeyID = 10;
            }
            if (KeyID >= 1 && KeyID <= 12)
            {
                if (m_drinkList != null)
                {

                    Drink drinkToOrder = m_drinkList.SingleOrDefault(d => d.HotKey == KeyID);
                    if (drinkToOrder != null)
                    {
                        m_CurrentOrders.Add(drinkToOrder.ID);
                        m_total += drinkToOrder.CurrentPrice;
                        DrinkOrderControl drinkOrderControl = new DrinkOrderControl();
                        drinkOrderControl.DrinkName = drinkToOrder.DrinkName;
                        drinkOrderControl.RemoveMe += new EventHandler(drinkOrderControl_RemoveMe);
                        drinkOrderControl.DrinkID = drinkToOrder.ID;
                        drinkOrderControl.DrankPriceLabel = drinkToOrder.CurrentPrice.ToString() + "€";
                        drinkOrderControl.Price = drinkToOrder.CurrentPrice;
                        drinkOrderControl.Margin = new Thickness(0, 0, 0, 3);

                        lstOrders.Items.Add(drinkOrderControl);
                        BlinkDrink(KeyID);
                        lblTotal.Content = Math.Round(m_total, 2) + "€";
                        float numberOfIntervals = (float)Math.Round((m_total * 100) / m_interval, 2);
                        lblBonnetjes.Content = "Aantal keer " + m_interval + " cent";
                        lblNumberOfIntervals.Content = String.Format("{0}", numberOfIntervals);

                    }
                }
            }

            if (e.Key == Key.Back)
            {
                m_service.Exec(d => d.Recalculate());
            }

            if (e.Key == Key.Enter)
            {
                OrderQueuedList();
            }




        }

        private void OrderQueuedList()
        {
            m_service.Exec(d => d.OrderDrinkList(m_CurrentOrders));
            m_total = 0;
            m_CurrentOrders.Clear();
            lstOrders.Items.Clear();
        }

        void drinkOrderControl_RemoveMe(object sender, EventArgs e)
        {
            lstOrders.Items.Remove(sender);
            DrinkOrderControl drinkOrderControlToRemove = ((DrinkOrderControl)sender);
            m_CurrentOrders.Remove(drinkOrderControlToRemove.DrinkID);
            m_total = m_total - drinkOrderControlToRemove.Price;
            lblTotal.Content = Math.Round(m_total, 2) + "€";

        }
        Decimal m_total = 0;
        public void BlinkDrink(int Fkey)
        {
            DrinkButton label = (DrinkButton)StackPanel.Children[Fkey - 1];
            Thread thread = new Thread(new ParameterizedThreadStart(StartBlinkDrinks));
            thread.Start(label);
        }

        public void StartBlinkDrinks(object label)
        {
            MyMethodInvoker myInvoker2 = new MyMethodInvoker(SetLabelColor);

            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, myInvoker2, label, Colors.OrangeRed);
            Thread.Sleep(150);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, myInvoker2, label, Colors.LightBlue);
        }

        private void SetLabelColor(DrinkButton label, Color color)
        {
            label.Background = new SolidColorBrush(color);
        }

        private void SellingsOverview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LicenseServerProxy proxy = new LicenseServerProxy();
                if (proxy.Exec(d => d.CanConnect()))
                {
                    m_service.Exec(d => d.PublishAllOrders());
                    Guid token = m_service.Exec(d => d.GetConfig()).Token;
                    System.Diagnostics.Process.Start("http://www.beursparty.net/Stats/" + token);
                }
                else
                {
                    MessageBox.Show("Internet is nodig om de statistieken te raadplegen");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Er heeft zich een fout voorgedaan" + ex.Message);
                Logger.LogMessage(ex.Message);
            }
        }
    }
}
