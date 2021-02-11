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
        private Thickness OMINO_PIANO_1 = new Thickness(128,384,0,106);
        private Thickness OMINO_PIANO_2 = new Thickness(128,283,0,201);
        private Thickness OMINO_PIANO_3 = new Thickness(128,178,0,306);
        private Thickness OMINO_PIANO_4 = new Thickness(128,78,0,406);
        private Thickness OMINO_PIANO_5 = new Thickness(128,0,0,484);

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
            semaforo.Release(2);

            threadSpawnOmini.Start();
        }

        private void MuoviOmini()
        {
            try
            {
                Thread.Sleep(100);
                while (whileState)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        for (int i = 0; i < ominiImage.Count; i++)
                        {
                            //MUOVI
                            int forzaUscita = 0;

                            int p = 0;

                            while (true)
                            {
                                //Controllo forza uscita
                                if (forzaUscita >= 20)
                                    break;

                                semaforo.WaitOne();

                                p = g.Children.IndexOf(ominiImage[i].Immagine);

                                //Controllo uscita
                                if (ominiImage[i].Piano == 1 && ((g.Children[p] as Image).Margin.Left <= OMINO_PIANO_1.Left)) //Omino al primo piano
                                    break;

                                if (ominiImage[i].Piano == 2 && ((g.Children[p] as Image).Margin.Left <= OMINO_PIANO_2.Left)) //Omino al secondo piano
                                    break;

                                if (ominiImage[i].Piano == 3 && ((g.Children[p] as Image).Margin.Left <= OMINO_PIANO_3.Left)) //Omino al terzo piano
                                    break;

                                if (ominiImage[i].Piano == 4 && ((g.Children[p] as Image).Margin.Left <= OMINO_PIANO_4.Left)) //Omino al quarto piano
                                    break;

                                if (ominiImage[i].Piano == 5 && ((g.Children[p] as Image).Margin.Left <= OMINO_PIANO_5.Left)) //Omino al quinto piano
                                    break;

                                //Sposta
                                double newLeft = (g.Children[p] as Image).Margin.Left - 10;

                                (g.Children[p] as Image).Margin = new Thickness(newLeft, (g.Children[p] as Image).Margin.Top, (g.Children[p] as Image).Margin.Right, (g.Children[p] as Image).Margin.Bottom);
                                Thread.Sleep(100);

                                semaforo.Release();

                                forzaUscita++;

                                Debug.WriteLine(ominiImage[i].Immagine.Margin);
                            }

                            //Aggiorna
                            ominiImage[i].Immagine = (g.Children[p] as Image);
                        }
                    }));
                }
            } catch(Exception ex)
            {
                Debug.WriteLine("AA");
            }
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
