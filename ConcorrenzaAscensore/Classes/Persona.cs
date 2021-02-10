using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcorrenzaAscensore.Classes
{
    public class Persona
    {
        public int StartPiano
        {
            get;
            set;
        }

        public PrenotazioneAscensore PrenotazioneAscensore
        {
            get;
            private set;
        }

        public void Prenota(int p)
        {
            if (p == StartPiano)
                throw new Exception("Non si può prenotare una corsa per lo stesso piano in cui ci si trova");

            PrenotazioneAscensore = new PrenotazioneAscensore(p);
        }

        public Persona(int startPiano)
        {
            StartPiano = startPiano;
        }
    }
}
