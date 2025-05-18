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

namespace DrinkKassaClient
{
    /// <summary>
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class Config : Window
    {
        public Config()
        {
            InitializeComponent();
        }

        public int SecondsBetween {
            get
            {
                try
                {
                    return int.Parse(txtSeconds.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtSeconds.Text = value.ToString();
            }
        }

        public int PriceInterval
        {
            get
            {
                try
                {
                    return int.Parse(txtPriceInterval.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtPriceInterval.Text = value.ToString();
            }
        }


        public int CrashSeconds
        {
            get
            {
                try
                {
                    return int.Parse(txtCrashSeconds.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtCrashSeconds.Text = value.ToString();
            }
        }

        public int BeursFuifFontSize
        {
            get
            {
                try
                {
                    return int.Parse(txtFontSize.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtFontSize.Text = value.ToString();
            }
        }

        public int SecondsTillNextCrash
        {
            get
            {
                try
                {
                    return int.Parse(txtSecondsTillNextCrash.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtSecondsTillNextCrash.Text = value.ToString();
            }
        }

        public float Sensibility
        {
            get
            {
                try
                {
                    return float.Parse(txtSensibility.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtSensibility.Text = value.ToString();
            }
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            bool error=false;
            if (!CheckForNumber(txtSensibility))
            {
                error = true;
            }

            if (!CheckForNumber(txtSeconds))
            {
                error = true;
            }
            if (!CheckForNumber(txtCrashSeconds))
            {
                error = true;
            }

            if (!CheckForNumber(txtFontSize))
            {
                error = true;
            }

            if (!error)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        bool CheckForNumber(TextBox textbox)
        {
            try
            {
                float test = float.Parse(textbox.Text);
                textbox.Background = Brushes.Pink;
                return true;
            }
            catch
            {
                textbox.Background = Brushes.White;
                return false;
            }
        }

        private void Button_Annuleren(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
