using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace lab03
{
    public class ClusterCollection
    {
        private ConcurrentDictionary<Color, Cluster> Assigned = new ConcurrentDictionary<Color, Cluster>();
        private HashSet<Cluster> Clusters = new HashSet<Cluster>();
        private List<Color> Colors = new List<Color>();
        private int K;

        public ClusterCollection(List<Color> colors, int k)
        {
            this.Colors = colors;
            this.K = k;
            this.Init();
        }

        private void Init() => this.RandomPartition();

        public void UpdateClusters()
        {
            bool changed = true;

            while(changed)
            {
                changed = false;
                int counter = 0;

                Parallel.ForEach(this.Colors, (Color c, ParallelLoopState state) =>
                {
                    counter++;

                    Cluster cluster = Functors.CalculateNearestCluster(c, this.Clusters);
                    this.Assigned.TryGetValue(c, out Cluster assigned);

                    if (assigned == null && cluster != null)
                    {
                        cluster.Add(c);
                        changed = true;
                    }
                    else if (assigned != cluster && cluster != null)
                    {
                        assigned.Remove(c);
                        cluster.Add(c);
                        changed = true;
                    }

                    this.Assigned[c] = cluster;

                    if (Projector.Instance.ThreadStop) state.Break();

                    if (counter > 1000)
                    {
                        if (Projector.Instance.KMeansPaletteProgress < Projector.Instance.KMeansPaletteMax * 3 / 4) 
                            Projector.Instance.KMeansPaletteProgress += counter / 10;
                        counter = 0;
                    }
                });

                this.CalculateRepresentantives();
            }
        }

        public List<Color> GetColors()
        {
            List<Color> ret = new List<Color>();
            
            foreach (Cluster cluster in this.Clusters)
                ret.Add(cluster.Representative);

            return ret;
        }

        private void RandomPartition() 
        {
            List<Cluster> clusters = new List<Cluster>();
            var rnd = new Random();

            foreach (var i in Enumerable.Range(0, this.K))
                clusters.Add(new Cluster());

            foreach (var c in this.Colors)
            {
                if (Projector.Instance.ThreadStop)
                    return;

                int i = rnd.Next(clusters.Count);

                this.Assigned.TryAdd(c, clusters[i]);
                clusters[i].Add(c);
            }

            foreach (var c in clusters)
                this.Clusters.Add(c);

            this.CalculateRepresentantives();
        }

        private void CalculateRepresentantives()
        {
            Parallel.ForEach(this.Clusters, (Cluster c, ParallelLoopState state) => 
            {
                if (Projector.Instance.ThreadStop) state.Break(); 
                c.CalculateRepresentative();
            });
        }
    }
}
