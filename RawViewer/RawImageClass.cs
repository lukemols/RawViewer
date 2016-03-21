using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RawViewer
{
    public class RawImageClass
    {
        int width; //Larghezza delle img
        public int Width { get { return width; } }

        int height; //altezza delle img
        public int Height { get { return height; } }

        int frames; //numero di frame
        public int Frames { get { return frames; } }

        int pixelCount; //numero di pixel totali
        public int PixelCount { get { return pixelCount; } }

        // Percorsi dei file
        string[] filePaths;
        /// <summary>
        ///  Restituisce il percorso del file (Usare se si visualizza una sola immagine)
        /// </summary>
        public string FilePath { get { return filePaths[0]; } }
        /// <summary>
        ///  Restituisce i percorsi dei file RGB(Usare se si visualizzano più immagini)
        /// </summary>
        public string[] FilePaths { get { return filePaths; } }

        bool needParameters;
        /// <summary>
        /// Indica se c'è bisogno di inserire i parametri
        /// </summary>
        public bool NeedParameters { get { return needParameters; } }

        bool singleImage;
        /// <summary>
        /// Indica se è stata caricata una sola img
        /// </summary>
        public bool SingleImage { get { return singleImage; } }
        
        byte[][] streams; //array di stream
        List<byte[]>[] imgs; //array di liste di immagini

        /// <summary>
        /// Costruttore della classe
        /// </summary>
        /// <param name="paths">Array di percorsi</param>
        /// <param name="w">Larghezza delle img (default 0 -> indicare successivamente con SetParams)</param>
        /// <param name="h">Altezza delle img (default 0 -> indicare successivamente con SetParams)</param>
        /// <param name="f">Numero di frame delle img (default 0 -> indicare successivamente con SetParams)</param>
        public RawImageClass(string[] paths, int w = 0, int h = 0, int f = 0)
        {
            width = w; height = h; frames = f; pixelCount = 0;
            filePaths = paths;
            int l = paths.Length;
            imgs = new List<byte[]>[l];
            streams = new byte[l][];

            if (l == 1)
                singleImage = true;
            else
                singleImage = false;
            ///Inizializza gli array
            for (int i = 0; i < imgs.Length; i++)
                imgs[i] = new List<byte[]>();
            //carica gli stream. LoadStream prende in ingresso streams[i] e lo ritorna come uscita tramite out
            for (int i = 0; i < streams.Length; i++)
            {
                LoadStream(filePaths[i], out streams[i]);
            }
            //Controlla se i dati sono corretti
            if (CheckParams())
            {//Se si crea i frame
                CreateFrames();
                needParameters = false;
            } //altrimenti richiedi i parametri
            else
                needParameters = true;
        }

        /// <summary>
        /// Metodo che setta i parametri
        /// </summary>
        /// <param name="w">Larghezza</param>
        /// <param name="h">Altezza</param>
        /// <param name="f">Numero di frame</param>
        public void SetParams(int w, int h, int f)
        {//Controlla che i conti tornino
            width = w; height = h; frames = f;
            if (!CheckParams())
                return;
            //Se si allora si può continuare
            needParameters = false;
            foreach(List<byte[]> l in imgs)
                l.Clear();
            //Cancelliamo le immagini prima di caricare i nuovi frame
            // Questo nel caso in cui si aprano nuove immagini
            // dopo averne aperta una (o più).
            CreateFrames();
        }

        /// <summary>
        /// Metodo che controlla i parametri inseriti
        /// </summary>
        /// <returns></returns>
        public bool CheckParams()
        {//Controlla se coincidono le dimensioni e il numero di frame con il numero di pixel
            if (!(width * height * frames == pixelCount))
                return false;
            foreach(byte[] s in streams)
            {//E se tutte le immagini hanno lo stesso numero di pixel
                if (s.Length != pixelCount)
                    return false;
            }
            //Se tutto va bene torna true, se invece anche solo che una sola cosa è errata torna false
            return true;
        }

        /// <summary>
        /// Metodo che carica l'immagine in uno stream di byte. L'immagine viene considerata dunque a 8 bit.
        /// </summary>
        /// <param name="filePath">Percorso dell'immagine</param>
        /// <param name="fileStream">Stream in cui salvare i byte. NOTARE che è dichiarato come out!</param>
        void LoadStream(string filePath, out byte[] fileStream)
        {
            try
            {
                //Apre un binary reader
                BinaryReader br = new BinaryReader(File.Open(filePath, FileMode.Open));
                byte pixByte;
                int j;
                pixelCount = (int)br.BaseStream.Length; //Ottieni lunghezza stream
                //Crea un array di quella lunghezza
                fileStream = new byte[pixelCount];
                //Leggi e salva un byte alla volta
                for (j = 0; j < pixelCount; ++j)
                {
                    pixByte = (byte)(br.ReadByte());
                    fileStream[j] = pixByte;
                }
                //Chiudi lo stream
                br.Close();
            }
            catch (Exception e)
            {
                //In caso di errore crea uno stream vuoto (dovuto per utilizzare out)
                fileStream = new byte[0];
                Console.WriteLine("ERRORE");
            }
        }

        /// <summary>
        /// Metodo che crea i frame per le immagini
        /// </summary>
        void CreateFrames()
        {
            //Prendi ogni stream (j)
            for (int j = 0; j < streams.Length; j++)
            {//Calcola la grandezza come w*h e crea i frame
                int i = 0;
                int imageSize = width * height;
                for (int f = 0; f < frames; f++)
                {
                    byte[] img = new byte[imageSize];
                    for (int k = 0; k < imageSize; k++, i++)
                    {
                        img[k] = streams[j][i];
                    }
                    imgs[j].Add(img);
                }
            }
        }

        /// <summary>
        /// Metodo che ritorna un array di BitmapSource relative al frame indicato in formato Scala di Grigi
        /// </summary>
        /// <param name="i">Indice del frame</param>
        /// <returns>Array di immagini del frame</returns>
        public BitmapSource[] GetImageFromIndex(int i)
        {
            //Array di bitmapSource
            BitmapSource[] bmpSources = new BitmapSource[imgs.Length];
            for(int j = 0; j < imgs.Length; j++)
            {
                int bitsPerPixel = 8;
                int stride = (width * bitsPerPixel + 7) / 8;

                // Crea una immagine bitmap
                BitmapSource bmps = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null,
                    imgs[j][i], stride);

                bmpSources[j] = bmps;
            }

            return bmpSources;
        }
    }
}
