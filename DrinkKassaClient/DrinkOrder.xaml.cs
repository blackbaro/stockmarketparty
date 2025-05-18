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

namespace DrinkKassaClient
{
    /// <summary>
    /// Interaction logic for DrinkOrder.xaml
    /// </summary>
    public partial class DrinkOrderControl : UserControl
    {
        public event EventHandler RemoveMe;

        public DrinkOrderControl()
        {
            InitializeComponent();
        }
        public Guid DrinkID
        {
            get;
            set;
        }

        public string DrinkName
        {
            set
            {
                lblDrinkName.Content = value;
            }
        }

        public Decimal Price
        {
            get;
            set;
        }

        public string DrankPriceLabel
        {
            get
            {
                return lblPrice.Content.ToString();
            }
            set
            {
                lblPrice.Content = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveMe != null)
            {
                RemoveMe(this, null);
            }
        }
    }
}
