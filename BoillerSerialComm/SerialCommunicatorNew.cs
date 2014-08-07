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
    internal class SerialCommunicatorNew
    {
        #region Inicijalizacija podataka

        private SerialPort _serialPort;
        private static bool _clearToSend = true;
        private int brojElemenataDictionarya;
        private int trenutniElementDictionarya = 1;

        private static AutoResetEvent waitForRxProcessingEnd_Event;

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


        public SerialCommunicatorNew(string serialPortName)
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
                                  ReceivedBytesThreshold = 1,
                                  //Encoding = Encoding.Default  //System.Text.Encoding.GetEncoding(1252)
                              };

            //Inicijaliziraj varijable i event handlere
            brojElemenataDictionarya = dictionaryNaredbi.Count;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
        }

        #endregion

        public void InitCommunication()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }

            //TODO: Vidjeti što napraviti ako je port već otvoren, da li ga zatvoriti 
            //      pa ponovno otvoriti ili napraviti nešto drugo
        }

        public void CloseCommunication()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public bool Communicator()
        {
            //Naredba indeksa 0
            //Ponovi Query 4 puta
            int sleepCounter = 0;
            var waitForReceive = new EventWaitHandle(false, EventResetMode.AutoReset);
            waitForRxProcessingEnd_Event = new AutoResetEvent(false);

            // Inicijalno pričekaj na keypress
            var keyPressed = Console.ReadKey();
            Console.WriteLine("");

            //petlja koja se uzvršava sve dok nije setiran flag timeouta...
            //dok je port otvoren
            while (_serialPort.IsOpen == true)
            {
                //Dohvati slijedeću naredbu iz dictionary-a i pošalji ju Serijskim portom
                var b = dictionaryNaredbi.Single(k => k.Value.Indeks == trenutniElementDictionarya);
                SendCommand(b.Value.SendCommand, b.Value.SendCommand.Count());

                #region Za potrebe testiranja
                //Ispiši
                Console.WriteLine();
                Console.WriteLine("Send: ");
                foreach (byte c in b.Value.SendCommand)
                {
                    Console.Write(c + "; ");
                }
                #endregion

                //
                //
                //Pričekaj da se dovrši procesiranje Rx podataka
                //TODO:Ugraditi timeout
                waitForRxProcessingEnd_Event.WaitOne();
                //
                //
            
                //Uvećaj brojač ako je to moguće, ili resetiraj na početnu vrijednost
                if (trenutniElementDictionarya < brojElemenataDictionarya)
                {
                    trenutniElementDictionarya++;
                }
                else
                {
                    trenutniElementDictionarya = 1;     //Početna vrijednost
                }
            }

            return true;
        }

        private void SendCommand(byte[] txData, int numberOfChars)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Write(txData, 0, numberOfChars);
            }
            else
            {
                Console.WriteLine(string.Format("Serijski port '{0}' nije otvoren.", _serialPort.PortName));
                //TODO: Razmisliti što dalje s aplikacijom u ovom trenutku...
            }

        }

        private void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                //Inicijaliziraj buffer
                int numberOfBytes = _serialPort.BytesToRead;
                int[] array1_rxData = new int[numberOfBytes];

                for (int x = 0; x <= numberOfBytes - 1; x++ )
                {
                    array1_rxData[x] = _serialPort.ReadByte();
                }




                //byte[] array_rxData = _serialPort.ReadByte();          //new byte[_serialPort.ReadBufferSize];


                //int jedan = array_rxData[0];
                //int dva = array_rxData[1];
                //int tri = array_rxData[2];
                //int cetiri = array_rxData[3];
                //int pet = array_rxData[4];
                //int sest = array_rxData[5];
                //int sedam = array_rxData[6];

                //int sve = jedan + dva + tri + cetiri + pet + sest;
                //Učitaj bajtove i saznaj postoji li greška
                //int bytesRead = _serialPort.Read(array_rxData, 0, array_rxData.Length);

                //var rxData = _serialPort.ReadExisting();
                //var array_RxData = rxData.ToArray();

                //Pošalji podatke na obradu u posebni thread
                //var consolewriterThread = new Thread(() => ObradiPodatke(rxData, trenutniElementDictionarya));
                //consolewriterThread.Start();
            //ObradiPodatke(rxData, trenutniElementDictionarya);

                #region Ispis za potrebe testiranja
                Console.WriteLine("Received: ");
                foreach (byte b in array1_rxData)
                {
                    Console.Write(b.ToString() + "; ");
                }
                #endregion

                //Preuzeo je sve podatke s Serial stacka, flipni flag na ready-to-send
                //_clearToSend = true;

                waitForRxProcessingEnd_Event.Set();
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
            // var elementDictionarya = dictionaryNaredbi.Single(k => k.Value.Indeks == trenutniElementDictionarya);


            //Dohvaća poslanu naredbu
            var singleKey = dictionaryNaredbi.Single(k => k.Value.Indeks == _trenutniElementDictionarya);
            int expectedUsefullReturnBytes = singleKey.Value.SendCommand[6];                                         //6. byte sadrži podatak o očekivanoj dužini korisnog dijela rxData
            
            int Status = 0;
            int SensorStatus = 0;
            UInt16 Vrijednost1 = 0;
            UInt16 Vrijednost2 = 0;            

            // Poslani podaci:
            // LEGENDA - poslani podaci (7 bajtova)
            // 07       -   Duljina poslanih podatka u bajtovima
            // 00       -   ?
            // 00 00 00 -   Naredba bojleru
            // 01       -   Očekivan broj povratnih bajtova
            // 00       -   Checksum svih 6 bajtova

            int lenght = array_RxData[0];

            if (array_RxDataCount == array_RxData[0])       //provjeri da li je return array kompletan
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



            
            ////Obradi zapisivanje u bazupodataka
            //switch (_trenutniElementDictionarya)
            //{
            //    case 1:
            //        //Testiranje komunikacije
            //        if (array_RxDataCount != singleKey.Value.NumberOf_AllExpectedReturnBytes)
            //        {
            //            Vrijednost1 = (UInt16)(array_RxData[3] << 8 + array_RxData[4]);
            //        }
            //        break;

            //    case 2: //Inicijalizacija
            //        //provjeri da li je duljina polja očekivana
            //        //provjeri da li je sadržaj očekivan
            //        //if ((array_RxDataCount == elementDictionarya.Value.NumberOf_AllExpectedReturnBytes) && ())




            //        break;

            //    case 3:
            //        break;

            //    default:

            //        break;
            //}
        }
    }
}




