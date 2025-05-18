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
using System.Windows.Media.Animation;
using System.Timers;

namespace DrinkStatsClient2
{
    /// <summary>
    /// Interaction logic for Bar.xaml
    /// </summary>
    public partial class Bar : UserControl
    {
        public static int maxPriceHeight = 7;

        double m_priceFrom = 0;
        double m_PriceTo = 0;

        public Guid DrinkID { get; set; }
        public bool BigBar { get; set; }
        Timer timer = new Timer();        
        DateTime m_startMovement = new DateTime();

        int m_moveSeconds = 2;

        public Bar()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(Bar_SizeChanged);
            timer.Enabled = true;
            timer.Interval = 100;
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        void Bar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                lblrectAngle.Width = Math.Max(10, this.ActualWidth);
                lblPrice.Width = Math.Max(10, this.ActualWidth);
                lblDrinkName.Width = Math.Max(10, this.ActualWidth);
                lblCrash.Width = Math.Max(10, this.ActualWidth);
                //lblDrinkName.Height = 100;
                //Canvas.SetLeft(lblrectAngle, (this.ActualWidth - lblrectAngle.Width) / 2);
                //Canvas.SetLeft(lblPrice, (this.ActualWidth - lblPrice.Width) / 2);
                //Canvas.SetLeft(lblDrinkName, (this.ActualWidth - lblDrinkName.Width) / 2);
                
            }
            if (e.HeightChanged)
            {
                ChangeBarSize();
            }
        }


        public void SetBar(string Name, Decimal Price)
        {
            m_priceFrom = m_PriceTo;
            m_PriceTo = (double)Price;
            m_startMovement = DateTime.Now;

            lblDrinkName.Content = Name;
            ChangeBarSize();
        }

        public void SetCrashVisibility(bool VisibleState)
        {
            lblCrash.Visibility = VisibleState ? Visibility.Visible : Visibility.Hidden;
        }

        private void ChangeBarSize()
        {
            
            DoubleAnimation doubleanimationRectangle = new DoubleAnimation();
            int toppadding = 30;
            double bottomPadding = 80;
            double maxPossibleHeight = this.Height - toppadding - bottomPadding;
            double priceTo = BigBar ? m_PriceTo / 2 : m_PriceTo;

            doubleanimationRectangle.To = maxPossibleHeight / maxPriceHeight * priceTo + bottomPadding;            
            doubleanimationRectangle.Duration = TimeSpan.FromSeconds(2);
            doubleanimationRectangle.AccelerationRatio = 0.5;
            doubleanimationRectangle.DecelerationRatio = 0.5;

            

            DoubleAnimation doubleanimationGroupCanvas = new DoubleAnimation();
            doubleanimationGroupCanvas.To = doubleanimationRectangle.To - 82;
            doubleanimationGroupCanvas.Duration = TimeSpan.FromSeconds(2);
            doubleanimationGroupCanvas.AccelerationRatio = 0.5;
            doubleanimationGroupCanvas.DecelerationRatio = 0.5;          


            lblrectAngle.BeginAnimation(Rectangle.HeightProperty, doubleanimationRectangle);
            groupCanvas.BeginAnimation(Canvas.BottomProperty, doubleanimationGroupCanvas);            
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ShowPriceDelegate deleg=new ShowPriceDelegate(ShowPrice);
            Dispatcher.Invoke(deleg);
            
        }

        delegate void ShowPriceDelegate();
        void ShowPrice()
        {
            double totalMilliseconds = m_moveSeconds * 1000;
            double milliSecondsPassed = Math.Min(DateTime.Now.Subtract(m_startMovement).TotalMilliseconds, totalMilliseconds);


            double priceToShow = (m_PriceTo - m_priceFrom) / totalMilliseconds * milliSecondsPassed + m_priceFrom;

            lblPrice.Content = Math.Round(priceToShow, 2) + "€";
        }

        internal void SetFontSizes(int fontSize)
        {
            int fontDelta = fontSize - 18;

            lblDrinkName.FontSize = fontSize;
            lblDrinkName.Height = 34 + fontDelta;
            Canvas.SetBottom(lblDrinkName, 50 - fontDelta);

            lblPrice.FontSize = fontSize;
            lblPrice.Height = 34 + fontDelta;
            Canvas.SetBottom(lblPrice, 20 - (fontDelta*2));

            lblCrash.FontSize = fontSize;
            lblCrash.Height = 34 + fontDelta;
            
        }
    }
}
