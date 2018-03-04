using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLib
{
    public class KeypadActionEventArgs : EventArgs
    {
        public KeypadAction KeyChanged { get; set; }
        public KeypadActionEventArgs(KeypadAction keyChanged)
        {
            KeyChanged = keyChanged;
        }
    }
}
