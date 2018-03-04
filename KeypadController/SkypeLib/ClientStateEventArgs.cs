using Microsoft.Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeLib
{
    //
    // Summary:
    //     Enumerates the SignIn states of the Lync client.
    public enum ClientState
    {
        //
        Invalid = -1,
        //
        // Summary:
        //     Client is in uninitialized state
        Uninitialized = 0,
        //
        // Summary:
        //     Client is in signed out state.
        SignedOut = 1,
        //
        // Summary:
        //     Client is in signing in state.
        SigningIn = 2,
        //
        // Summary:
        //     Client is in signed in state.
        SignedIn = 3,
        //
        // Summary:
        //     Client is in signing out state.
        SigningOut = 4,
        //
        // Summary:
        //     Client is in shutting down state.
        ShuttingDown = 5,
        //
        // Summary:
        //     Client is initializing.
        Initializing = 6
    }

    public class ClientStateEventArgs : EventArgs
    {
        public ClientState NewState { get; set; }
        public ClientStateEventArgs(Microsoft.Lync.Model.ClientState newState)
        {
            NewState = (SkypeLib.ClientState)newState;
        }
    }
}
