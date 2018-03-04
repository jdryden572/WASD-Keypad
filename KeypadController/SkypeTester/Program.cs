using SkypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkypeTester
{
    class Program
    {
        static SkypeManager skypeManager;
        static Timer skypeRetryTimer;

        static void Main(string[] args)
        {
            if(TryConnectSkypeManager())
            {
                Console.WriteLine("Press ENTER to quit");
                PrintState(skypeManager.ActiveCallName, skypeManager.IsMuted);
            }
            else
            {
                skypeRetryTimer = new Timer(RetrySkypeManagerConnection, null, 5000, 5000);
            }

            Console.ReadLine();
        }

        static bool TryConnectSkypeManager()
        {
            Console.WriteLine("Trying to connect to Skype...");
            try
            {
                skypeManager = new SkypeManager();
                skypeManager.CallStateChanged += SkypeManager_CallStateChanged;
                skypeManager.MuteStateChanged += SkypeManager_MuteStateChanged;
                skypeManager.ClientStateChanged += SkypeManager_ClientStateChanged;
                return true;
            }
            catch (Exception ex)
            {
                skypeManager = null;
                return false;
            }
        }

        static void RetrySkypeManagerConnection(object sender)
        {
            if(TryConnectSkypeManager())
            {
                skypeRetryTimer.Dispose();
                Console.WriteLine("Connected to Skype.");
                PrintState(skypeManager.ActiveCallName, skypeManager.IsMuted);
            }
        }

        static void CheckIfSkypeExited(object sender)
        {
            if(skypeManager.ClientState == ClientState.Invalid)
            {
                Console.WriteLine("Skype client was closed. Starting 5 second reconnect cycle.");
                skypeRetryTimer.Dispose();
                skypeRetryTimer = new Timer(RetrySkypeManagerConnection, null, 5000, 5000);
            }
            else if (skypeManager.ClientState == ClientState.SignedIn)
            {
                Console.WriteLine("Skype client signed in again.");
                skypeRetryTimer.Dispose();
            }
        }

        static void PrintState(string activeCall, bool isMuted)
        {
            Console.WriteLine("{0,-20} {1}", activeCall ?? "--", isMuted ? "Muted" : "--");
        }

        static void SkypeManager_CallStateChanged(object sender, CallStateEventArgs e)
        {
            PrintState(e.Participant, skypeManager.IsMuted);
        }

        static void SkypeManager_MuteStateChanged(object sender, MuteStateEventArgs e)
        {
            PrintState(skypeManager.ActiveCallName, e.IsMuted);
        }

        static void SkypeManager_ClientStateChanged(object sender, ClientStateEventArgs e)
        {
            // The ShuttingDown event doesn't seem to actually get fired when Skype exits.
            // Need to poll the client to see if it is active, that's the only way to tell if 
            // it actually exited.
            Console.WriteLine(e.NewState);
            switch(e.NewState)
            {
                case ClientState.SignedOut:
                    skypeRetryTimer = new Timer(CheckIfSkypeExited, null, 5000, 5000);
                    break;
                case ClientState.SignedIn:
                    skypeRetryTimer?.Dispose();
                    break;
            }
        }
    }
}
