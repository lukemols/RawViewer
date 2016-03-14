using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RawViewer
{
    /// <summary>
    /// Interaction logic for ImageParametersChoiceWindow.xaml
    /// </summary>
    public partial class ImageParametersChoiceWindow : Window
    {
        LoadFileClass loadFileObj;

        public ImageParametersChoiceWindow(LoadFileClass lfc)
        {
            InitializeComponent();
            loadFileObj = lfc;
        }

        private void bnOK_Click(object sender, RoutedEventArgs e)
        {
            int w = Convert.ToInt32(tbWidth.Text);
            int h = Convert.ToInt32(tbHeight.Text);
            int f = Convert.ToInt32(tbFrames.Text);

            if (w * h * f != loadFileObj.PixelCount)
            {
                MessageBox.Show("Le dimensioni inserite non coincidono. Riprova per favore.",
                    "Errore dimensioni immagine.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                loadFileObj.SetParams(w, h, f);
                this.DialogResult = true;
            }
        }
    }
}
