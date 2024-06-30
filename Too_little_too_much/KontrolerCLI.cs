﻿using System;
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
                if(gra != null)
                {
                    Console.WriteLine("Odczytano stan poprzedniej gry.");
                    File.Delete(PLIK_SAVE);
                    //if (widok.ChceszKontynuowac("Czy chcesz kontynuować poprzednią grę (t/n)? "))
                    //{
                    //    File.Delete(PLIK_SAVE);
                    //}
                    //else
                    //{
                    //    gra = new Gra();
                    //    File.Delete(PLIK_SAVE);
                    //}
                }else
                {
                    gra = new Gra();
                }
            } else { gra = new Gra(); }
            widok = new WidokCLI(this);
        }

        public void Uruchom()
        {
            widok.OpisGry();
            while (widok.ChceszKontynuowac("Czy chcesz kontynuować aplikację (t/n)? "))
                UruchomRozgrywke();
        }

        public void UruchomRozgrywke()
        {
            widok.CzyscEkran();
            // ustaw zakres do losowania


            gra = new Gra(MinZakres, MaxZakres); //może zgłosić ArgumentException

            do
            {
                //wczytaj propozycję
                int propozycja = 0;
                try
                {
                    propozycja = widok.WczytajPropozycje();
                }
                catch (KoniecGryException)
                {
                    Console.WriteLine("Kończymy grę?");
                    gra.Przerwij();
                    ZapiszStanGry();
                    return;
                }

                if (gra.StatusGry == Gra.Status.Poddana) break;

                //Console.WriteLine( gra.Ocena(propozycja) );
                //oceń propozycję, break
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

            //if StatusGry == Przerwana wypisz poprawną odpowiedź
            //if StatusGry == Zakończona wypisz statystyki gry
        }

        private void ZapiszStanGry()
        {
            gra.ZapiszGreXml(PLIK_SAVE);
        }

        public void UstawZakresDoLosowania(ref int min, ref int max)
        {

        }

        public int LiczbaProb() => gra.ListaRuchow.Count();

        public void ZakonczGre()
        {
            //np. zapisuje stan gry na dysku w celu późniejszego załadowania
            //albo dopisuje wynik do Top Score
            //sprząta pamięć
            gra = null;
            widok.CzyscEkran(); //komunikat o końcu gry
            widok = null;
            System.Environment.Exit(0);
        }

        public void ZakonczRozgrywke()
        {
            gra.Przerwij();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <exception cref="KoniecGryException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <returns></returns>
        public int WczytajLiczbeLubKoniec(string value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            value = value.TrimStart().ToUpper();
            if (value.Length > 0 && value[0].Equals(ZNAK_ZAKONCZENIA_GRY))
                throw new KoniecGryException();

            //UWAGA: ponizej może zostać zgłoszony wyjątek 
            return Int32.Parse(value);
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
