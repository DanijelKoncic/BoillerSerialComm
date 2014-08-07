using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Website.Data;

namespace BoillerSerialComm
{
    class SerialCommunicator
    {
        #region Inicijalizacija podataka
        private SerialPort _serialPort;
        private static bool _clearToSend = true;
        int brojElemenataDictionarya;
        int trenutniElementDictionarya = 0;
        private Dictionary<string, Naredbe> dictionaryNaredbi = new Dictionary<string, Naredbe>()
                {
                    
                    //Something like initialization connection.
                    //# 07 # 02 # 00 # 00 # 00 # 04 # C4
                    //# 08 # 00 # 00 # 9E # 0A # 0C # 6B # FD
                    {"Initialize Comm", new Naredbe(){
                        Indeks                = 1,
                        SendCommand =  new byte[] { 0x07, 0x02, 0x00, 0x00, 0x00, 0x04, 0xC4 },
                        NumberOf_UsefullExpectedReturnDataBytes = 2,
                        NumberOf_AllExpectedReturnBytes = 8
                                    }},
                    
                    //Request to heat DHW C1/C2
                    //# 07 # 00 # 00 # 00 # 58 # 01 # 51
                    //# 04 # 00 # 00 # 10
                    //0 (OFF)
                    {"Request to heat DHW C1/C2", new Naredbe(){
                        Indeks                = 2,
                        SendCommand =  new byte[] { 0x07, 0x00, 0x00, 0x00, 0x58, 0x01, 0x51 },
                        NumberOf_UsefullExpectedReturnDataBytes = 2,
                        NumberOf_AllExpectedReturnBytes = 4
                                    }},
                };

        //delegate is used to write to a UI control from a non-UI thread
        //private delegate void SetTextDeleg(string text);

        //private Dictionary<string, byte[]> naredbeDictionary; 
        //private Dictionary<string, byte[]> naredbeDictionary = new Dictionary<string, byte[]>()
        //{
        //    {"Joe", new byte[] { 0x07, 0x02, 0x00, 0x00, 0x00, 0x04, 0xC4 }},   //Inicijalizacija
        //    {"Joe", new byte[] {1,1}},
        //};

        //public event EventHandler ReadyToSend;
        //public event EventHandler ReadyToReceive;

        private ManualResetEvent sendEvent = new ManualResetEvent(false);
        private ManualResetEvent receiveEvent = new ManualResetEvent(false);

        #endregion

        public SerialCommunicator(string serialPortName)
        {
            _serialPort = new SerialPort
                {
                    PortName = serialPortName,
                    BaudRate = 9600,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    WriteTimeout = 500,
                    ReadTimeout = 500,
                    ReceivedBytesThreshold = 1
                };

            brojElemenataDictionarya = dictionaryNaredbi.Count;
            // Kreiraj EventHandler
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);


            //Kreiraj i pokreni threadove
            //Thread consolewriterThread = new Thread(() => ObradiPodatke ("Thread started!"));
            //consolewriterThread.Start();


            //_serialPort.ReadyToSend += new Read
        }

        public void InitCommunication()
        {

            _serialPort.Open();

        }

        public void CloseCommunication()
        {

            _serialPort.Close();

        }

        public bool CommunicationHandshake()
        {
            #region Iniciraj handshake s bojlerom
            //Naredba indeksa 0
            //Ponovi Query 4 puta
            int sleepCounter = 0;

            // Inicijalno pričekaj na keypress
            var keyPressed = Console.ReadKey();
            Console.WriteLine("");

            if (_serialPort.IsOpen)
            {
                Console.WriteLine("Serial port is open.");

                for (int ponavljac = 0; ponavljac < 4; ponavljac++)
                {
                    if (_clearToSend)
                    {
                        _clearToSend = false;
                        //SendCommand(Niz, Niz.Count());
                        trenutniElementDictionarya = 1;
                        var b = dictionaryNaredbi.Single(k => k.Value.Indeks == trenutniElementDictionarya);
                        SendCommand(b.Value.SendCommand, b.Value.SendCommand.Count());
                        //Mala pauzica
                        Thread.Sleep(100);
                    }
                    else
                    {
                        //Ako nije clear to send onda malo čekaj
                        //Ako prođe 5sekundi izađi iz aplikacije
                        Thread.Sleep(10);
                        if (sleepCounter == 500)
                        {
                            return false;
                        }
                        sleepCounter++;
                    }
                }
            }
            else
            {
                Console.WriteLine("(CommunicationHandshake): port is not open!");
            }

            //ako je došao dovdje komunikacija je uspjela
            return true;

            #endregion
        }
        
