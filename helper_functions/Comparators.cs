using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace lab03
{
    public class ColorComparer : IEqualityComparer<ColorNode>
    {
        public bool Equals([AllowNull] ColorNode x, [AllowNull] ColorNode y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (x.Color.R == y.Color.R && x.Color.G == y.Color.G && x.Color.B == y.Color.B)
                return true;

            return false;
        }

        public int GetHashCode([DisallowNull] ColorNode obj)
        {
            return obj.Color.GetHashCode();
        }
    }

    public class More : IComparer<int>
    {
        public int Compare([AllowNull] int x, [AllowNull] int y)
        {
            if (x == y) return 0;
            if (x > y) return -1;
            
            return -1;
        }
    }
}
