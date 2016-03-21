using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Ribbon;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RawViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        RawImageClass rawImageObj; //Istanza della classe che gestisce le immagini
        string[] filePaths; //Percorsi dei file
        int imageIndex; //Indice del frame
        int maxImageIndex; //Numero max di frame

        Image[] panels; //Pannelli delle immagini

        /// <summary>
        /// Costruttore di default
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //Lista delle risoluzioni. Questa viene visualizzata nella prima scheda del ribbon
            List<string> resolutions = new List<string>(new string[] { "181x217", "256x256", "512x512", "Seleziona manualmente" });
            ResolutionComboBoxGalleryCategory.ItemsSource = resolutions; //aggiungi le risoluzioni
            filePaths = new string[] { "" };//Setta i percorsi ad un array vuoto
            panels = new Image[] { ImagePanel, ImagePanelR, ImagePanelG, ImagePanelB }; //Setta i pannelli delle immagini
            NavigationButtons.Visibility = Visibility.Collapsed; //Nascondi i pulsanti avanti - indietro...
        }

        /// <summary>
        /// Metodo che mostra l'immagine sui pannelli
        /// </summary>
        /// <param name="index">Indice del frame</param>
        void SetImage(int index = 0)
        {
            //Mostra i pulsanti avanti - indietro..
            NavigationButtons.Visibility = Visibility.Visible;
            maxImageIndex = rawImageObj.Frames - 1;
            if (index < 0 || index > maxImageIndex) //Controlla se si è alla fine o all'inizio dei frame (nel caso non fare nulla)
                return;
            imageIndex = index; //Indice attuale
            BitmapSource[] sources = rawImageObj.GetImageFromIndex(imageIndex); //ottieni le immagini
            if (rawImageObj.SingleImage) //Se c'è una sola img mostra il primo pannello
            {
                panels[0].Visibility = Visibility.Visible;
                panels[0].Source = sources[0];
                for (int i = 1; i < panels.Length; i++)
                {
                    panels[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (rawImageObj.FilePaths.Length == 3) //Se 3 mostrale sui pannelli RGB
            {
                panels[0].Visibility = Visibility.Collapsed;
                for (int i = 1; i < panels.Length; i++)
                {
                    panels[i].Visibility = Visibility.Visible;
                    panels[i].Source = sources[i - 1];
                }
            }
            else if (rawImageObj.FilePaths.Length == 4)//Se 4 mostrale su tutti i pannelli
            {
                for (int i = 0; i < panels.Length; i++)
                {
                    panels[i].Visibility = Visibility.Visible;
                    panels[i].Source = sources[i];
                }
            }
            FrameNumberTextBox.Text = imageIndex + "/" + maxImageIndex; //Mostra numero del frame nella textbox
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
            //Path = @"C:\Users\luca9\Desktop\Uni Files\2M\DIP\ESAME\File Di Test"; //decommenta e cambia cartella in fase di debug per velocizzare
            ofd.InitialDirectory = Path;
            ofd.Filter = "Immagini raw (.raw)|*.raw";
            ofd.Multiselect = true;//Abilita selezione multipla
            //ottieni il risultato
            Nullable<bool> result = ofd.ShowDialog();

            if (result == true)
            {//Controlla che l'utente abbia deciso di aprire 1, 3 o 4 immagini
                int l = ofd.FileNames.Length;
                if (l == 1 || l == 3 || l == 4)
                {//Se si aggiungi ai percorsi dei file i file selezionati
                    filePaths = ofd.FileNames;
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Metodo che chiama ImageParametersChoiceWindow per scegliere i parametri
        /// </summary>
        private void ManualParametersSelection()
        {
            rawImageObj = new RawImageClass(filePaths);
            ImageParametersChoiceWindow imgParams = new ImageParametersChoiceWindow(rawImageObj);
            if ((bool)imgParams.ShowDialog()) //se la finestra di selezione parametri ritorna true allora apri l'immagine
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
            //Se la selezione file va a buon fine apri la finestra per l'inserimento dei parametri
            if (ShowDialogOpenFile())
            {
                ManualParametersSelection();
            }
            else
                MessageBox.Show("Non è stato caricato nessun file. Rimarrà in memoria il vecchio percorso.", "Attenzione");
        }

        /// <summary>
        /// Evento generato dal click sul pulsante apri del gruppo nel tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileFromRibbonButton_Click(object sender, RoutedEventArgs e)
        {
            //Se la selezione file non va a buon fine mostra un messaggio d'errore
            if (!ShowDialogOpenFile())
                MessageBox.Show("Non è stato caricato nessun file. Rimarrà in memoria il vecchio percorso.", "Attenzione");
        }

        /// <summary>
        /// Metodo generato dal click sul pulsante carica immagine del gruppo nel tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            //Controlla che ci siano dei percorsi
            if (filePaths.Length > 0)
            {//Se è selezionato seleziona manualmente apri selezione parametri
                if (ResolutionComboBoxGallery.SelectedItem.ToString() == "Seleziona manualmente")
                {
                    ManualParametersSelection();
                }
                else
                {//altrimenti converti la stringa inserita nella combo box dei parametri
                    string[] wh = ResolutionComboBoxGallery.SelectedItem.ToString().Split('x');
                    if (wh.Length == 2)//(o almeno provaci ;) )
                    {
                        try
                        {
                            int w = Convert.ToInt32(wh[0]);
                            int h = Convert.ToInt32(wh[1]);
                            int f = Convert.ToInt32(SlicesTextBox.Text);
                            rawImageObj = new RawImageClass(filePaths, w, h, f);
                            SetImage(); //Se tutto va bene carica le immagini
                        }
                        catch
                        {//altrimenti mostra un errore
                            MessageBox.Show("Errore durante la conversione dei parametri inseriti. Controlla la loro validità.", "Errore");
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Metodo che chiude l'applicazione
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CLOSE WINDOW");
        }

        /// <summary>
        /// Metodo che gestisce il click sul pulsante indietro
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SetImage(imageIndex - 1);
        }

        /// <summary>
        /// Metodo che gestisce il click sul pulsante avanti
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            SetImage(imageIndex + 1);
        }

        /// <summary>
        /// Metodo che gestisce il cambio del testo nella textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameNumberTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {//Prova a convertire la prima parte della scritta (quella prima di / )
            string[] s = FrameNumberTextBox.Text.Split('/');
            try
            {
                int i = Convert.ToInt32(s[0]);
                if (i != imageIndex)
                    SetImage(i);//se ci riesci cambia immagine
            }
            catch
            { } //Altrimenti non fare nulla
        }

        #endregion

        #region Azioni sulle immagini (Click del mouse e scroll della rotellina

        /// <summary>
        /// Metodo che gestisce il click del mouse (SINISTRO) sull'immagine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImagePanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            /*Siccome l'immagine può variare la sua dimensione all'interno della finestra si procede così:
                ottieni la posizione del mouse all'interno del controllo immagine.
                ottieni le x e y reali facendo il rapporto tra la dimensione dell'immagine e quella del controllo
                (altezza e larghezza) e moltiplicale per la posizione del mouse all'interno del controllo.
                In pratica è una proporzione: p.X : panel.Width = pixel.X : image.Width (lo stesso per y)
                
            **** ATTENZIONE: ****
            Questo metodo è chiamato indifferentemente per ogni pannello. 
            Nel caso in cui si volesse sapere quale pannello ha scatenato l'evento, controllate SENDER.
            */
            var p = e.GetPosition((System.Windows.Controls.Image)sender);
            int x = (int)(rawImageObj.Width / ImagePanel.ActualWidth * p.X);
            int y = (int)(rawImageObj.Height / ImagePanel.ActualHeight * p.Y);
            MessageBox.Show("Hai cliccato nelle coordinate x: " + x + ", y: " + y + " dell'immagine.");
            // Qui sopra c'è un esempio di utilizzo delle coordinate calcolate. Modifica a seconda della necessità.
            
        }

        /// <summary>
        /// Metodo che gestisce il movimento della rotellina del mouse sopra l'immagine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImagePanel_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {//Se va su torna indietro di una img, altrimenti vai avanti
            if (e.Delta > 0) //Rotellina su
                SetImage(imageIndex - 1);
            else
                SetImage(imageIndex + 1);
        }
        #endregion
    }
}
