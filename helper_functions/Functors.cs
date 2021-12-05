using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace lab03
{
    public static class Functors
    {
        public static Color CalculateNearestColor(Color c, List<Color> lc)
        {
            return CalculateNearest<Color>(c, lc, (IEnumerable<Color> l, Action<Color, Color> a) =>
            {
                foreach (var color in l)
                    a(color, color);
            },
            Color.Empty);
        }

        public static Cluster CalculateNearestCluster(Color c, IEnumerable<Cluster> lc)
        {
            return CalculateNearest<Cluster>(c, lc, (IEnumerable<Cluster> l, Action<Cluster, Color> a) => 
            { 
                foreach (var cluster in l) 
                    a(cluster, cluster.Representative);
            },
            null);
        }

        private static T CalculateNearest<T>(Color c, IEnumerable<T> l, Action<IEnumerable<T>, Action<T, Color>> loop, T empty)
        {
            double min = double.MaxValue;
            T rc = empty;

            loop(l, (T o, Color color) =>
            {
                double distance = Math.Pow(c.R - color.R, 2) + Math.Pow(c.G - color.G, 2) + Math.Pow(c.B - color.B, 2); ;

                //if (Projector.Instance.Cache.ContainsKey((color, c)))
                //    distance = Projector.Instance.Cache[(color, c)];
                //else
                //{
                //    distance = Math.Pow(c.R - color.R, 2) + Math.Pow(c.G - color.G, 2) + Math.Pow(c.B - color.B, 2);
                //    Projector.Instance.Cache.TryAdd((color, c), distance);
                //    Projector.Instance.Cache.TryAdd((c, color), distance);
                //}

                if (distance < min)
                {
                    min = distance;
                    rc = o;
                }
            });

            return rc;
        }

        public static Rectangle ScaleImage(Image image, int width, int height)
        {
            Rectangle ret = new Rectangle();
            double scale = Math.Min((double) width / (double) image.Width, (double) height / (double) image.Height);

            ret.Width = (int)(image.Width * scale);
            ret.Height = (int)(image.Height * scale);

            ret.X = (int)((width - ret.Width) / 2.0);
            ret.Y = (int)((height - ret.Height) / 2.0);

            return ret;
        }

        public static class Loop2D
        {
            public delegate void Prototype(Action<int, int> a);

            public static Prototype Parallel(List<(int x, int y)> range)
            {
                return (Action<int, int> a) =>
                {
                    System.Threading.Tasks.Parallel.ForEach(range, ((int x, int y) t, System.Threading.Tasks.ParallelLoopState state) =>
                    {
                        a(t.x, t.y); 
                        
                        if (Projector.Instance.ThreadStop) 
                            state.Break(); 
                    });
                };
            }

            public static Prototype Synchronous(List<(int x, int y)> range)
            {
                return (Action<int, int> a) =>
                {
                    foreach ((int x, int y) t in range)
                    {
                        a(t.x, t.y);

                        if (Projector.Instance.ThreadStop)
                            break;
                    }
                };
            }

            public static Prototype Parallel(int maxX, int maxY)
            {
                return Functors.Loop2D.Parallel(GenerateRange(maxX, maxY));
            }

            public static Prototype Synchronous(int maxX, int maxY)
            {
                return Functors.Loop2D.Synchronous(GenerateRange(maxX, maxY));
            }

            private static List<(int x, int y)> GenerateRange(int maxX, int maxY) 
            {
                List<(int x, int y)> range = new List<(int x, int y)>();
                
                for (int j = 0; j < maxY; j++)
                    for (int i = 0; i < maxX; i++)
                        range.Add((i, j));

                return range;
            }
        }
    }
}
