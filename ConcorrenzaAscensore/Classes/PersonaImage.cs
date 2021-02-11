using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace ConcorrenzaAscensore.Classes
{
    public class PersonaImage
    {
        public int Piano
        {
            get;
            private set;
        }

        public Image Immagine
        {
            get;
            set;
        }

        public PersonaImage(int piano, Image im)
        {
            Piano = piano;
            Immagine = im;
        }
    }
}
