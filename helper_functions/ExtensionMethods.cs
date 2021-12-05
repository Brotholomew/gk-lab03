using System;
using System.Collections.Generic;
using System.Text;
using Priority_Queue;
using System.Drawing;

namespace lab03
{
    public static class ExtensionMethods
    {
        public static List<System.Drawing.Color> ToList<Color>(this SimplePriorityQueue<ColorNode, int> Q) 
        {
            var ret = new List<System.Drawing.Color>();
            foreach (var node in Q)
            {
                ret.Add(node.Color);
            }

            return ret;
        }
    }
}
