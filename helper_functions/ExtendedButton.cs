using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace lab03
{
    public class ExtendedButton : Button
    {
        public DirectBitmap Bitmap { get; }
        
        public ExtendedButton(DirectBitmap btmp)
        {
            this.Bitmap = btmp;
        }

        public ExtendedButton() { }
    }
}
