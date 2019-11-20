﻿using System;
using System.Linq;

namespace Emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Syntax: Emulator [Port]");
                Console.WriteLine("     Port            Serial Port Name (e.g.\"COM1\")");
                return;
            }

            Console.WriteLine("Commands");
            Console.WriteLine("[1] Emulate card roll");
            Console.WriteLine("[q] Quit");

            var portName = args[0];
            var msrEmulator = new MsrEmulator();
            msrEmulator.Start(portName);
            do
            {
                var c = Console.ReadKey(true);
                if (c.KeyChar == '1')
                    msrEmulator.EmulateCardRoll();
                if (c.KeyChar == 'q')
                    break;
            } while (true);
            msrEmulator.Dispose();
        }
    }
}
