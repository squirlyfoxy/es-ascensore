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

//Per il debug
using System.Diagnostics;

//Thread
using System.Threading;

//Namespaces interni al progetto
using ConcorrenzaAscensore.Classes;

namespace ConcorrenzaAscensore
{
    ///
    /// Author: Leonardo Baldazzi (@sqluirlyfoxy)
    ///

    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Posizioni da raggiungere per l'ascensore per ogni piano

        //private Thickness ASCENSORE_PIANO_0 = new Thickness(0, 484, 0, 0);
        private Thickness ASCENSORE_PIANO_1 = new Thickness(0, 378, 0, 105);
        private Thickness ASCENSORE_PIANO_2 = new Thickness(0, 278, 0, 205);
        private Thickness ASCENSORE_PIANO_3 = new Thickness(0, 177, 0, 306);
        private Thickness ASCENSORE_PIANO_4 = new Thickness(0, 73, 0, 410);
        private Thickness ASCENSORE_PIANO_5 = new Thickness(0, -27, 0, 510);

        //Posizione da raggiungere per gli omini per ogni piano
        private Thickness OMINO_PIANO_1 = new Thickness(128, 378, 566, 96);
        private Thickness OMINO_PIANO_2 = new Thickness(128, 278, 566, 196);
        private Thickness OMINO_PIANO_3 = new Thickness(128, 178, 566, 296);
        private Thickness OMINO_PIANO_4 = new Thickness(128, 78, 566, 396);
        private Thickness OMINO_PIANO_5 = new Thickness(128, -27, 566, 496);

        //Posizione di spawn per gli omini
        private Thickness OMINO_SPAWN_PIANO_1 = new Thickness(694, 378, 0, 96);
        private Thickness OMINO_SPAWN_PIANO_2 = new Thickness(694, 278, 0, 196);
        private Thickness OMINO_SPAWN_PIANO_3 = new Thickness(694, 178, 0, 296);
        private Thickness OMINO_SPAWN_PIANO_4 = new Thickness(694, 78, 0, 396);
        private Thickness OMINO_SPAWN_PIANO_5 = new Thickness(694, -27, 0, 496);

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

        //FIX (FIXATO IL 14/02/2021): UNO DEI DUE THREAD VA IN CONFLITTO CON QUALCOSA E DI CONSEGUENZA, DOPO AVER FATTO SPAWNARE UNO/DUE OMINI, I THREAD "IMPLODONO", AVVIENE QUANDO IL RIMO OMINO ARRIVA A DESTINAZIONE

