using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoillerSerialComm
{
    class Naredbe           //TODO: Rename u Naredba
    {
        public int Indeks { get; set; }
        public byte[] SendCommand { get; set; }
        public byte[] ReceivedCommand { get; set; }
        public int NumberOf_UsefullExpectedReturnDataBytes { get; set; }
        public int NumberOf_AllExpectedReturnBytes { get; set; }
    }

}
