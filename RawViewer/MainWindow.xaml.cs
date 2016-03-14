using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Ribbon;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RawViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        LoadFileClass loadFileObj;
        string filePath;
        int imageIndex;
        int maxImageIndex;
        
        /// <summary>
        /// Costruttore di default
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            List<string> resolutions = new List<string>(new string[] { "181x217", "256x256", "512x512", "Seleziona manualmente" });
            ResolutionComboBoxGalleryCategory.ItemsSource = resolutions;
            filePath = "";
        }

        void SetImage()
        {
            imageIndex = 0;
            maxImageIndex = loadFileObj.Frames - 1;
            ImagePanel.Source = loadFileObj.GetImageFromIndex(imageIndex);
            FrameNumberTextBox.Text = imageIndex + "/" + maxImageIndex; 
        }

        void SetImage(int index)
        {
            maxImageIndex = loadFileObj.Frames - 1;
            if (index < 0 || index > maxImageIndex)
                return;
            imageIndex = index;
            ImagePanel.Source = loadFileObj.GetImageFromIndex(imageIndex);
            FrameNumberTextBox.Text = imageIndex + "/" + maxImageIndex;
        }

        /// <summary>
        /// Metodo che mostra all'utente l'open file dialog
        /// </summary>
        /// <returns>True se ha scelto un file</returns>
        bool ShowDialogOpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Seleziona una immagine .raw";
            //Ottieni la cartella Desktop
            var pathWithEnv = @"%USERPROFILE%\Desktop";
            var Path = Environment.ExpandEnvironmentVariables(pathWithEnv);
            Path = @"C:\Users\luca9\Desktop\Uni Files\2M\DIP\ESAME\File Di Test";
            ofd.InitialDirectory = Path;
            ofd.Filter = "Immagini raw (.raw)|*.raw";

            Nullable<bool> result = ofd.ShowDialog();

            if (result == true)
            {
                filePath = ofd.FileName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Metodo che chiama ImageParametersChoiceWindow per scegliere i parametri
        /// </summary>
        private void ManualParametersSelection()
        {
            loadFileObj = new LoadFileClass(filePath);
            ImageParametersChoiceWindow imgParams = new ImageParametersChoiceWindow(loadFileObj);
            if ((bool)imgParams.ShowDialog())
                SetImage();
        }

        #region Click sui pulsanti

        /// <summary>
        /// Evento generato dal click sui pulsanti apri file della quick access toolbar e del menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if(ShowDialogOpenFile())
            {
                ManualParametersSelection();
            }
        }

        /// <summary>
        /// Evento generato dal click sul pulsante apri del gruppo nel tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileFromRibbonButton_Click(object sender, RoutedEventArgs e)
        {
            if(!ShowDialogOpenFile())
            {
                MessageBox.Show("Non è stato caricato nessun file. Rimarrà in memoria il vecchio percorso.", "Attenzione");
            }
        }
        
        /// <summary>
        /// Metodo generato dal click sul pulsante carica immagine del gruppo nel tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            if(filePath != null)
            {
                if(ResolutionComboBoxGallery.SelectedItem.ToString() == "Seleziona manualmente")
                {
                    ManualParametersSelection();
                }
                else
                {
                    string[] wh = ResolutionComboBoxGallery.SelectedItem.ToString().Split('x');
                    if(wh.Length == 2)
                    {
                        try
                        {
                            int w = Convert.ToInt32(wh[0]);
                            int h = Convert.ToInt32(wh[1]);
                            int f = Convert.ToInt32(SlicesTextBox.Text);
                            loadFileObj = new LoadFileClass(filePath, w, h, f);
                            SetImage();
                        }
                        catch
                        {
                            MessageBox.Show("Errore durante la conversione dei parametri inseriti. Controlla la loro validità.", "Errore");
                        }
                    }
                    
                }
            }
        }
        

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CLOSE WINDOW");
        }

        private void ResolutionComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string s = ResolutionComboBoxGallery.SelectedItem.ToString();
            MessageBox.Show(s);
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SetImage(imageIndex - 1);
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            SetImage(imageIndex + 1);
        }
        #endregion

        private void FrameNumberTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string[] s = FrameNumberTextBox.Text.Split('/');
            try
            {
                int i = Convert.ToInt32(s[0]);
                if(i != imageIndex)
                    SetImage(i);
            }
            catch
            { }
        }
    }
}
