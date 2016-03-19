using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RawViewer
{
    public class LoadFileClass
    {
        int width;
        public int Width { get { return width; } }

        int height;
        public int Height { get { return height; } }

        int frames;
        public int Frames { get { return frames; } }

        int pixelCount;
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
        public bool NeedParameters { get { return needParameters; } }

        bool singleImage;
        public bool SingleImage { get { return singleImage; } }
        
        byte[][] streams; //array di stream
        List<byte[]>[] imgs; //array di liste di immagini

        public LoadFileClass(string[] paths, int w = 0, int h = 0, int f = 0)
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

            for (int i = 0; i < imgs.Length; i++)
                imgs[i] = new List<byte[]>();

            for (int i = 0; i < streams.Length; i++)
            {
                LoadStream(filePaths[i], out streams[i]);
            }

            if (width * height * frames == pixelCount)
            {
                CreateFrames();
                needParameters = false;
            }
            else
                needParameters = true;
        }

        public void SetParams(int w, int h, int f)
        {
            width = w; height = h; frames = f;
            needParameters = false;
            foreach(List<byte[]> l in imgs)
                l.Clear();
            //Cancelliamo le immagini prima di caricare i nuovi frame
            // Questo nel caso in cui si aprano nuove immagini
            // dopo averne aperta una (o più).
            CreateFrames();
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
            for (int j = 0; j < streams.Length; j++)
            {
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
        /// Metodo che ritorna un array di BitmapSource relative al frame indicato
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
