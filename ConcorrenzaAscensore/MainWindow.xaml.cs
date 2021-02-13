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
        private Thickness OMINO_PIANO_1 = new Thickness(128,384,566,106);
        private Thickness OMINO_PIANO_2 = new Thickness(128,283,566,201);
        private Thickness OMINO_PIANO_3 = new Thickness(128,178,566,306);
        private Thickness OMINO_PIANO_4 = new Thickness(128,78,566,406);
        private Thickness OMINO_PIANO_5 = new Thickness(128,0,566,484);

        //Posizione di spawn
        private Thickness OMINO_SPAWN_PIANO_1 = new Thickness(694, 384, 0, 106);
        private Thickness OMINO_SPAWN_PIANO_2 = new Thickness(694, 283, 0, 201);
        private Thickness OMINO_SPAWN_PIANO_3 = new Thickness(694, 178, 0, 306);
        private Thickness OMINO_SPAWN_PIANO_4 = new Thickness(694, 78, 0, 406);
        private Thickness OMINO_SPAWN_PIANO_5 = new Thickness(694, 0, 0, 484);

        public object LockObject;

        public Ascensore Ascensore;

        //Lista Omini
        private List<PersonaImage> ominiImage;

        //Ciclo
        bool whileState = true;

        private Thread threadSpawnOmini;
        private Thread threadMuoviOmini;
        private Thread threadAscensore;

        private Semaphore semaforo;

        public MainWindow()
        {
            LockObject = new object();
            ominiImage = new List<PersonaImage>();

            InitializeComponent();

            Ascensore = new Ascensore();
            Ascensore.CurrentPiano = 0;

            threadSpawnOmini = new Thread(new ThreadStart(ProcessaSpawnOmini));
            threadMuoviOmini = new Thread(new ThreadStart(MuoviOmini));
            threadAscensore = new Thread(new ThreadStart(ProcessaAscensore));

            semaforo = new Semaphore(0, 2);
            semaforo.Release();

            threadSpawnOmini.Start();
        }

        //FIX: UNO DEI DUE THREAD VA IN CONFLITTO CON QUALCOSA E DI CONSEGUENZA, DOPO AVER FATTO SPAWNARE UNO/DUE OMINI, I THREAD "IMPLODONO"

        private void MuoviOmini()
        {
            try
            {
                //Thread.Sleep(100);
                while (whileState)
                {
                    for (int i = 0; i < ominiImage.Count; i++)
                    {
                        //Controllo se devo arrivare ancora a destinazione
                        if (ominiImage[i].Stato == PersonaImage.Stati.Non_Ancora_Arrivato_Ascensore)
                        {
                            //MUOVI
                            int forzaUscita = 0;

                            int p = 0;

                            while (true)
                            {
                                Thread.Sleep(100);

                                //Controllo forza uscita
                                if (forzaUscita >= 20)
                                    break;

                                semaforo.WaitOne();

                                Thickness margine = new Thickness();

                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    p = g.Children.IndexOf(ominiImage[i].Immagine);
                                    margine = (g.Children[p] as Image).Margin;
                                }));

                                //Controllo uscita
                                if (ominiImage[i].Piano == 1 && (margine.Left <= OMINO_PIANO_1.Left && margine.Right >= OMINO_PIANO_1.Right)) //Omino al primo piano
                                {
                                    ominiImage[i].Stato = PersonaImage.Stati.Arrivato;
                                    break;
                                }

                                if (ominiImage[i].Piano == 2 && (margine.Left <= OMINO_PIANO_2.Left && margine.Right >= OMINO_PIANO_2.Right)) //Omino al secondo piano
                                {
                                    ominiImage[i].Stato = PersonaImage.Stati.Arrivato;
                                    break;
                                }

                                if (ominiImage[i].Piano == 3 && (margine.Left <= OMINO_PIANO_3.Left && margine.Right >= OMINO_PIANO_3.Right)) //Omino al terzo piano
                                {
                                    ominiImage[i].Stato = PersonaImage.Stati.Arrivato;
                                    break;
                                }

                                if (ominiImage[i].Piano == 4 && (margine.Left <= OMINO_PIANO_4.Left && margine.Right >= OMINO_PIANO_4.Right)) //Omino al quarto piano
                                {
                                    ominiImage[i].Stato = PersonaImage.Stati.Arrivato;
                                    break;
                                }

                                if (ominiImage[i].Piano == 5 && (margine.Left <= OMINO_PIANO_5.Left && margine.Right >= OMINO_PIANO_5.Right)) //Omino al quinto piano
                                {
                                    ominiImage[i].Stato = PersonaImage.Stati.Arrivato;
                                    break;
                                }

                                //Sposta
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    double newLeft = (g.Children[p] as Image).Margin.Left - 10;
                                    double newRight = (g.Children[p] as Image).Margin.Right + 10;

                                    (g.Children[p] as Image).Margin = new Thickness(newLeft, (g.Children[p] as Image).Margin.Top, newRight, (g.Children[p] as Image).Margin.Bottom);
                                }));

                                semaforo.Release();

                                forzaUscita++;

                                //Debug.WriteLine(ominiImage[i].Immagine.Margin);
                            }

                            //Aggiorna
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                ominiImage[i].Immagine = (g.Children[p] as Image);
                            }));
                        }
                    }
                }
            } catch(Exception ex)
            {
                Debug.WriteLine("Errore: " + ex);
            }

            Debug.WriteLine("Uscito");
        }

        private void ProcessaSpawnOmini()
        {
            ///
            /// Un'omino viene fatto spawnare ogni 5 secondi
            ///

            int c = 0;

            while (whileState)
            {
                Thread.Sleep(5000); //Ogni x secondi

                //Spawna
                Random rnd = new Random();
                int piano = rnd.Next(0, 5);

                this.Dispatcher.Invoke(new Action(() =>
                {
                    Debug.WriteLine("Spawned");

                    //Oggetti
                    Image toAdd = new Image();
                    BitmapImage src = new BitmapImage();

                    //Risorsa immagine
                    src.BeginInit();
                    src.UriSource = new Uri(@"Src/omino.png", UriKind.Relative);
                    src.EndInit();

                    toAdd.Source = src;

                    //Dimensione immagine
                    toAdd.Width = 100;
                    toAdd.Height = 100;
                    toAdd.Stretch = Stretch.Fill;

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

                    semaforo.WaitOne();

                    g.Children.Add(toAdd);

                    semaforo.Release();

                    // TENERE TRACCIA DELLA SITUAZIONE CORRENTE DELL'OMINO
                    ominiImage.Add(new PersonaImage(piano + 1, toAdd));
                }));

                //Se è la prima volta che siamo in esecuzione
                if (c == 0)
                {
                    threadMuoviOmini.Start();
                }

                c++;
            }
        }

        private void ProcessaAscensore()
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                whileState = false;

                threadSpawnOmini.Abort();
                threadMuoviOmini.Abort();
                threadAscensore.Abort();

                threadSpawnOmini.Join();
                threadMuoviOmini.Join();
                threadAscensore.Join();
            } catch(Exception ex)
            {
                Debug.WriteLine("Errore: " + ex);
            }
        }
    }
}
