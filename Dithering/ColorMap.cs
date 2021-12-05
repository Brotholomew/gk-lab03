using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Priority_Queue;

namespace lab03
{
    public class ColorNode : GenericPriorityQueueNode<int>
    {
        public Color Color { get; set; }
        public ColorNode(Color _color) 
        {
            this.Color = _color;
        }
    }

    public class ColorMap
    {
        private HashSet<ColorNode> visited;
        private SimplePriorityQueue<ColorNode, int> Q;
        private DirectBitmap Image;

        public List<Color> MostUsedPalette;

        #region Constructors

        private void Init()
        {
            this.visited = new HashSet<ColorNode>();
            this.Q = new SimplePriorityQueue<ColorNode, int>(new More(), new ColorComparer());
            this.MostUsedPalette = new List<Color>();
        }

        public ColorMap()
        {
            this.Init();
        }

        public ColorMap(DirectBitmap img)
        {
            this.Image = img;
            this.Init();
        }

        #endregion

        public void LoadImage(DirectBitmap img)
        {
            this.Clear();
            this.Image = img;
        }

        public void Clear()
        {
            this.visited.Clear();
            this.Q.Clear();
            this.MostUsedPalette.Clear();
        }

        public void GenerateMap()
        {
            int X = this.Image.Width;
            int Y = this.Image.Height;
            int counter = 0;

            for (int x = 0; x < X; x++)
                for (int y = 0; y < Y; y++)
                {
                    counter++;

                    ColorNode node = new ColorNode(this.Image.GetPixel(x, y));

                    if (this.Q.Contains(node))
                        this.Q.UpdatePriority(node, this.Q.GetPriority(node) + 1);
                    else
                        this.Q.Enqueue(node, 1);

                    if (Projector.Instance.ThreadStop) return;

                    if (counter > 1000)
                    {
                        Projector.Instance.PopularityPaletteProgress += counter;
                        counter = 0;
                    }
                }

            this.MostUsedPalette = this.Q.ToList<Color>();
        }
    }
}
