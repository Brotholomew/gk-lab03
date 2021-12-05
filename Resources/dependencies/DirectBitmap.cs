using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lab03
{
    // source: https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height) => this.Init(width, height);

        public DirectBitmap(Image image)
        {
            Bitmap tmp = new Bitmap(image);
            this.Init(tmp.Width, tmp.Height);
            this.CopyPixels(tmp);
        }

        public DirectBitmap(Int32[] bits, int width, int height)
        {
            this.Init(width, height);
            this.CopyPixels(bits);
        }

        private void CopyPixels(Int32[] bits)
        {
            List<(int x, int y)> loop = new List<(int x, int y)>();
            
            for (int x = 0; x < this.Width; x++)
                for (int y = 0; y < this.Height; y++)
                    loop.Add((x, y));

            Parallel.ForEach(loop, ((int x, int y) t, ParallelLoopState state) =>
            {
                if (Projector.Instance.ThreadStop) state.Break();

                this.SetPixel(t.x, t.y, Color.FromArgb(bits[t.x + t.y * this.Width]));
            });
        }

        private void CopyPixels(Bitmap image)
        {
            for (int i = 0; i < this.Width; i++)
                for (int j = 0; j < this.Height; j++)
                    this.SetPixel(i, j, image.GetPixel(i, j));
        }

        private void Init(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            if (index < 0 || index >= this.Width * this.Height)
                return;

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
