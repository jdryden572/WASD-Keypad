using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeLib
{
    public class MuteStateEventArgs : EventArgs
    {
        public bool IsMuted { get; set; }
        public MuteStateEventArgs(bool isMuted)
        {
            IsMuted = isMuted;
        }
    }
}
