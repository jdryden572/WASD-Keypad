using SerialLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialTester
{
    class Program
    {
        static KeypadSerial keypad;

        static void Main(string[] args)
        {
            while(true)
            {
                try
                {
                    Console.WriteLine("Attempting to connect to keypad...");
                    using (keypad = new KeypadSerial())
                    {
                        try
                        {
                            keypad.Open();
                            keypad.KeyChanged += (object sender, KeypadActionEventArgs e) =>
                            {
                                Console.WriteLine(e.KeyChanged);
                            };
                            Console.WriteLine("Press keypad keys to test key events.");
                            Console.WriteLine("Enter a byte value to test keypad LEDs.");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // port already opened by someone else
                        }
                        catch (System.IO.IOException ex)
                        {
                            // port not connected
                        }
                        catch (Exception ex)
                        {
                            // unknown
                            throw ex;
                        }

                        int parsedInt;
                        while (true)
                        {
                            string userInput = Console.ReadLine();
                            if (int.TryParse(userInput, out parsedInt))
                            {
                                if (parsedInt < 256)
                                {
                                    keypad.SendByte((byte)parsedInt);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught exception: {e.GetType()} : {e.Message}");
                    Console.WriteLine("Retrying connection in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
