using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcorrenzaAscensore.Classes
{
    public class PrenotazioneAscensore
    {
        public int Piano
        {
            get;
            private set;
        }

        public PrenotazioneAscensore(int piano)
        {
            Piano = piano;
        }
    }
}
