using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace lab03
{
    public struct Params
    {
        public int K;
         
        public PictureBox Main;
        public PictureBox Uncertainty;
        public PictureBox Popularity;
        public PictureBox KMeans;
         
        public FlowLayoutPanel ImagePanel;
        public FlowLayoutPanel PopularityPanel;
        public FlowLayoutPanel KMeansPanel;
         
        public ProgressBar UncertaintyPB;
        public ProgressBar PopularityPB;
        public ProgressBar KMeansPB;
        public ProgressBar PopularityPalettePB;
        public ProgressBar KMeansPalettePB;

        public TextBox UncertaintyTB;
        public TextBox PopularityTB;
        public TextBox KMeansTB;
        public TextBox PopularityPaletteTB;
        public TextBox KMeansPaletteTB;

        public Params(int k, 
            PictureBox main, PictureBox uncertainty, PictureBox popularity, PictureBox kmeans,
            FlowLayoutPanel imagePanel, FlowLayoutPanel popularityPanel, FlowLayoutPanel kmeansPanel,
            ProgressBar uncertaintyPB, ProgressBar popularityPB, ProgressBar kmeansPB, ProgressBar popularityPalettePB, ProgressBar kmeansPalettePB,
            TextBox uncertaintyTB, TextBox popularityTB, TextBox kmeansTB, TextBox popularityPaletteTB, TextBox kmeansPaletteTB)
        {
            this.K = k;

            this.Main = main;
            this.Uncertainty = uncertainty;
            this.Popularity = popularity;
            this.KMeans = kmeans;

            this.ImagePanel = imagePanel;
            this.PopularityPanel = popularityPanel;
            this.KMeansPanel = kmeansPanel;

            this.UncertaintyTB = uncertaintyTB;
            this.PopularityTB = popularityTB;
            this.KMeansTB = kmeansTB;
            this.PopularityPaletteTB = popularityPaletteTB;
            this.KMeansPaletteTB = kmeansPaletteTB;

            this.UncertaintyPB = uncertaintyPB;
            this.PopularityPB = popularityPB;
            this.KMeansPB = kmeansPB;
            this.PopularityPalettePB = popularityPalettePB;
            this.KMeansPalettePB = kmeansPalettePB;
        }
    }
}
