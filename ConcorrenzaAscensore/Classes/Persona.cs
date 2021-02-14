using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace ConcorrenzaAscensore.Classes
{
    public class Persona : IEquatable<PersonaImage>
    {
        public int StartPiano
        {
            get;
            set;
        }

        public Image Immagine
        {
            get;
            set;
        }

        public PrenotazioneAscensore PrenotazioneAscensore
        {
            get;
            set;
        }

        public void Prenota(int p)
        {
            if (p == StartPiano)
                throw new Exception("Non si può prenotare una corsa per lo stesso piano in cui ci si trova");

            PrenotazioneAscensore = new PrenotazioneAscensore(p);
        }

        public bool Equals(PersonaImage other)
        {
            if (other.Immagine == this.Immagine && other.Piano == this.StartPiano)
                return true;

            return false;
        }

        public Persona(int startPiano)
        {
            StartPiano = startPiano;
        }
    }
}
