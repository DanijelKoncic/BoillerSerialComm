using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using Website.Data;


namespace BoillerSerialComm
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var argComPort = "COM2";                //args[0];
            //var argComPort = args[0];

            Console.WriteLine("START");

            var boillerObject = new SerialCommunicatorNew(argComPort);
            boillerObject.InitCommunication();

            boillerObject.Communicator();
            //if (boillerObject.CommunicationHandshake() == true)
            //{
            //    boillerObject.Communicator();
            //}
            //else
            //{
            //    Console.WriteLine("Communication hanshake error");        
            //}
            boillerObject.CloseCommunication();
            Console.WriteLine("STOP");
        }
    }
}