using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections.Concurrent;

namespace lab03
{
    public class Cluster
    {
        public Color Representative { get; set; } = Color.Empty;
        private ConcurrentDictionary<Color, Color> Colors { get; set; } = new ConcurrentDictionary<Color, Color>();

        public void Add(Color c)
        {
            this.Colors.TryAdd(c, c);
        }

        public Color Remove(Color c)
        {
            this.Colors.TryRemove(c, out Color ret);
            return ret;
        }

        public void CalculateRepresentative()
        {
            if (Projector.Instance.ThreadStop)
                return;

            if (this.Colors.Count == 0)
                return;

            int R = 0;
            int G = 0;
            int B = 0;

            foreach (var c in this.Colors.Values)
            {
                R += c.R;
                G += c.G;
                B += c.B;
            }

            R /= this.Colors.Count;
            G /= this.Colors.Count; 
            B /= this.Colors.Count;

            R = Math.Max(0, Math.Min(255, R));
            G = Math.Max(0, Math.Min(255, G));
            B = Math.Max(0, Math.Min(255, B));

            this.Representative = Color.FromArgb(R, G, B);
        }
    }
}
