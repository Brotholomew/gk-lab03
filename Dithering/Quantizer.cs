using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace lab03
{
    public class Quantizer
    {
        private Semaphore MapGeneratedSemaphore;
        private Semaphore KMeansPaletteSemaphore;
        private Semaphore PopularityPaletteSemaphore;

        private ColorMap ColorMap;

        public DirectBitmap Image;
        public DirectBitmap UncertaintyImage;
        public DirectBitmap PopularityImage;
        public DirectBitmap KMeansImage;

        private Thread PropagationOfUncetraintyThread;
        private Thread PopularityMethodThread;
        private Thread KMeansThread;
        private Thread PopularityPaletteThread;
        private Thread KMeansPaletteThread;

        private List<Color> PopularityPaletteColors;
        private List<Color> KMeansPaletteColors;

        public int K { get => Projector.Instance.K; }

        #region Constructors

        public Quantizer()
        {
            this.Init();
        }

        private void Init()
        {
            this.MapGeneratedSemaphore = new Semaphore(1, 1);
            this.KMeansPaletteSemaphore = new Semaphore(1, 1);
            this.PopularityPaletteSemaphore = new Semaphore(1, 1);

            this.ColorMap = new ColorMap();
        }

        #endregion

        public void LoadImage(DirectBitmap img)
        {
            this.AbortThreads();

            this.Image = img;
            this.ColorMap.LoadImage(this.Image);

            // this.GeneratePopularityPalette();
        }

        #region Cleanup Functions

        private void EraseProgress()
        {
            Projector.Instance.UncertaintyMax = 1;
            Projector.Instance.PopularityMax = 1;
            Projector.Instance.KMeansMax = 1;
            Projector.Instance.PopularityPaletteMax = 1;
            Projector.Instance.KMeansPaletteMax = 1;

            Projector.Instance.UncertaintyProgress = 0;
            Projector.Instance.PopularityProgress = 0;
            Projector.Instance.KMeansProgress = 0;
            Projector.Instance.KMeansPaletteProgress = 0;
            Projector.Instance.PopularityPaletteProgress = 0;

            Projector.Instance.UncertaintyTB.Text = "0%";
            Projector.Instance.PopularityTB.Text = "0%";
            Projector.Instance.KMeansTB.Text = "0%";
            Projector.Instance.KMeansPaletteTB.Text = "0%";
            Projector.Instance.PopularityPaletteTB.Text = "0%";
        }

        private void Erase()
        {
            Projector.Instance.KMeansPanel.Controls.Clear();
            Projector.Instance.PopularityPanel.Controls.Clear();

            Projector.Instance.KMeansGenerated = false;
            Projector.Instance.PopularityGenerated = false;
            Projector.Instance.UncertaintyGenerated = false;

            Projector.Instance.PopularityPictureBox.Refresh();
            Projector.Instance.UncertaintyPictureBox.Refresh();
            Projector.Instance.KMeansPictureBox.Refresh();
        }

        #endregion

        #region Threading Control

        private void GeneratePopularityPalette()
        {
            this.PopularityPaletteThread = new Thread(this.PopularityPalette);
            this.PopularityPaletteThread.Start();
        }

        private void GenerateKMeansPalette()
        {
            this.KMeansPaletteThread = new Thread(this.KMeansPalette);
            this.KMeansPaletteThread.Start();
        }

        private void ColorLoad(List<Color> palette, FlowLayoutPanel panel)
        {
            panel.BeginInvoke(new Action(() =>
            {
                foreach (var col in palette)
                {
                    Button btn = new Button();
                    btn.Enabled = false;
                    btn.BackColor = col;
                    btn.Height = btn.Width = panel.Height - 10;

                    if (!panel.Contains(btn)) panel.Controls.Add(btn);
                }
            }));
        }

        public void GenerateDithered()
        {
            this.KMeansPaletteColors = null;
            this.PopularityPaletteColors = null;

            this.Erase();
            this.EraseProgress();
            this.ThreadInit();

            this.GeneratePopularityPalette();
            this.GenerateKMeansPalette();

            List<Thread> threads = new List<Thread> { this.KMeansThread, this.PopularityMethodThread, this.PropagationOfUncetraintyThread };

            foreach (var thread in threads)
                thread.Start();
        }

        public void GenerateUncertainty()
        {
            Projector.Instance.UncertaintyGenerated = false;
            Projector.Instance.UncertaintyPictureBox.Refresh();
            Projector.Instance.OverrideReset();

            this.PropagationOfUncetraintyThread = new Thread(this.PropagationOfUncertainty);
            this.PropagationOfUncetraintyThread.Start();
        }

        private void ThreadInit()
        {
            this.KMeansThread = new Thread(this.KMeans);
            this.PropagationOfUncetraintyThread = new Thread(this.PropagationOfUncertainty);
            this.PopularityMethodThread = new Thread(this.PopularityMethod);
        }

        public void AbortThreads()
        {
            List<Thread> threads = new List<Thread> { this.KMeansThread, this.PopularityMethodThread, this.PropagationOfUncetraintyThread, this.KMeansPaletteThread, this.PopularityPaletteThread };

            Projector.Instance.ThreadStop = true;

            foreach (var thread in threads)
                if (thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Suspended))
                    thread.Join();

            this.Erase();
            this.EraseProgress();

            Projector.Instance.ThreadStop = false;
        }

        #endregion

        #region Palettes Generators

        private void PopularityPalette()
        {
            this.PopularityPaletteSemaphore.WaitOne();

            if (Projector.Instance.ThreadStop) return;

            this.ColorMap.Clear();
            this.ColorMap.GenerateMap();
            this.PopularityPaletteColors = this.ColorMap.MostUsedPalette.GetRange(0, Math.Max(0, Math.Min(this.ColorMap.MostUsedPalette.Count - 1, this.K)));

            this.PopularityPaletteSemaphore.Release();

            this.ColorLoad(this.PopularityPaletteColors, Projector.Instance.PopularityPanel);
            Projector.Instance.PopularityPaletteProgress = Projector.Instance.PopularityPaletteMax;
        }

        private void KMeansPalette()
        {
            this.KMeansPaletteSemaphore.WaitOne();

            while (this.PopularityPaletteColors == null || this.PopularityPaletteColors.Count == 0)
            {
                this.PopularityPaletteSemaphore.WaitOne();
                this.PopularityPaletteSemaphore.Release();
            }

            if (Projector.Instance.ThreadStop) return;

            Projector.Instance.KMeansPaletteMax = this.Image.Bits.Length;

            ClusterCollection CC = new ClusterCollection(this.ColorMap.MostUsedPalette, this.K);
            CC.UpdateClusters();
            this.KMeansPaletteColors = CC.GetColors();

            this.KMeansPaletteSemaphore.Release();

            this.ColorLoad(this.KMeansPaletteColors, Projector.Instance.KMeansPanel);
            Projector.Instance.KMeansPaletteProgress = Projector.Instance.KMeansPaletteMax;
        }

        #endregion

        #region Dithering functions

        private void KMeans()
        {
            Projector.Instance.KMeansGenerated = false;
            Projector.Instance.KMeansSemaphore.WaitOne();

            while (this.KMeansPaletteColors == null || this.KMeansPaletteColors.Count == 0)
            {
                this.KMeansPaletteSemaphore.WaitOne();
                this.KMeansPaletteSemaphore.Release();
            }

            this.KMeansImage = new DirectBitmap(this.Image.Bits, this.Image.Width, this.Image.Height);

            Projector.Instance.KMeansMax = this.KMeansImage.Bits.Length;

            DirectBitmap image = this.KMeansImage;
            List<Color> palette = this.KMeansPaletteColors;

            this.UpdateImage(palette, this.Image, image, Functors.Loop2D.Parallel(image.Width, image.Height), (count) => Projector.Instance.KMeansProgress += count);
            Projector.Instance.KMeansProgress += Projector.Instance.KMeansMax;
            
            Projector.Instance.KMeansGenerated = true;
            Projector.Instance.KMeansSemaphore.Release();
            Projector.Instance.KMeansPictureBox.BeginInvoke(new Action(() => Projector.Instance.RefreshPBs()));
        }

        private void PopularityMethod()
        {
            Projector.Instance.PopularityGenerated = false;
            Projector.Instance.PopularitySemaphore.WaitOne();

            this.PopularityPaletteSemaphore.WaitOne();
            this.PopularityPaletteSemaphore.Release();

            this.PopularityImage = new DirectBitmap(this.Image.Bits, this.Image.Width, this.Image.Height);

            Projector.Instance.PopularityMax = this.PopularityImage.Bits.Length;
            
            DirectBitmap image = this.PopularityImage;
            List<Color> palette = this.PopularityPaletteColors;

            this.UpdateImage(palette, this.Image, image, Functors.Loop2D.Parallel(image.Width, image.Height), (int count) => Projector.Instance.PopularityProgress += count);
            Projector.Instance.PopularityProgress += Projector.Instance.PopularityMax;

            Projector.Instance.PopularityGenerated = true;
            Projector.Instance.PopularitySemaphore.Release();
            Projector.Instance.PopularityPictureBox.BeginInvoke(new Action(() => Projector.Instance.RefreshPBs()));
        }

        private void PropagationOfUncertainty()
        {
            Projector.Instance.UncertaintyGenerated = false;
            Projector.Instance.UncertaintySemaphore.WaitOne();

            if (Projector.Instance.PaletteChoice == Projector.PaletteChoices.KMeansPalette)
            {
                this.KMeansPaletteSemaphore.WaitOne();
                this.KMeansPaletteSemaphore.Release();
            }
            else
            {
                this.PopularityPaletteSemaphore.WaitOne();
                this.PopularityPaletteSemaphore.Release();
            }

            this.UncertaintyImage = new DirectBitmap(this.Image.Bits, this.Image.Width, this.Image.Height);

            Projector.Instance.UncertaintyMax = this.UncertaintyImage.Bits.Length;

            DirectBitmap image = this.UncertaintyImage;
            DirectBitmap baseImage = new DirectBitmap(this.Image.Bits, this.Image.Width, this.Image.Height);
            List<Color> palette = Projector.Instance.PaletteChoice == Projector.PaletteChoices.PopularityPalette ? this.PopularityPaletteColors : this.KMeansPaletteColors;

            this.UpdateImage(palette, baseImage, image, Functors.Loop2D.Synchronous(image.Width, image.Height), (int count) => Projector.Instance.UncertaintyProgress += count, true);
            Projector.Instance.UncertaintyProgress += Projector.Instance.UncertaintyMax;

            Projector.Instance.UncertaintyGenerated = true;
            Projector.Instance.UncertaintySemaphore.Release();
            Projector.Instance.UncertaintyPictureBox.BeginInvoke(new Action(() => Projector.Instance.RefreshPBs()));
        }

        #endregion

        #region Dithering kernel

        private void UpdateImage(List<Color> palette, DirectBitmap baseImage, DirectBitmap Image, Functors.Loop2D.Prototype loop, Action<int> updateProgress, bool filter = false)
        {
            int counter = 0;

            loop((int x, int y) => 
            { 
                counter++;

                Color original = baseImage.GetPixel(x, y);
                Color dithered = Functors.CalculateNearestColor(original, palette);

                if (filter) this.Filter(baseImage, x, y, dithered, original, palette);

                Image.SetPixel(x, y, dithered);

                if (counter > 1000)
                {
                    updateProgress(counter);
                    counter = 0;
                }
            });
        }

        private readonly double[,] FS = {
            { 0, 0, 0 },
            { 0, 0, 7.0 / 16.0 },
            { 3.0 / 16.0, 5.0 / 16.0, 1.0 / 16.0}
        };

        private readonly double[,] B = {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 8.0 / 32.0, 4.0 / 32.0 },
            { 2.0 / 32.0, 4.0 / 32.0, 8.0 / 32.0, 4.0 / 32.0, 2.0 / 32.0}
        };

        private readonly double[,] S = {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 8.0 / 42.0, 4.0 / 42.0 },
            { 2.0 / 42.0, 4.0 / 42.0, 8.0 / 42.0, 4.0 / 42.0, 2.0 / 42.0 },
            { 1.0 / 42.0, 2.0 / 42.0, 4.0 / 42.0, 2.0 / 42.0, 1.0 / 42.0 }
        };

        private void Filter(DirectBitmap bitmap, int x, int y, Color dithered, Color original, List<Color> palette)
        {
            double[,] arr;

            switch(Projector.Instance.ErrorDifusion)
            {
                case Projector.ErrorDifusionMethod.FS: 
                    arr = this.FS;
                    break;
                case Projector.ErrorDifusionMethod.S:
                    arr = this.S;
                    break;
                case Projector.ErrorDifusionMethod.B:
                    arr = this.B;
                    break;
                default:
                    arr = new double[0, 0];
                    break;
            }

            int eR = original.R - dithered.R;
            int eG = original.G - dithered.G;
            int eB = original.B - dithered.B;

            int bi = (int)Math.Ceiling(arr.GetLength(0) / 2.0) - 1;
            int bj = (int)Math.Ceiling(arr.GetLength(1) / 2.0) - 1;

            bool firstIteration = true;
            
            for (int j = bj; j < arr.GetLength(1); j++)
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    if (firstIteration)
                    {
                        i = bi + 1;
                        firstIteration = false;
                    }

                    if (x + i - bi < 0 || y + j - bj < 0 || x + i - bi >= bitmap.Width || y + j - bj >= bitmap.Height)
                        continue;

                    Color nx = bitmap.GetPixel(x + i - bi, y + j - bj);

                    int R = Math.Max(0, Math.Min(255, nx.R + (int)Math.Round(eR * arr[i, j])));
                    int G = Math.Max(0, Math.Min(255, nx.G + (int)Math.Round(eG * arr[i, j])));
                    int B = Math.Max(0, Math.Min(255, nx.B + (int)Math.Round(eB * arr[i, j])));

                    bitmap.SetPixel(x + i - bi, y + j - bj, Color.FromArgb(R, G, B));
                }
        }

        #endregion
    }
}
