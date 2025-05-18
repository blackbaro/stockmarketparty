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
using System.Timers;
using System.Windows.Media.Animation;
using DrinkServiceContract;
using Utilities;
using System.IO;
using System.Windows.Markup;
using System.Windows.Threading;
using DrinkServiceProxy;


namespace DrinkStatsClient2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public delegate void MethodInvoker();
        Random rnd = new Random();
        int maxEllipses = 100;
        List<Bar> barList = new List<Bar>();

        List<UIElement> m_ellipses = new List<UIElement>();
        //ReflexLogo m_logo = new ReflexLogo();
        UserControl m_logo;
        AnimationClock m_crashClock;
        int m_BarsMargin = 3;
        int m_secondsForTurn = 20;
        int TO = 0;

        AnimationClock m_logoClock;
        bool dissapearing = false;
        double m_lastHeight = 0;
        double m_lastWidth = 0;
        DrinkConfig m_config;
        Timer dispatcherTimer = new Timer();
        DrinkServiceAgent m_service;
        DrinkStatus m_status;



        public Window1(DrinkServiceAgent DrinkServiceAgent,bool reflexlogo)
        {
            try
            {
                m_service = DrinkServiceAgent;
                if (reflexlogo)
                {
                    m_logo=new ReflexLogo();
                }else{
                    m_logo = new VoorbeeldLogo(m_service.Exec(d => d.GetLogo()));
                }
                InitializeComponent();

                m_config = m_service.Exec(d => d.GetConfig());
                setPrices(m_service.Exec(d => d.GetDrinkList()));

                dispatcherTimer.Interval = 1000;
                dispatcherTimer.Elapsed += new ElapsedEventHandler(dispatcherTimer_Elapsed);
                dispatcherTimer.Start();


                Timer timerTO = new Timer(60000);
                timerTO.Enabled = true;
                timerTO.Start();
                timerTO.Elapsed += new ElapsedEventHandler(timerTO_Elapsed);



                Timer timerCreateBubbel = new Timer(400);
                timerCreateBubbel.Elapsed += new ElapsedEventHandler(timerCreateBubbel_Elapsed);
                timerCreateBubbel.Enabled = true;
                timerCreateBubbel.Start();

                DoubleAnimation crashAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
                crashAnimation.RepeatBehavior = RepeatBehavior.Forever;
                crashAnimation.AutoReverse = true;
                m_crashClock = crashAnimation.CreateClock();


                this.SizeChanged += new SizeChangedEventHandler(Window1_SizeChanged);


                ScaleTransform scale = new ScaleTransform(1, 1);

                m_logo.RenderTransform = scale;
                m_logo.Opacity = 1;

                RotateTransform transform = new RotateTransform(0, m_logo.Width / 2, m_logo.Height / 2);
                testCanvas.Children.Add(m_logo);
                m_logo.RenderTransform = transform;


                SetLogoPosition();
                StartReflexAnimation();
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void dispatcherTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DrinkConfig config = m_service.Exec(d => d.GetConfig());
            DrinkStatus status = m_service.Exec(d => d.GetStatus());

            this.Dispatcher.Invoke(new Action<int>(proxy_SecondsLeftChanged), status.SecondsLeft);

            if (config.LastChangeDate != m_config.LastChangeDate)
            {
                this.Dispatcher.Invoke(new Action<DrinkConfig>(proxy_NewConfigEvent), config);
            }

            if (m_status == null || status.LastCalculation != m_status.LastCalculation)
            {
                List<Drink> drinkList = m_service.Exec(d => d.GetPrices());
                this.Dispatcher.Invoke(new Action<List<Drink>>(setPrices), drinkList);
                m_status = status;
            }

            m_status = status;
        }

        void proxy_NewConfigEvent(DrinkConfig config)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.Invoke(new Action<DrinkConfig>(proxy_NewConfigEvent), config);
                return;
            }

            m_config = config;
            foreach (Bar bar in BeerStack.Children)
            {
                bar.SetFontSizes(config.FontSize);
            }

            if (m_config.TurnLogo.GetValueOrDefault())
            {
                rotateAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(m_secondsForTurn));
                rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
                m_logoClock = rotateAnimation.CreateClock();

                m_logo.RenderTransform.ApplyAnimationClock(RotateTransform.AngleProperty, m_logoClock);

            }
            else
            {
                rotateAnimation = new DoubleAnimation(0, 0, TimeSpan.FromSeconds(m_secondsForTurn));
                rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
                m_logoClock = rotateAnimation.CreateClock();

                m_logo.RenderTransform.ApplyAnimationClock(RotateTransform.AngleProperty, m_logoClock);

            }
        }



        void timerTO_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                TO = m_service.Exec(d => d.GetTotalTurnover());
            }
            catch
            {

            }
        }

        void timerCreateBubbel_Elapsed(object sender, ElapsedEventArgs e)
        {
            MethodInvoker invok = new MethodInvoker(CreateBubbel);
            this.Dispatcher.Invoke(invok);
        }


        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                Logger.LogMessage(ex.Message + "|" + ex.StackTrace);
            }
            catch
            {

            }
        }



        DoubleAnimation rotateAnimation;
        void StartReflexAnimation()
        {
            rotateAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(m_secondsForTurn));
            rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
            m_logoClock = rotateAnimation.CreateClock();
            if (m_config.TurnLogo.GetValueOrDefault())
            {
                m_logo.RenderTransform.ApplyAnimationClock(RotateTransform.AngleProperty, m_logoClock);
            }
        }

        private void SetLogoPosition()
        {
            //m_logo.Margin = new Thickness(this.ActualWidth / 2 - image1.Width / 2, 50, 0, 0);
            Canvas.SetLeft(m_logo, this.ActualWidth / 2 - m_logo.Width / 2);
            Canvas.SetTop(m_logo, 20);
        }

        private void SetBarHeights()
        {
            foreach (Bar bar in barList)
            {
                bar.Height = this.ActualHeight/2;
                bar.Margin = new Thickness(0, 0, m_BarsMargin, 0);
                BeerStack.Height = this.ActualHeight/2;
            }
        }

        private void SetBarWidths()
        {

            foreach (Bar bar in barList.ToList())
            {
                double totalBarMargin = ((bigBars * 2) + smallBars) * m_BarsMargin;
                double beerStackMargin = BeerStack.Margin.Left + BeerStack.Margin.Right;
                bar.Width = Math.Max(50, (this.ActualWidth - totalBarMargin - beerStackMargin - 7) / ((bigBars * 2) + smallBars));
                if (bar.BigBar)
                {
                    bar.Width = bar.Width * 2;
                }
                //bar.Width = 80;
                bar.Margin = new Thickness(0, 0, m_BarsMargin, 0);
            }
        }
        private void CreateBubbel()
        {
            //SetLogoPosition();

            foreach (UIElement ellipseToCheck in m_ellipses.ToList())
            {
                double top = Canvas.GetTop(ellipseToCheck);
                if (top < 20)
                {
                    m_ellipses.Remove(ellipseToCheck);
                    testCanvas.Children.Remove(ellipseToCheck);
                }
            }


            UIElement ellipse = CreateBackgroundItem();
            m_ellipses.Add(ellipse);

            testCanvas.Children.Add(ellipse);

            int left = rnd.Next(0, (int)testCanvas.ActualWidth);
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, testCanvas.ActualHeight + 50);


            DoubleAnimation doubleanimationTop = new DoubleAnimation();
            doubleanimationTop.From = testCanvas.ActualHeight + 50;
            doubleanimationTop.To = -50;
            TimeSpan seconds = TimeSpan.FromSeconds(rnd.Next(5, 15));
            //TimeSpan seconds = TimeSpan.FromSeconds(30);
            doubleanimationTop.Duration = seconds;
            doubleanimationTop.AccelerationRatio = 0.9;

            DoubleAnimation doubleanimationOpacity = new DoubleAnimation();
            doubleanimationOpacity.To = 0;
            doubleanimationOpacity.BeginTime = seconds.Subtract(TimeSpan.FromSeconds(4));
            doubleanimationOpacity.Duration = TimeSpan.FromSeconds(4);
            doubleanimationOpacity.AccelerationRatio = 0.9;

            ellipse.BeginAnimation(Canvas.TopProperty, doubleanimationTop);
        }
        private UIElement CreateBackgroundItem()
        {
            UIElement elementToReturn;
            if (rnd.Next(0, 10000) == 15)
            {
                Label label = new Label();
                label.Content = "www.beursparty.net";
                label.FontSize = 15;
                elementToReturn = label;
            }
            else
            {
                elementToReturn = CreateBackgroundBubbel();
            }

            return elementToReturn;
        }

        private Ellipse CreateBackgroundBubbel()
        {

            Ellipse ellipse = new Ellipse();
            //ellipse.Fill =new SolidColorBrush(Color.FromRgb(193,131,0));
            if (TO > 0)
            {
                if (TO > 1000)
                {
                    TO -= 1000;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    TO -= 100;
                    ellipse.Fill = new SolidColorBrush(Colors.Black);
                }
            }
            else
            {
                ellipse.Fill = new SolidColorBrush(Colors.WhiteSmoke);
            }
            ellipse.Opacity = 0.8;
            int diameter = rnd.Next(5, 20);
            ellipse.Width = diameter;
            ellipse.Height = diameter;

            return ellipse;
        }





        int bigBars = 0;
        int smallBars = 0;

        public void setPrices(List<Drink> DrinkPrices)
        {
            if (DrinkPrices.Count > 0)
            {
                bigBars = 0;
                smallBars = 0;
                decimal maxHeight1 = 3;
                decimal maxHeight2 = 3;
                if (DrinkPrices.Where(d => d.NormalPrice < 10).Count() > 0)
                {
                    maxHeight1 = DrinkPrices.Where(d => d.NormalPrice < 5).Max(d => d.MaxPrice);
                }
                if (DrinkPrices.Where(d => d.NormalPrice > 10).Count() > 0)
                {
                    maxHeight2 = DrinkPrices.Where(d => d.NormalPrice > 5).Max(d => d.MaxPrice) / 2;
                }
                Bar.maxPriceHeight = (int)Math.Max(maxHeight1, maxHeight2) + 1;
                foreach (Drink drink in DrinkPrices.OrderBy(d => d.HotKey))
                {

                    Bar bar = barList.SingleOrDefault(d => d.DrinkID == drink.ID);

                    if (bar == null)
                    {
                        bar = new Bar();

                        bar.VerticalAlignment = VerticalAlignment.Bottom;

                        bar.DrinkID = drink.ID;
                        bar.Opacity = 0.9;
                        barList.Add(bar);
                        BeerStack.Children.Add(bar);

                    }
                    if (drink.NormalPrice > 10)
                    {
                        bigBars++;
                        bar.BigBar = true;
                    }
                    else
                    {
                        smallBars++;
                        bar.BigBar = false;
                    }
                    bar.SetFontSizes(m_config.FontSize);
                    bar.SetBar(drink.DrinkName, drink.CurrentPrice);
                    if (drink.CurrentPrice == drink.MinPrice)
                    {
                        bar.SetCrashVisibility(true);
                    }
                    else
                    {
                        bar.SetCrashVisibility(false);
                    }
                }
            }
            bool barRemoved = false;

            foreach (Bar bar in barList.ToList())
            {

                if (!DrinkPrices.Select(d => d.ID).Contains(bar.DrinkID))
                {
                    BeerStack.Children.Remove(bar);
                    barList.Remove(bar);
                    barRemoved = true;

                }
            }

            SetBarWidths();
            SetBarHeights();

        }


        void proxy_SecondsLeftChanged(int SecondsLeft)
        {

            try
            {
                if (m_lastHeight != this.ActualHeight || m_lastWidth != this.ActualWidth)
                {
                    m_lastWidth = this.Width;
                    m_lastHeight = this.Height;

                    SetBarWidths();
                }
                if (SecondsLeft <= 20 && SecondsLeft > 0)
                {
                    Number.Content = SecondsLeft;
                    if (!dissapearing)
                    {
                        dissapearing = true;
                        DoubleAnimation dissapear = new DoubleAnimation(0, TimeSpan.FromSeconds(3));
                        AnimationClock dissapearClock = dissapear.CreateClock();
                        BeerStack.ApplyAnimationClock(StackPanel.OpacityProperty, dissapearClock);
                        m_logo.ApplyAnimationClock(UserControl.OpacityProperty, dissapearClock);
                        DoubleAnimation appear = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(3));
                        Number.BeginAnimation(Label.OpacityProperty, appear);

                    }


                }
                else
                {
                    if (dissapearing)
                    {
                        dissapearing = false;
                        m_logoClock.Controller.Resume();

                        DoubleAnimation appear = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
                        AnimationClock appearClock = appear.CreateClock();
                        BeerStack.ApplyAnimationClock(StackPanel.OpacityProperty, appearClock);
                        m_logo.ApplyAnimationClock(UserControl.OpacityProperty, appearClock);
                        BeerStack.BeginAnimation(StackPanel.OpacityProperty, appear);

                        DoubleAnimation dissapear = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                        Number.BeginAnimation(Label.OpacityProperty, dissapear);


                    }
                }
            }
            catch
            {

            }
        }

        void Window1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                SetBarWidths();
            }
            if (e.HeightChanged)
            {
                SetBarHeights();
            }
            SetLogoPosition();
        }
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }
        }

        private void DynamicLoadStyles(string fileName)
        {

            fileName = Environment.CurrentDirectory + @"\" + fileName;

            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    // Read in ResourceDictionary File
                    ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                    // Clear any previous dictionaries loaded

                    Resources.MergedDictionaries.Clear();
                    // Add in newly loaded Resource Dictionary
                    Resources.MergedDictionaries.Add(dic);
                }
            }
        }

        private void Context_Click(object sender, RoutedEventArgs e)
        {
            DynamicLoadStyles(((MenuItem)sender).Tag.ToString());
        }



    }
}
