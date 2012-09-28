using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
            port.Open();
        }
    }
}
