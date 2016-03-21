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
        RawImageClass rawImageObj;

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="ric">Riferimento all'oggetto RawImageClass</param>
        public ImageParametersChoiceWindow(RawImageClass ric)
        {
            InitializeComponent();
            rawImageObj = ric;
        }

        /// <summary>
        /// Metodo che controlla il click su ok
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Ottieni le dimensioni inserite (nel try perché convertiamo)
                int w = Convert.ToInt32(tbWidth.Text);
                int h = Convert.ToInt32(tbHeight.Text);
                int f = Convert.ToInt32(tbFrames.Text);
                //Se le dimensioni sono sbagliate segnala errore
                if (w * h * f != rawImageObj.PixelCount)
                {
                    MessageBox.Show("Le dimensioni inserite non coincidono. Riprova per favore.",
                        "Errore dimensioni immagine.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {//altrimenti setta i parametri e chiudi la dialog con risultato true
                    rawImageObj.SetParams(w, h, f);
                    this.DialogResult = true;
                }
            }
            catch { }
        }
    }
}
