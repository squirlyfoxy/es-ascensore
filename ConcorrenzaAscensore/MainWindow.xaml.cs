using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

//Thread
using System.Threading;

//Namespaces interni al progetto
using ConcorrenzaAscensore.Classes;

namespace ConcorrenzaAscensore
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Posizioni da raggiungere per l'ascensore per ogni piano
        private Thickness ASCENSORE_PIANO_0 = new Thickness(0, 484, 0, 0);
        private Thickness ASCENSORE_PIANO_1 = new Thickness(0, 416, 0, 0);
        private Thickness ASCENSORE_PIANO_2 = new Thickness(0, 318, 0, 0);
        private Thickness ASCENSORE_PIANO_3 = new Thickness(0, 220, 0, 0);
        private Thickness ASCENSORE_PIANO_4 = new Thickness(0, 216, 0, 0);
        private Thickness ASCENSORE_PIANO_5 = new Thickness(0, 10, 0, 0);

        //Posizione da raggiungere per gli omini per ogni piano
        private Thickness OMINO_PIANO_1 = new Thickness(128,384,0,0);
        private Thickness OMINO_PIANO_2 = new Thickness(128,283,0,0);
        private Thickness OMINO_PIANO_3 = new Thickness(128,178,0,0);
        private Thickness OMINO_PIANO_4 = new Thickness(128,78,0,0);
        private Thickness OMINO_PIANO_5 = new Thickness(128,0,0,0);

        //Posizione di spawn
        private Thickness OMINO_SPAWN_PIANO_1 = new Thickness(694, 384, 0, 0);
        private Thickness OMINO_SPAWN_PIANO_2 = new Thickness(694, 283, 0, 0);
        private Thickness OMINO_SPAWN_PIANO_3 = new Thickness(694, 178, 0, 0);
        private Thickness OMINO_SPAWN_PIANO_4 = new Thickness(694, 78, 0, 0);
        private Thickness OMINO_SPAWN_PIANO_5 = new Thickness(694, 0, 0, 0);

        public object LockObject;

        public Ascensore Ascensore;

        private List<Image> ominiImage;

        private Thread threadOmini;
        private Thread threadAscensore;

        public MainWindow()
        {
            LockObject = new object();
            ominiImage = new List<Image>();

            InitializeComponent();

            Ascensore = new Ascensore();
            Ascensore.CurrentPiano = 0;

            threadOmini = new Thread(new ThreadStart(ProcessaOmini));
            //threadOmini.SetApartmentState(ApartmentState.STA);
            threadAscensore = new Thread(new ThreadStart(ProcessaAscensore));

            threadOmini.Start();
        }

        private void ProcessaOmini()
        {
            ///
            /// Un'omino viene fatto spawnare ogni 5 secondi
            ///

            DateTime start = DateTime.Now;

            int sec = 0;

            while(true)
            {
                TimeSpan tempoPassato = DateTime.Now - start;

                //if(tempoPassato.Seconds % 5 > sec)
                {
                    //Spawna
                    Random rnd = new Random();
                    int piano = rnd.Next(0, 5);

                    lock (LockObject)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {

                            Image toAdd = new Image();
                            BitmapImage src = new BitmapImage();

                            src.BeginInit();
                            src.UriSource = new Uri(@"Src/omino.png", UriKind.Relative);
                            src.EndInit();

                            toAdd.Source = src;

                            //Posizione del piano
                            switch (piano)
                            {
                                case 0:
                                    //Piano 1
                                    toAdd.Margin = OMINO_SPAWN_PIANO_1;
                                    break;
                                case 1:
                                    //Piano 2
                                    toAdd.Margin = OMINO_SPAWN_PIANO_2;
                                    break;
                                case 2:
                                    //Piano 3
                                    toAdd.Margin = OMINO_SPAWN_PIANO_3;
                                    break;
                                case 3:
                                    //Piano 4
                                    toAdd.Margin = OMINO_SPAWN_PIANO_4;
                                    break;
                                case 4:
                                    //Piano 5
                                    toAdd.Margin = OMINO_SPAWN_PIANO_5;
                                    break;
                            }

                            ominiImage.Add(toAdd);
                        }));
                    }
                    sec++;
                }
            }
        }

        private void ProcessaAscensore()
        {

        }
    }
}
