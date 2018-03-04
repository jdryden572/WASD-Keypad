using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeLib
{
    public delegate void CallStateEventHandler(object sender, CallStateEventArgs e);
    public delegate void MuteStateEventHandler(object sender, MuteStateEventArgs e);
    public delegate void ClientStateEventHandler(object sender, ClientStateEventArgs e);

    public class SkypeManager
    {
        private LyncClient lyncClient;
        private Conversation latestCall = null;

        public SkypeManager()
        {
            try
            {
                lyncClient = LyncClient.GetClient();
                lyncClient.StateChanged += LyncClient_StateChanged;
            }
            catch (Microsoft.Lync.Model.ClientNotFoundException ex)
            {
                throw new SkypeLib.ClientNotFoundException(ex.Message);
            }
            InitializeConversationSignups();
        }

        public event CallStateEventHandler CallStateChanged;
        public event MuteStateEventHandler MuteStateChanged;
        public event ClientStateEventHandler ClientStateChanged;

        /// <summary>
        /// Gets the current mute state of the active audio call
        /// </summary>
        public bool IsMuted {
            get {
                var result = latestCall?.SelfParticipant?.IsMuted;
                return result == null ? false : (bool)result;
            }
        }

        /// <summary>
        /// Indicates whether an audio call is currently active
        /// </summary>
        public bool HasActiveCall
        {
            get
            {
                return latestCall != null;
            }
        }

        /// <summary>
        /// Gets the name of the currently active audio call
        /// </summary>
        public string ActiveCallName
        {
            get
            {
                if (latestCall is null) return null;

                if(latestCall.Participants.Count == 2)
                {
                    return latestCall.Participants[1].Properties[ParticipantProperty.Name].ToString();
                }
                else if(latestCall.Participants.Count > 2)
                {
                    return $"{latestCall.Participants.Count} Participants";
                }
                else
                {
                    return "No participants";
                }
            }
        }

        public SkypeLib.ClientState ClientState
        {
            get
            {
                return (SkypeLib.ClientState)lyncClient.State;
            }
        }

        /// <summary>
        /// Toggles the mute state of the active audio call, if there is one
        /// </summary>
        public void T()
        {
            var conv = latestCall;
            if(conv != null)
            {
                conv.SelfParticipant.BeginSetMute(
                    !IsMuted,
                    (ar) =>
                    {
                        conv.SelfParticipant.EndSetMute(ar);
                    },
                    null
                    );
            }
        }

        /// <summary>
        /// Hangs up the active audio call, if there is one
        /// </summary>
        public void HangUpCall()
        {
            var avModality = latestCall?.Modalities[ModalityTypes.AudioVideo];
            if(avModality != null)
            {
                avModality.BeginDisconnect(ModalityDisconnectReason.None, 
                    (ar) =>
                    {
                        avModality.EndDisconnect(ar);
                    },
                    null);
            }
        }

        protected virtual void OnMuteStateChanged(MuteStateEventArgs e)
        {
            MuteStateChanged?.Invoke(this, e);
        }

        protected virtual void OnCallStateChanged(CallStateEventArgs e)
        {
            CallStateChanged?.Invoke(this, e);
        }

        protected virtual void OnClientStateChanged(ClientStateEventArgs e)
        {
            ClientStateChanged?.Invoke(this, e);
        }

        private void InitializeConversationSignups()
        {
            lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;

            foreach(var conv in lyncClient.ConversationManager.Conversations)
            {
                Modality avModality = conv.Modalities[ModalityTypes.AudioVideo];
                avModality.ModalityStateChanged += Modality_ModalityStateChanged;
                if(avModality.State == ModalityState.Connected)
                {
                    latestCall = conv;
                    latestCall.SelfParticipant.IsMutedChanged += Participant_IsMutedChanged;
                }
            }
        }

        private void LyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            Console.WriteLine($"Client state: {e.NewState}");
            OnClientStateChanged(new ClientStateEventArgs(e.NewState));
        }

        private void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            e.Conversation.Modalities[ModalityTypes.AudioVideo].ModalityStateChanged += Modality_ModalityStateChanged;
        }

        private void Modality_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            Conversation conversation = ((Modality)sender).Conversation;

            // Audio call connected, this is the active call
            if(e.NewState == ModalityState.Connected)
            {
                latestCall = conversation;
                latestCall.SelfParticipant.IsMutedChanged += Participant_IsMutedChanged;
                OnCallStateChanged(new CallStateEventArgs(ActiveCallName));
            }
            // Just disconnected or put the active audio call on hold
            else if(e.OldState == ModalityState.Connected)
            {
                if (latestCall == conversation) latestCall = null;
                conversation.SelfParticipant.IsMutedChanged -= Participant_IsMutedChanged;
                OnCallStateChanged(new CallStateEventArgs(ActiveCallName));
            }
        }

        private void Participant_IsMutedChanged(object sender, MutedChangedEventArgs e)
        {
            OnMuteStateChanged(new MuteStateEventArgs(e.IsMuted));
        }
    }
}
