using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab03
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();

        #region Paint Events

        private void MainCanvasPaint(object sender, PaintEventArgs e) =>
            this.DrawImage(Projector.Instance.MainBitmap, e.Graphics, this.MainPictureBox.Width, this.MainPictureBox.Height);

        private void UncertaintyPaint(object sender, PaintEventArgs e)
        {
            Projector.Instance.UncertaintySemaphore.WaitOne();
            Projector.Instance.UncertaintySemaphore.Release();

            if (Projector.Instance.UncertaintyGenerated)
                this.DrawImage(Projector.Instance.UncertaintyBitmap, e.Graphics, this.PropagationOfUncertaintyPictureBox.Width, this.PropagationOfUncertaintyPictureBox.Height);
        }

        private void PopularityPaint(object sender, PaintEventArgs e)
        {
            Projector.Instance.PopularitySemaphore.WaitOne();
            Projector.Instance.PopularitySemaphore.Release();

            if (Projector.Instance.PopularityGenerated)
                this.DrawImage(Projector.Instance.PopularityBitmap, e.Graphics, this.PopularityAlgorithmPictureBox.Width, this.PopularityAlgorithmPictureBox.Height);
        }

        private void KMeansPaint(object sender, PaintEventArgs e)
        {
            Projector.Instance.KMeansSemaphore.WaitOne();
            Projector.Instance.KMeansSemaphore.Release();

            if (Projector.Instance.KMeansGenerated)
                this.DrawImage(Projector.Instance.KMeansBitmap, e.Graphics, this.KMeansPictureBox.Width, this.KMeansPictureBox.Height);
        }

        private void DrawImage(Image image, Graphics gx, int width, int height)
        {
            gx.DrawImage(image, Functors.ScaleImage(image, width, height));
        }

        #endregion

        #region UI Events

        private void ClusterSizeScroll(object sender, EventArgs e)
        {
            Projector.Instance.K = this.KTrackBar.Value;
            this.ColorTextBox.Text = this.KTrackBar.Value.ToString();
        }

        private void CreateImageButtonClick(object sender, EventArgs e)
        {
            Projector.Instance.GenerateDithered();
        }

        private void ClusterImageButtonClick(object sender, EventArgs e)
        {
            Projector.Instance.GenerateDithered();
        }

        private void FormLoad(object sender, EventArgs e)
        {
            Projector.Init(new Params(
                this.KTrackBar.Value,
                this.MainPictureBox, this.PropagationOfUncertaintyPictureBox, this.PopularityAlgorithmPictureBox, this.KMeansPictureBox,
                this.ImagesPanel, this.PopularityPanel, this.KMeansPanel,
                this.UncertaintyProgressBar, this.PopularityProgressBar, this.KMeansProgressBar, this.PopularityPalettePB, this.KMeansPalettePB,
                this.UncertaintyTextBox, this.PopularityTextBox, this.KMeansTextBox, this.PopularityPaletteTB, this.KMeansPaletteTB));
            Projector.Instance.LoadImages();
            Projector.ImageButtonClick(Projector.Instance.GetDefaultImage(), null);

            this.ClusterSizeScroll(null, null);
            this.PaletteChoice(null, null);
        }

        private void FilterMatrixChanged(object sender, EventArgs e)
        {
            if (this.FSRadioButton.Checked)
            {
                Projector.Instance.ErrorDifusion = Projector.ErrorDifusionMethod.FS;
            }
            else if (this.SRadioButton.Checked)
            {
                Projector.Instance.ErrorDifusion = Projector.ErrorDifusionMethod.S;
            }
            else if (this.BRadioButton.Checked)
            {
                Projector.Instance.ErrorDifusion = Projector.ErrorDifusionMethod.B;
            }
        }

        private void PaletteChoice(object sender, EventArgs e)
        {
            if (this.KMeansPaletteRadioButton.Checked)
                Projector.Instance.PaletteChoice = Projector.PaletteChoices.KMeansPalette;
            else if (this.PopularityPaletteRadioButton.Checked)
                Projector.Instance.PaletteChoice = Projector.PaletteChoices.PopularityPalette;
        }

        #endregion

        #region Saving images

        private void UncertaintySave(object sender, EventArgs e) => this.SaveFileDialog(Projector.Instance.UncertaintyBitmap, "uncertainty");
        private void PopularitySave(object sender, EventArgs e) => this.SaveFileDialog(Projector.Instance.PopularityBitmap, "popularity");
        private void KMeansSave(object sender, EventArgs e) => this.SaveFileDialog(Projector.Instance.KMeansBitmap, "k-means");
        private void SaveFileDialog(Bitmap bitmap, string name)
        {
            // source: https://docs.microsoft.com/pl-pl/dotnet/desktop/winforms/controls/how-to-save-files-using-the-savefiledialog-component?view=netframeworkdesktop-4.8
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.FileName = name;
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        bitmap.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        bitmap.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        bitmap.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }
        }

        #endregion
    }
}
