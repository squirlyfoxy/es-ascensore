using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcorrenzaAscensore.Classes
{
    public class Ascensore
    {
        public static int NUMERO_MASSIMO_CLIENTI = 1;

        private Queue<Persona> ClientiAscensore
        {
            get;
            set;
        }

        public void AggiungiPersona(Persona p)
        {
            if (GetNumeroClienti() == NUMERO_MASSIMO_CLIENTI)
                throw new Exception("Raggiunta la capacità masisma dell'ascensore");

            ClientiAscensore.Enqueue(p);
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
