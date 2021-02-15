using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcorrenzaAscensore.Classes
{
    public class AscensorePienoException : Exception
    {
        public AscensorePienoException() : base()
        {

        }
    }

    public class Ascensore
    {
        public const int NUMERO_MASSIMO_CLIENTI = 1;

        private Queue<Persona> ClientiAscensore
        {
            get;
            set;
        }

        public uint CurrentPiano
        {
            get; set;
        }

        public void AggiungiPersona(Persona p)
        {/*
            if (GetNumeroClienti() == NUMERO_MASSIMO_CLIENTI)
                throw new AscensorePienoException();*/

            ClientiAscensore.Enqueue(p);
        }

        public Persona PekkaNuovaRichiesta()
        {
                return ClientiAscensore.Peek();
        }

        public Persona ProcessaProssimaRichiesta()
        {
            return ClientiAscensore.Dequeue();
        }

        public int GetNumeroClienti()
        {
            return ClientiAscensore.Count;
        }

        public Ascensore()
        {
            ClientiAscensore = new Queue<Persona>();
        }
    }
}