////Dohvaća poslanu naredbu
//            //var singleKey = dictionaryNaredbi.Single(k => k.Value.Indeks == _trenutniElementDictionarya);
//            //int expectedUsefullReturnBytes = singleKey.Value.SendCommand[6];                                          //6. byte sadrži podatak o očekivanoj dužini korisnog dijela rxData


//            // Primljeni podaci
//            // LEGENDA - primljeni podaci
//            //
//            //
//            // Kreni od drugog bajta pa ponovi expectedUsefullReturnBytes puta
//            for (int position = 2; position < position + expectedUsefullReturnBytes; position++)
//            {

//            }
//            int Status = 0;
//            int SensorStatus = 0;
//            UInt16 Vrijednost1 = 0;
//            UInt16 Vrijednost2 = 0;



//            //Extraktaj korisne informacije zavisno o očekivanom broju povratnih karaktera
//            if (array_RxDataCount == array_RxData[0])
//            {
//                if (expectedUsefullReturnBytes == 1) //nema errora
//                {
//                    //Status 
//                    Status = array_RxData[3];
//                }
//                else if (expectedUsefullReturnBytes == 2) //nema errora
//                {
//                    //An analog value (2 bytes)
//                    Vrijednost1 = (UInt16)(array_RxData[3] << 8 + array_RxData[4]);
//                }
//                else if (expectedUsefullReturnBytes == 3) //nema errora
//                {
//                    //An analog value (2 bytes) + a sensor status
//                    Vrijednost1 = (UInt16)(array_RxData[3] + array_RxData[4]);
//                    SensorStatus = array_RxData[5];
//                }
//                else if (expectedUsefullReturnBytes == 5) //nema errora
//                {
//                    //Two analog values ​​(2 x 2 bytes) + a sensor status
//                    Vrijednost1 = (UInt16)(array_RxData[3] << 8 + array_RxData[4]);
//                    Vrijednost2 = (UInt16)(array_RxData[5] << 8 + array_RxData[6]);
//                    SensorStatus = array_RxData[5];
//                }
//            }
//            else
//            {
//                Console.WriteLine("Došlo je do nekakve greške");
//                Console.WriteLine(array_RxData[0].ToString());
//                Console.WriteLine(array_RxDataCount.ToString());

//                Console.Write("Full response: ");
//                foreach (byte b in Encoding.ASCII.GetBytes(array_RxData))
//                {
//                    Console.Write(b + "; ");
//                }
//                Console.WriteLine();
//            }



//            Console.Write("Response: ");
//            foreach (byte b in Encoding.ASCII.GetBytes(array_RxData))
//            {
//                Console.Write(b + "; ");
//            }
//            Console.WriteLine(string.Format("Status: {0}; Vrijednost1: {1}; Vrijednost2: {2}.", Status.ToString(), Vrijednost1.ToString(), Vrijednost2.ToString()));
//            Console.WriteLine();
//            //Spremi u bazu
//            var dc = new HMonitorData(BoillerSerialComm.Properties.Settings.Default.HistoricalDBConnection);
