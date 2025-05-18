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
using System.Windows.Shapes;
using System.Threading;

namespace DrinkKassaClient
{
    /// <summary>
    /// Interaction logic for EditDrink.xaml
    /// </summary>
    public partial class EditDrink : Window
    {
        public bool Delete = false;

        public EditDrink()
        {
            InitializeComponent();            

        }

        bool CheckForNumber(TextBox textbox)
        {
            try
            {
                Decimal test = Decimal.Parse(textbox.Text);
                textbox.Background = Brushes.White;
                return true;
            }
            catch
            {
                textbox.Background = Brushes.Pink;
                return false;
            }
        }

        public string DrinkNaam
        {
            get
            {
                return txtDrinkName.Text.Trim();
            }
            set
            {
                txtDrinkName.Text = value;
            }

        }

        public Decimal NormalPrice
        {
            get
            {
                try
                {
                    return Decimal.Parse(txtNormalPrice.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtNormalPrice.Text = value.ToString();
            }
        }

        public Decimal MinPrice
        {
            get
            {
                try
                {
                    return Decimal.Parse(txtMinPrice.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtMinPrice.Text = value.ToString();
            }
        }

        public Decimal MaxPrice
        {
            get
            {
                try
                {
                    return Decimal.Parse(txtMaxPrice.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtMaxPrice.Text = value.ToString();
            }
        }

        public int Weight
        {
            get
            {
                try
                {
                    return int.Parse(txtWeight.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtWeight.Text = value.ToString();
            }
        }
        public string HotKey
        {
            set
            {
                lblHotkey.Content = value;
            }
        }


        private void Button_Save(object sender, RoutedEventArgs e)
        {
            bool error = false;
            string separator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string from="";
            string to = "";
            if (separator == ".")
            {
                from = ",";
                to = ".";
            }
            else
            {
                from = ".";
                to = ",";
            }

            txtMaxPrice.Text = txtMaxPrice.Text.Replace(from, to);
            txtMinPrice.Text = txtMinPrice.Text.Replace(from, to);
            txtNormalPrice.Text = txtNormalPrice.Text.Replace(from, to);

            if (!CheckForNumber(txtMaxPrice))
            {
                error = true;
            }

            if (!CheckForNumber(txtMinPrice))
            {
                error = true;
            }
            if (!CheckForNumber(txtNormalPrice))
            {
                error = true;
            }
            if (!error)
            {
                txtMaxPrice.Text = Math.Min(decimal.Parse(txtNormalPrice.Text) * 2, decimal.Parse(txtMaxPrice.Text)).ToString();
            }

            if (txtDrinkName.Text.Trim() == "")
            {
                txtDrinkName.Background = Brushes.Pink;
                error = true;
            }

            if (!CheckForNumber(txtWeight))
            {
                error = true;
            }

            if (!error){            
                this.DialogResult = true;
                this.Close();
            }
        }

        private void Button_Annuleren(object sender, RoutedEventArgs e)
        {			
            this.DialogResult = false;
            this.Close();
        }
        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Delete = true;
            this.Close();
        }
        
    }
}