        public void Communicator()
        {
            
            int sleepCounter = 0;

            if (_serialPort.IsOpen)
            {
                Console.WriteLine("Serial port is open.");
                //byte[] Niz = { 0x07, 0x02, 0x00, 0x00, 0x00, 0x04, 0xC4 }; //, 0x07, 0x02, 0x00, 0x00, 0x00, 0x04, 0xC4};

                //TODO: Ovdje bi trebalo implementirati okidanje eventa te handler koji uzima komande sa stacka
                //

                // Inicijalno pričekaj na keypress
                var keyPressed = Console.ReadKey();
                Console.WriteLine("");

                #region Petlja komunikacije

                //Vrti naredbe od indeksa 1 do indeksa n-1

                while (true)//(keyPressed.Key != ConsoleKey.A)
                {
                    
                    
                    if (_clearToSend)
                    {
                        _clearToSend = false;
                        //SendCommand(Niz, Niz.Count());
                        int trenutniElementCopy = trenutniElementDictionarya;
                        var b = dictionaryNaredbi.Single(k => k.Value.Indeks == trenutniElementDictionarya);
                        
                        SendCommand(b.Value.SendCommand,b.Value.SendCommand.Count());

                        if (trenutniElementDictionarya < brojElemenataDictionarya)
                            trenutniElementDictionarya++;
                        else
                            trenutniElementDictionarya = 0;

                        Thread.Sleep(100);
                    }
                    else
                    {
                        //Ako nije clear to send onda malo čekaj i izađi iz aplikacije (5s)
                        Thread.Sleep(10);
                        if (sleepCounter == 500)
                        {
                            return;
                        }
                        sleepCounter ++;
                    }
                }


                #endregion
            }
            else
            {
                Console.WriteLine("Error opening port.");
            }
        }


        private void SendCommand(byte[] txData, int numberOfChars)
        {
            _serialPort.Write(txData, 0, numberOfChars);
        }


        private void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            
            try
            {
                var rxData = _serialPort.ReadExisting();

                //Pošalji podatke na obradu u posebni thread
                var consolewriterThread = new Thread(() => ObradiPodatke(rxData, trenutniElementDictionarya));
                consolewriterThread.Start();
                //Preuzeo je sve podatke s Serial stacka, flipni flag na ready-to-send
                //_clearToSend = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Ovo bi trebao biti novi thread (u budućnosti database writer)
        private void ObradiPodatke(string rxData, int _trenutniElementDictionarya)
        {
            //primljeni podaci
            var array_RxData = rxData.ToArray();
            int array_RxDataCount = array_RxData.Count();

            // dohvati element dictionarya
            var elementDictionarya = dictionaryNaredbi.Single(k => k.Value.Indeks == trenutniElementDictionarya);

            // Poslani podaci:
            // LEGENDA - poslani podaci (7 bajtova)
            // 07       -   Duljina poslanih podatka u bajtovima
            // 00       -   ?
            // 00 00 00 -   Naredba bojleru
            // 01       -   Očekivan broj povratnih bajtova
            // 00       -   Checksum svih 6 bajtova


            switch (_trenutniElementDictionarya)
            {
                case 1: //Inicijalizacija
                    //provjeri da li je duljina polja očekivana
                    //provjeri da li je sadržaj očekivan
                    //if ((array_RxDataCount == elementDictionarya.Value.NumberOf_AllExpectedReturnBytes) && ())

                    break;

            }






















            //Dohvaća poslanu naredbu
            var singleKey = dictionaryNaredbi.Single(k => k.Value.Indeks == _trenutniElementDictionarya);
            int expectedUsefullReturnBytes = singleKey.Value.SendCommand [6];                                          //6. byte sadrži podatak o očekivanoj dužini korisnog dijela rxData


            // Primljeni podaci
            // LEGENDA - primljeni podaci
            //
            //
            // Kreni od drugog bajta pa ponovi expectedUsefullReturnBytes puta
            for (int position = 2; position < position + expectedUsefullReturnBytes; position ++)
            {

            }
            int Status = 0;
            int SensorStatus = 0;
            UInt16 Vrijednost1 = 0;
            UInt16 Vrijednost2 = 0;



            //Extraktaj korisne informacije zavisno o očekivanom broju povratnih karaktera
            if (array_RxDataCount == array_RxData[0])
            {
                if (expectedUsefullReturnBytes == 1) //nema errora
                {
                    //Status 
                    Status = array_RxData[3];
                }
                else if (expectedUsefullReturnBytes == 2) //nema errora
                {
                    //An analog value (2 bytes)
                    Vrijednost1 = (UInt16)(array_RxData[3] << 8 + array_RxData[4]);
                }
                else if (expectedUsefullReturnBytes == 3) //nema errora
                {
                    //An analog value (2 bytes) + a sensor status
                    Vrijednost1 = (UInt16)(array_RxData[3] + array_RxData[4]);
                    SensorStatus = array_RxData[5];
                }
                else if (expectedUsefullReturnBytes == 5) //nema errora
                {
                    //Two analog values ​​(2 x 2 bytes) + a sensor status
                    Vrijednost1 = (UInt16)(array_RxData[3] << 8 + array_RxData[4]);
                    Vrijednost2 = (UInt16)(array_RxData[5] << 8 + array_RxData[6]);
                    SensorStatus = array_RxData[5];
                }
            }
            else
            {
                Console.WriteLine("Došlo je do nekakve greške");
                Console.WriteLine(array_RxData[0].ToString());
                Console.WriteLine(array_RxDataCount.ToString());

                Console.Write("Full response: ");
                foreach (byte b in Encoding.ASCII.GetBytes(array_RxData))
                {
                    Console.Write(b + "; ");
                }
                Console.WriteLine();
            }



            Console.Write("Response: ");
            foreach (byte b in Encoding.ASCII.GetBytes(array_RxData))
            {
                Console.Write(b + "; ");
            }
            Console.WriteLine(string.Format("Status: {0}; Vrijednost1: {1}; Vrijednost2: {2}.", Status.ToString(), Vrijednost1.ToString(), Vrijednost2.ToString()));
            Console.WriteLine();
            //Spremi u bazu
            var dc = new HMonitorData(BoillerSerialComm.Properties.Settings.Default.HistoricalDBConnection);
            

        }

    }
    
}
