using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeLib
{
    public class CallStateEventArgs : EventArgs
    {
        public string Participant { get; set; }
        public CallStateEventArgs(string participant)
        {
            Participant = participant;
        }
    }
}