        private void MuoviOmini()
        {
            try
            {
                //Thread.Sleep(100);
                while (whileState)
                {
                    //Thread.Sleep(500);
                    for (int i = 0; i < ominiImage.Count; i++)
                    {
                        if (ominiImage[i] != null)
                        {
                            if (ominiImage[i].Stato == PersonaImage.Stati.Arrivato && i < ominiImage.Count)
                            {
                                ///
                                /// Una volta arrivto a destinazione, creo un nuovo oggetto persona, lo aggiungo alla queue dell'ascensore e ne cambio lo stato 
                                ///

                                semaforo.WaitOne();

                                try
                                {
                                    Persona persona = new Persona(ominiImage[i].Piano);
                                    persona.Immagine = ominiImage[i].Immagine;

                                    //Crea la prenotazione per l'ascensore
                                    Random rnd = new Random();
                                    int piano;

                                    do
                                    {
                                        piano = rnd.Next(1, 6);
                                    } while (piano == persona.StartPiano); //Controllo che non sia lo stesso piano in cui mi trovo adesso

                                    persona.Prenota(piano);

                                    Ascensore.AggiungiPersona(persona); //Aggiungo la persona

                                    ominiImage[i].Stato = PersonaImage.Stati.Richiesta;

                                    Debug.WriteLine("Omino arrivato all'ascensore, prenotazione per il piano: " + piano);

                                    ///
                                    /// Controlla lo stato del thread dell'ascensore, se non è ancora partito fallo partire
                                    ///

                                    if (threadAscensore.IsAlive == false)
                                        threadAscensore.Start();
                                }
                                catch (AscensorePienoException)
                                {
                                    /* SE CADO QUI VUOL DIRE CHE L'ASCENSORE è PIENO E QUINDI NON POSSO RICHIEDERLO FINO A QUANDO NON SI SVUOTA */
                                    Debug.WriteLine("Ascensore pieno");
                                }

                                semaforo.Release();
                            }
                            else
                            {
                                //Controllo se devo arrivare ancora a destinazione
                                if (ominiImage[i].Stato == PersonaImage.Stati.Non_Ancora_Arrivato_Ascensore && i < ominiImage.Count)
                                {
                                    //MUOVI
                                    int forzaUscita = 0; //Serve per dare una senzazione di "stacco" a chi osserva la simulazione

                                    int p = 0;

                                    while (true)
                                    {
                                        Thread.Sleep(100);

                                        //Controllo forza uscita
                                        if (forzaUscita >= 40)
                                            break;

                                        Thickness margine = new Thickness();
                                        int n = 0;
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            n = g.Children.Count;
                                        }));

                                        if (i < ominiImage.Count && p < n)
                                        {
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

                                            semaforo.WaitOne();

                                            //Thread.Sleep(20);

                                            //Sposta
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                lock (LockObject)
                                                {
                                                    double newLeft = (g.Children[p] as Image).Margin.Left - 5;
                                                    double newRight = (g.Children[p] as Image).Margin.Right + 5;

                                                    (g.Children[p] as Image).Margin = new Thickness(newLeft, (g.Children[p] as Image).Margin.Top, newRight, (g.Children[p] as Image).Margin.Bottom);
                                                }
                                            }));

                                            semaforo.Release();
                                        }

                                        forzaUscita++;
                                    }

                                    Thread.Sleep(50);

                                    //Aggiorna
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        if (i < ominiImage.Count && p < g.Children.Count)
                                        {
                                            ominiImage[i].Immagine = (g.Children[p] as Image);
                                        }
                                    }));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Errore: " + ex);
                FermaTutto();
            }
        }

        private void ProcessaSpawnOmini()
        {
            ///
            /// Un'omino viene fatto spawnare ogni 5 secondi
            ///

            try
            {
                while (whileState)
                {
                    Thread.Sleep(20000); //Ogni x secondi

                    //Spawna
                    Random rnd = new Random();
                    int piano = rnd.Next(0, 5);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Debug.WriteLine("Nuovo omino");

                        //Oggetti
                        Image toAdd = new Image();
                        BitmapImage src = new BitmapImage();

                        //Risorsa immagine
                        src.BeginInit();
                        src.UriSource = new Uri(@"Src/nuovo_omino.png", UriKind.Relative);
                        src.EndInit();

                        toAdd.Source = src;

                        //Dimensione immagine
                        toAdd.Width = 100;
                        toAdd.Height = 100;
                        toAdd.Stretch = Stretch.Uniform;

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
                    if (!threadMuoviOmini.IsAlive)
                    {
                        threadMuoviOmini.Start();
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine("Errore: " + ex);
                FermaTutto();
            }
        }

        const int VELOCITA_MOVIMENTO_ASCENSORE = 1;

        private void ProcessaAscensore()
        {
            //IL SEGUENTE THREAD PROCESSA OGNI SINGOLA PRENOTAZIONE PER L'ASCENSORE; E LO FA MUOVERE

            try
            {
                while (whileState)
                {
                    //Thread.Sleep(200);
                    if (Ascensore.GetNumeroClienti() > 0)
                    {
                        Persona p = Ascensore.PekkaNuovaRichiesta();
                        int posLista = GetPosizionePersonaAscensoreDentroLista(p);
                        PersonaImage pi = ominiImage[posLista];

                        if (pi.Stato == PersonaImage.Stati.Richiesta)
                        {
                            //Muovi ascensore alla posizione desiderata
                            while (true)
                            {
                                Thread.Sleep(150);

                                semaforo.WaitOne();

                                //Piano corrente
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (Ascensore.CurrentPiano != 1 && (imgAscensore.Margin == ASCENSORE_PIANO_1))
                                        Ascensore.CurrentPiano = 1;
                                    else if (Ascensore.CurrentPiano != 2 && (imgAscensore.Margin == ASCENSORE_PIANO_2))
                                        Ascensore.CurrentPiano = 2;
                                    else if (Ascensore.CurrentPiano != 3 && (imgAscensore.Margin == ASCENSORE_PIANO_3))
                                        Ascensore.CurrentPiano = 3;
                                    else if (Ascensore.CurrentPiano != 4 && (imgAscensore.Margin == ASCENSORE_PIANO_4))
                                        Ascensore.CurrentPiano = 4;
                                    else if (Ascensore.CurrentPiano != 5 && (imgAscensore.Margin == ASCENSORE_PIANO_5))
                                        Ascensore.CurrentPiano = 5;
                                }));

                                //Controllo uscite
                                if (Ascensore.CurrentPiano == p.StartPiano)
                                {
                                    pi.Stato = PersonaImage.Stati.In_Viaggio;
                                    ominiImage[posLista] = pi;

                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        g.Children.Remove(ominiImage[posLista].Immagine); //Rimuovi dalla grafica

                                        BitmapImage sr = new BitmapImage();
                                        sr.BeginInit();
                                        sr.UriSource = new Uri(@"Src/ascensore-riempito.png", UriKind.Relative);
                                        sr.EndInit();

                                        imgAscensore.Source = sr;
                                    }));

                                    semaforo.Release();

                                    Debug.WriteLine("In viaggio");

                                    break;
                                }

                                //Muovo ascensore
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    Thickness marginAscensoreVecchio = imgAscensore.Margin;

                                    if (p.StartPiano > Ascensore.CurrentPiano)
                                    {
                                        marginAscensoreVecchio.Top -= VELOCITA_MOVIMENTO_ASCENSORE;
                                        marginAscensoreVecchio.Bottom += VELOCITA_MOVIMENTO_ASCENSORE;
                                    }
                                    else
                                    {
                                        marginAscensoreVecchio.Top += VELOCITA_MOVIMENTO_ASCENSORE;
                                        marginAscensoreVecchio.Bottom -= VELOCITA_MOVIMENTO_ASCENSORE;
                                    }

                                    imgAscensore.Margin = marginAscensoreVecchio;
                                }));

                                semaforo.Release();
                            }
                        }
                        else if (pi.Stato == PersonaImage.Stati.In_Viaggio)
                        {
                            while (true)
                            {
                                Thread.Sleep(150);

                                semaforo.WaitOne();

                                //Piano corrente
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (Ascensore.CurrentPiano != 1 && (imgAscensore.Margin == ASCENSORE_PIANO_1))
                                        Ascensore.CurrentPiano = 1;
                                    else if (Ascensore.CurrentPiano != 2 && (imgAscensore.Margin == ASCENSORE_PIANO_2))
                                        Ascensore.CurrentPiano = 2;
                                    else if (Ascensore.CurrentPiano != 3 && (imgAscensore.Margin == ASCENSORE_PIANO_3))
                                        Ascensore.CurrentPiano = 3;
                                    else if (Ascensore.CurrentPiano != 4 && (imgAscensore.Margin == ASCENSORE_PIANO_4))
                                        Ascensore.CurrentPiano = 4;
                                    else if (Ascensore.CurrentPiano != 5 && (imgAscensore.Margin == ASCENSORE_PIANO_5))
                                        Ascensore.CurrentPiano = 5;
                                }));


                                //Controllo uscite
                                if (Ascensore.CurrentPiano == p.PrenotazioneAscensore.Piano)
                                {
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        BitmapImage sr = new BitmapImage();
                                        sr.BeginInit();
                                        sr.UriSource = new Uri(@"Src/ascensore.png", UriKind.Relative);
                                        sr.EndInit();

                                        imgAscensore.Source = sr;

                                        ominiImage.Remove(pi);

                                        Ascensore.ProcessaProssimaRichiesta();
                                    }));

                                    semaforo.Release();

                                    Debug.WriteLine("Omino arrivato nel piano a lui destinato");

                                    break;
                                }

                                //Muovo ascensore
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    Thickness marginAscensoreVecchio = imgAscensore.Margin;

                                    if (p.PrenotazioneAscensore.Piano > Ascensore.CurrentPiano)
                                    {
                                        marginAscensoreVecchio.Top -= VELOCITA_MOVIMENTO_ASCENSORE;
                                        marginAscensoreVecchio.Bottom += VELOCITA_MOVIMENTO_ASCENSORE;
                                    }
                                    else
                                    {
                                        marginAscensoreVecchio.Top += VELOCITA_MOVIMENTO_ASCENSORE;
                                        marginAscensoreVecchio.Bottom -= VELOCITA_MOVIMENTO_ASCENSORE;
                                    }

                                    imgAscensore.Margin = marginAscensoreVecchio;
                                }));

                                semaforo.Release();
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine("Errore: " + ex);
                FermaTutto();
            }
        }

        private int GetPosizionePersonaAscensoreDentroLista(Persona p)
        {
            semaforo.WaitOne();

            int toRet = 0;

            foreach (PersonaImage pi in ominiImage)
            {
                if(p.Equals(pi))
                {
                    break;
                }

                toRet++;
            }

            semaforo.Release();

            return toRet;
        }

        private void FermaTutto()
        {
            whileState = false;

            threadSpawnOmini.Abort();
            threadMuoviOmini.Abort();
            threadAscensore.Abort();

            threadSpawnOmini.Join();
            threadMuoviOmini.Join();
            threadAscensore.Join();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                FermaTutto();
            } catch(Exception ex)
            {
                Debug.WriteLine("Errore: " + ex);
            }
        }
    }
}
