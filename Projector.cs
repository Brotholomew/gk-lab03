using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;

namespace lab03
{
    internal class Projector
    {

        #region Singleton Fields and Methods

        private static Projector _Instance = null;
        private Projector(Params p)
        {
            this.K = p.K;
            this.MainPictureBox = p.Main;
            this.UncertaintyPictureBox = p.Uncertainty;
            this.PopularityPictureBox = p.Popularity;
            this.KMeansPictureBox = p.KMeans;

            this.KMeansSemaphore = new Semaphore(1, 1);
            this.PopularitySemaphore = new Semaphore(1, 1);
            this.UncertaintySemaphore = new Semaphore(1, 1);

            this.ImagePanel = p.ImagePanel;
            this.PopularityPanel = p.PopularityPanel;
            this.KMeansPanel = p.KMeansPanel;

            this.Quantizer = new Quantizer();

            this.UncertaintyTB = p.UncertaintyTB;
            this.PopularityTB = p.PopularityTB;
            this.KMeansTB = p.KMeansTB;
            this.PopularityPaletteTB = p.PopularityPaletteTB;
            this.KMeansPaletteTB = p.KMeansPaletteTB;

            this.UncertaintyPB = p.UncertaintyPB;
            this.PopularityPB = p.PopularityPB;
            this.KMeansPB = p.KMeansPB;
            this.PopularityPalettePB = p.PopularityPalettePB;
            this.KMeansPalettePB = p.KMeansPalettePB;
        }

        public static Projector Instance => Projector._Instance;

        public static void Init(Params p)
        {
            Projector._Instance = new Projector(p);
        }

        #endregion

        #region GUI Elements

        private Dictionary<String, String> ImagesList;
        private FlowLayoutPanel ImagePanel { get; set; }

        public FlowLayoutPanel PopularityPanel { get; set; }
        public FlowLayoutPanel KMeansPanel { get; set; }

        public void LoadImages()
        {
            this.ImagesList = new Dictionary<String, String>();

            this.ImagesList.Add("mini", "Resources\\img\\mini.jpg");
            this.ImagesList.Add("lotr", "Resources\\img\\lotr.jpg");
            this.ImagesList.Add("got", "Resources\\img\\dragonstone.jpg");
            this.ImagesList.Add("landscape1", "Resources\\img\\landscape1.jpg");
            this.ImagesList.Add("landscape2", "Resources\\img\\landscape2.jpg");
            this.ImagesList.Add("bricks", "Resources\\img\\bricks.jpg");

            ExtendedButton firtsbtn = new ExtendedButton(new DirectBitmap(Image.FromFile(this.ImagesList["landscape1"])));
            this.EmbellishButton(firtsbtn);
            this.ImagePanel.Controls.Add(firtsbtn);

            Thread t = new Thread(this.GenerateButtons);
            t.Start();
        }

        public void AddImage(string title, string path)
        {
            this.ImagesList.Add(title, path);

            ExtendedButton firtsbtn = new ExtendedButton(new DirectBitmap(Image.FromFile(this.ImagesList["landscape1"])));
            this.EmbellishButton(firtsbtn);
            this.ImagePanel.Controls.Add(firtsbtn);

            Thread t = new Thread(this.GenerateButtons);
            t.Start();
        }

        private void GenerateButtons()
        {
            foreach (var path in this.ImagesList.Values)
            {
                if (path.Contains("landscape1"))
                    continue;

                DirectBitmap bitmap = new DirectBitmap(Image.FromFile(path));

                this.ImagePanel.BeginInvoke(new Action(() =>
                {
                    ExtendedButton button = new ExtendedButton(bitmap);
                    this.EmbellishButton(button);
                    this.ImagePanel.Controls.Add(button);
                }));
            }
        }

        private void EmbellishButton(ExtendedButton button)
        {
            int width = (this.ImagePanel.Width - 30);
            int height = (int)(this.ImagePanel.Height / 6);

            button.BackgroundImage = button.Bitmap.Bitmap;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.Width = width;
            button.Height = height;
            button.Click += Projector.ImageButtonClick;
        }

        public static void ImageButtonClick(object sender, EventArgs e)
        {
            ExtendedButton button = (ExtendedButton)sender;
            Projector.Instance.LoadImage(button.Bitmap);
        }

        public ExtendedButton GetDefaultImage() => (ExtendedButton)this.ImagePanel.Controls[0];

        #endregion

        #region Quantizer

        private Quantizer Quantizer;

        public Bitmap MainBitmap { get => this.Quantizer.Image.Bitmap; }
        public Bitmap UncertaintyBitmap { get => this.Quantizer.UncertaintyImage.Bitmap; }
        public Bitmap PopularityBitmap { get => this.Quantizer.PopularityImage.Bitmap; }
        public Bitmap KMeansBitmap { get => this.Quantizer.KMeansImage.Bitmap; }

