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
using System.IO;

namespace DrinkStatsClient2
{
	/// <summary>
	/// Interaction logic for VoorbeeldLogo.xaml
	/// </summary>
	public partial class VoorbeeldLogo : UserControl
	{
		public VoorbeeldLogo(byte[] Logo)
		{
			InitializeComponent();
         
                string logoDir = Environment.CurrentDirectory + @"/logo.png";
                BitmapImage bmpImage = null;
                if (Logo != null)
                {
                    bmpImage = ImageFromBuffer(Logo);
                }
                else if (File.Exists(logoDir))
                {
                    bmpImage = new BitmapImage(new Uri(logoDir));
                }
                if(bmpImage!=null){
                    image1.Source = bmpImage;
                    ExampleLogo.Visibility = Visibility.Hidden;
                    this.Loaded += new RoutedEventHandler(VoorbeeldLogo_Loaded);
                }               
             
            
            
            
            
		}
        public BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        void VoorbeeldLogo_Loaded(object sender, RoutedEventArgs e)
        {
            image1.Margin = new Thickness(0, (this.ActualHeight - image1.ActualHeight)/2, 0, 0);
        }
	}
}
