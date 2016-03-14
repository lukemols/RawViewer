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

        string filePath;
        public string FilePath { get { return filePath; } }

        bool needParameters;
        public bool NeedParameters { get { return needParameters; } }

        byte[] stream;
        List<byte[]> images;

        public LoadFileClass(string path)
        {
            width = 0; height = 0; frames = 0; pixelCount = 0;
            needParameters = true;
            filePath = path;
            images = new List<byte[]>();
            LoadStream();
        }

        public LoadFileClass(string path, int w, int h, int f)
        {
            width = w; height = h; frames = f; pixelCount = 0;
            needParameters = false;
            filePath = path;
            images = new List<byte[]>();
            LoadStream();
            if (width * height * frames == pixelCount)
                CreateFrames();
        }

        public void SetParams(int w, int h, int f)
        {
            width = w; height = h; frames = f;
            needParameters = false;
            images.Clear();
            CreateFrames();
        }

        void LoadStream()
        {
            try
            {
                BinaryReader br = new BinaryReader(File.Open(filePath, FileMode.Open));
                byte pixByte;
                int j;
                pixelCount = (int)br.BaseStream.Length;

                stream = new byte[pixelCount];

                for (j = 0; j < pixelCount; ++j)
                {
                    pixByte = (byte)(br.ReadByte());
                    stream[j] = pixByte;
                }

                br.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERRORE");
            }
        }

        void CreateFrames()
        {
            int i = 0;
            int imageSize = width * height;
            for (int f = 0; f < frames; f++)
            {
                byte[] img = new byte[imageSize];
                for (int k = 0; k < imageSize; k++, i++)
                {
                    img[k] = stream[i];
                }
                images.Add(img);
            }
        }

        public BitmapSource GetImageFromIndex(int i)
        {
            int bitsPerPixel = 8;
            int stride = (width * bitsPerPixel + 7) / 8;

            // Crea una immagine bitmap
            BitmapSource bmps = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null,
                images[i], stride);

            return bmps;
        }
    }
}