        public enum ErrorDifusionMethod { FS, B, S };
        public ErrorDifusionMethod ErrorDifusion { get; set; }

        public bool ThreadStop { get; set; } = false;

        public enum PaletteChoices { KMeansPalette, PopularityPalette }
        public PaletteChoices PaletteChoice { get; set; }

        public void LoadImage(DirectBitmap img)
        {
            this.Quantizer.LoadImage(img);
            this.MainPictureBox.Refresh();
        }
        public void GenerateDithered() => this.Quantizer.GenerateDithered();
        public void GenerateUncertainty() => this.Quantizer.GenerateUncertainty();

        public Semaphore UncertaintySemaphore { get; private set; }
        public Semaphore PopularitySemaphore { get; private set; }
        public Semaphore KMeansSemaphore { get; private set;}

        public void RefreshPBs()
        {
            if (this.KMeansGenerated)
                this.KMeansPictureBox.Refresh();

            if (this.PopularityGenerated)
                this.PopularityPictureBox.Refresh();

            if (this.UncertaintyGenerated)
                this.UncertaintyPictureBox.Refresh();
        }

        #endregion

        #region Fields from GUI

        public int K { get; set; }
        public PictureBox MainPictureBox { get; private set; }
        public PictureBox UncertaintyPictureBox { get; private set; }
        public PictureBox PopularityPictureBox { get; private set; }
        public PictureBox KMeansPictureBox { get; private set; }

        public bool UncertaintyGenerated { get; set; } = false;
        public bool PopularityGenerated { get; set; } = false;
        public bool KMeansGenerated { get; set; } = false;

        #endregion

        #region Progress Bars

        public ProgressBar UncertaintyPB { get; private set; }
        public ProgressBar PopularityPB { get; private set; }
        public ProgressBar KMeansPB { get; private set; }
        public ProgressBar PopularityPalettePB { get; private set; }
        public ProgressBar KMeansPalettePB { get; private set; }

        public TextBox UncertaintyTB { get; private set; }
        public TextBox PopularityTB { get; private set; }
        public TextBox KMeansTB { get; private set; }
        public TextBox PopularityPaletteTB { get; private set; }
        public TextBox KMeansPaletteTB { get; private set; }

        private int _UncertaintyProgress = 0;
        private int _PopularityProgress = 0;
        private int _KMeansProgress = 0;
        private int _PopularityPaletteProgress = 0;
        private int _KMeansPaletteProgress = 0;
        
        public int UncertaintyProgress 
        { 
            get => this._UncertaintyProgress; 
            set => this.UpdateFields(this.UncertaintyPB, this.UncertaintyTB, value, this.UncertaintyMax, ref this._UncertaintyProgress); 
        }
        public int PopularityProgress 
        {
            get => this._PopularityProgress;
            set => this.UpdateFields(this.PopularityPB, this.PopularityTB, value, this.PopularityMax, ref this._PopularityProgress);
        }
        public int KMeansProgress 
        {
            get => this._UncertaintyProgress;
            set => this.UpdateFields(this.KMeansPB, this.KMeansTB, value, this.KMeansMax, ref this._KMeansProgress);
        }
        public int PopularityPaletteProgress
        {
            get => this._PopularityPaletteProgress;
            set => this.UpdateFields(this.PopularityPalettePB, this.PopularityPaletteTB, value, this.PopularityPaletteMax, ref this._PopularityPaletteProgress);
        }
        public int KMeansPaletteProgress
        {
            get => this._KMeansPaletteProgress;
            set => this.UpdateFields(this.KMeansPalettePB, this.KMeansPaletteTB, value, this.KMeansPaletteMax, ref this._KMeansPaletteProgress);
        }

        public int UncertaintyMax { get; set; } = 1;
        public int PopularityMax { get; set; } = 1;
        public int KMeansMax { get; set; } = 1;
        public int PopularityPaletteMax { get; set; } = 1;
        public int KMeansPaletteMax { get; set; } = 1;

        private void UpdateFields(ProgressBar PB, TextBox TB, int value, int max, ref int changed)
        {
            int percentage = Math.Max(0, Math.Min(100, (int)((double)value / (double)max * 100)));
            PB.BeginInvoke(new Action(() => PB.Value = percentage));
            TB.BeginInvoke(new Action(() => TB.Text = $"{percentage}%"));
            changed = value;
        }

        public void OverrideReset()
        {
            this._UncertaintyProgress = 0;
            this.UncertaintyPB.Value = 0;
            this.UncertaintyTB.Text = "0%";
        }

        #endregion
    }
}
