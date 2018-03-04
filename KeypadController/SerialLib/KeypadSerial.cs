using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialLib
{
    public enum KeypadAction
    {
        Key0_Down = 243,
        Key1_Down = 244,
        Key2_Down = 245,
        Key3_Down = 246,
        Key4_Down = 247,
        Key5_Down = 248,
        Key0_Up = 249,
        Key1_Up = 250,
        Key2_Up = 251,
        Key3_Up = 252,
        Key4_Up = 253,
        Key5_Up = 254,
    }

    public delegate void KeypadEventHandler(object sender, KeypadActionEventArgs e);

    public class KeypadSerial : IDisposable
    {
        const byte _REQ = 224;
        const byte _ACK = 225;
        const byte _heartbeatRetries = 5;

        private SerialPort _comPort;
        private Timer _hearbeatTimer;

        public string PortName => _comPort?.PortName;
        public int BaudRate => _comPort?.BaudRate ?? 0;
        public bool IsOpen => _comPort?.IsOpen ?? false;

        public event KeypadEventHandler KeyChanged;

        public void OnKeyChanged(KeypadActionEventArgs e)
        {
            if(KeyChanged != null)
            {
                KeyChanged(this, e);
            }
        }

        public KeypadSerial(string port, int baudRate)
        {
            _comPort = new SerialPort(port);
            _comPort.BaudRate = baudRate;
            _comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            LightShow();
            _hearbeatTimer = new Timer(OnHeartbeatTimer, null, 5000, 5000);
        }

        public KeypadSerial()
        {
            // Default constructor will search for connected keypad
            SearchForKeypad();
            _comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            LightShow();
            _hearbeatTimer = new Timer(OnHeartbeatTimer, null, 5000, 5000);
        }

        public void Open()
        {
            if(!_comPort.IsOpen)
            {
                _comPort.Open();
                _comPort.DiscardInBuffer();
            }
        }

        public void SendByte(byte data)
        {
            byte[] arrayOfOneByte = { data };
            _comPort.Write(arrayOfOneByte, 0, 1);
        }

        private void OnHeartbeatTimer(object sender)
        {
            bool foundKeypad = false;
            try
            {
                // Retry handshake up to configured tries in case a keypress sneaks in
                // between the handshake REQ and ACK
                int attempts = 0;
                do
                {
                    attempts++;
                    SendByte(_REQ);
                    foundKeypad = _comPort.ReadByte() == _ACK;
                }
                while (attempts < _heartbeatRetries && !foundKeypad);
                
                if (!foundKeypad)
                {
                    string msg = "Keypad has stopped responding to heartbeats correctly.";
                    throw new System.IO.IOException(msg);
                }
            }
            catch (InvalidOperationException)
            {
                // Port has been closed
                try
                {
                    _comPort.Open();
                }
                catch
                {
                    // Last-ditch effort, search for different COM port
                    // Leave any remaining exception unhandled, return to caller
                    SearchForKeypad();
                }
            }
        }

        private void SearchForKeypad()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                foreach(string port in ports)
                {
                    if (TryConnectPort(port)) break;
                }
                if(_comPort == null)
                {
                    string msg = "Could not locate any keypad devices.";
                    throw new System.IO.IOException(msg);
                }
            }
            else
            {
                string msg = "No COM ports detected.";
                throw new System.IO.IOException(msg);
            }
        }

        private bool TryConnectPort(string port)
        {
            bool foundKeypad = false;
            try
            {
                _comPort = new SerialPort(port);
                _comPort.BaudRate = 115200;
                _comPort.WriteTimeout = 1000;
                _comPort.ReadTimeout = 1000;
                _comPort.Open();
                _comPort.DiscardInBuffer();
                SendByte(_REQ);
                if(_comPort.ReadByte() == _ACK)
                {
                    foundKeypad = true;
                }
            }
            catch { }
            finally
            {
                if(!foundKeypad)
                {
                    if (_comPort != null && _comPort.IsOpen)
                    {
                        _comPort.Close();
                    }
                    _comPort = null;
                }
            }
            return foundKeypad;
        }

        private void LightShow()
        {
            for (int i = 5; i >= 0; i--)
            {
                SendByte((byte)(1 << i));
                Thread.Sleep(100);
            }
            for (int i = 0; i < 6; i++)
            {
                SendByte((byte)(1 << i));
                Thread.Sleep(100);
            }
            SendByte(0);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            while(sp.BytesToRead > 0)
            {
                byte inByte = (byte)sp.ReadByte();
                OnKeyChanged(new KeypadActionEventArgs((KeypadAction)inByte));
                //Console.WriteLine((KeypadAction)inByte);
            }
        }

        public void Dispose()
        {
            if(_comPort != null)
            {
                _comPort.Close();
            }
            if(_hearbeatTimer != null)
            {
                _hearbeatTimer.Dispose();
            }
        }
    }
}
