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

namespace DrinkKassaClient
{
    /// <summary>
    /// Interaction logic for DrinkButton.xaml
    /// </summary>
    public partial class DrinkButton : UserControl
    {
        public delegate void ManualChangedDelegate(DrinkButton sender, Drink drinkChanged);
        public event ManualChangedDelegate ManualChanged;

        public DateTime? LastChangeDate { get; set; }

        public DrinkButton()
        {
            InitializeComponent();

            SliderManual.IsEnabled = false;
            chkAutoPrice.IsEnabled = false;
            SliderManual.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderManual_ValueChanged);
            SliderManual.LostMouseCapture += new MouseEventHandler(SliderManual_LostMouseCapture);
            chkAutoPrice.Click += new RoutedEventHandler(chkAutoPrice_Clicked);

        }

        void SliderManual_LostMouseCapture(object sender, MouseEventArgs e)
        {

            SendDrinkChangedEvent();
        }

        private void SendDrinkChangedEvent()
        {
            if (m_drink.ID != Guid.Empty)
            {
                if (ManualChanged != null)
                {
                    ManualChanged(this, m_drink);
                }
            }
        }
        Drink m_drink;
       
        public void SetDrinkButtonLabels(Drink drink)
        {
            if (LastChangeDate!=null && LastChangeDate == drink.LastEditDate) return;
            LastChangeDate = drink.LastEditDate;

            
            m_drink = drink;
            SliderManual.IsEnabled = drink.ID != Guid.Empty;
            chkAutoPrice.IsEnabled = drink.ID != Guid.Empty;
            chkAutoPrice.IsChecked = drink.NextManualPrice==null;
            //drink.NextManualPrice = null;

            SliderManual.Value = (double)(drink.NextManualPrice.HasValue ? drink.NextManualPrice.Value : drink.NextPrice);            
            SliderManual.Minimum = (double)drink.MinPrice;
            SliderManual.Maximum = (double)drink.MaxPrice;

            lblDrinkName.Content = "F" + drink.HotKey + " " + drink.DrinkName;
            lblCurrentPrice.Content = drink.CurrentPrice;

            if (drink.NextManualPrice != null)
            {
                lblNextPrice.Content = drink.NextManualPrice.Value;
            }
            else
            {
                lblNextPrice.Content = drink.NextPrice;
            }

        }

        void SliderManual_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string s = "";
        }
        int m_hotkey = 0;
        public void ResetFields()
        {
            ResetFields(m_hotkey);
            SliderManual.IsEnabled = false;

        }
        public void ResetFields(int hotkey)
        {
            m_hotkey = hotkey;

            lblDrinkName.Content = "F" + hotkey;
            lblCurrentPrice.Content = "N/A";
            lblNextPrice.Content = "N/A";
        }

        private void SliderManual_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_drink != null)
            {
                if (chkAutoPrice.IsChecked.HasValue && !chkAutoPrice.IsChecked.Value)
                {
                    m_drink.NextManualPrice = (Decimal)Math.Round(e.NewValue, 2);                    
                }
                

            }
        }

        private void chkAutoPrice_Clicked(object sender, RoutedEventArgs e)
        {
            if (chkAutoPrice.IsChecked.GetValueOrDefault())
            {
                m_drink.NextManualPrice = null;
                lblNextPrice.Content = m_drink.NextPrice;
                SliderManual.Value = (double)m_drink.NextPrice;
            }
            else
            {
                m_drink.NextManualPrice = (Decimal)SliderManual.Value;
            }
            SendDrinkChangedEvent();
        }


    }
}
