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
using Utilities;

namespace DrinkServiceMain
{
    /// <summary>
    /// Interaction logic for ServerControl.xaml
    /// </summary>
    public partial class ServerControl : UserControl
    {
        public ServerControl()
        {
            InitializeComponent();
			SetMinutes();

        }
		void SetMinutes()
		{
			int minutesLeft = License.getMinutes();
			lblTimeLeft.Content = TimeSpan.FromMinutes(minutesLeft).ToString();
		}
        private void btnActivate(object sender, RoutedEventArgs e)
        {
            LicenseServerProxy licenserviceProxy = new LicenseServerProxy();
            try
            {
                Guid guid=new Guid(txtActivationCode.Text);
                int hours=licenserviceProxy.getHours(guid);
				License.AddMinutes(hours * 60);
				SetMinutes();
                txtActivationCode.Background = Brushes.White;
                lblActivationCodeError.Visibility = Visibility.Hidden;
            }
            catch (FormatException ex)
            {
                txtActivationCode.Background = Brushes.Red;
                lblActivationCodeError.Visibility = Visibility.Visible;
                lblActivationCodeError.Text = "De activatiecode is niet correct";
            }
            catch(Exception ex)
            {
                lblActivationCodeError.Visibility = Visibility.Visible;
                lblActivationCodeError.Text= "Er heeft zich een fout voorgedaan " + ex.Message;
            }

            
            
        }
    }
}
