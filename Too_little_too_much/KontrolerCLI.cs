using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Too_little_too_much.Gra.Odpowiedz;

namespace Too_little_too_much
{
    public class KontrolerCLI
    {
        public const char ZNAK_ZAKONCZENIA_GRY = 'X';
        private const string PLIK_SAVE = "save.xml";

        private Gra gra;
        private WidokCLI widok;
        private Timer backupTimer;

        public int MinZakres { get; private set; } = 1;
        public int MaxZakres { get; private set; } = 100;

        public IReadOnlyList<Gra.Ruch> ListaRuchow
        {
            get
            { return gra.ListaRuchow; }
        }

        public KontrolerCLI()
        {

            if (File.Exists(PLIK_SAVE))
            {
                Console.WriteLine("wczytuje gierkę--------");
                gra = Gra.OdczytZXML(PLIK_SAVE);
                if (gra != null)
                {
                    Console.WriteLine("Odczytano stan poprzedniej gry.");
                }else
                {
                    Console.WriteLine("Nie udało się odczytać stanu gry. Rozpoczynam nową grę.");
                    gra = new Gra();
                }
            } else { gra = new Gra(); }
            widok = new WidokCLI(this);
            StartAutoBackup();
        }

        public void Uruchom()
        {
            widok.OpisGry();
            while (widok.ChceszKontynuowac("Czy chcesz kontynuować gre (t/n)? "))
                UruchomRozgrywke();
            StopAutoBackup();
        }

        public void UruchomRozgrywke()
        {
            widok.CzyscEkran();

            if (gra.StatusGry == Gra.Status.WTrakcie || gra.StatusGry == Gra.Status.Zawieszona)
            {
                Console.WriteLine("Jedziemy Kochaniii!!!");
            }
            else
            {
                gra = new Gra();
            }

            do
            {
                int propozycja = 0;
                try
                {
                    propozycja = widok.WczytajPropozycje();
                }
                catch (KoniecGryException)
                {
                    ZapiszStanGry();
                    gra.Przerwij();
                    return;
                }

                if (gra.StatusGry == Gra.Status.Poddana) break;

                switch (gra.Ocena(propozycja))
                {
                    case ZaDuzo:
                        widok.KomunikatZaDuzo();
                        break;
                    case ZaMalo:
                        widok.KomunikatZaMalo();
                        break;
                    case Trafiony:
                        widok.KomunikatTrafiono();
                        break;
                    default:
                        break;
                }
                widok.HistoriaGry();
            }
            while (gra.StatusGry == Gra.Status.WTrakcie);
        }

        private void ZapiszStanGry()
        {
            gra.ZapiszGreXml(PLIK_SAVE);
        }

        public void ZakonczGre()
        {
            StopAutoBackup();
            gra = null;
            widok.CzyscEkran();
            widok = null;
            System.Environment.Exit(0);
        }

        public void ZakonczRozgrywke()
        {
            gra.Przerwij();
        }

        private void StartAutoBackup()
        {
            backupTimer = new Timer(SaveState, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private void StopAutoBackup()
        {
            backupTimer?.Dispose();
        }

        private void SaveState(object state)
        {
            ZapiszStanGry();
        }


    }

    [Serializable]
    internal class KoniecGryException : Exception
    {
        public KoniecGryException()
        {
        }

        public KoniecGryException(string message) : base(message)
        {
        }

        public KoniecGryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected KoniecGryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
