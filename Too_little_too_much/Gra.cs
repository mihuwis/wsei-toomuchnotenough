using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Too_little_too_much
{
    public class Gra
    {
        /// <summary>
        /// Górne ograniczenie losowanej liczby, która ma zostać odgadnięta.
        /// </summary>
        /// <value>
        /// Domyślna wartość wynosi 100. Wartość jest ustawiana w konstruktorze i nie może zmienić się podczas życia obiektu gry.
        /// </value>
        public int MaxLiczbaDoOdgadniecia { get; } = 100;

        /// <summary>
        /// Dolne ograniczenie losowanej liczby, która ma zostać odgadnięta.
        /// </summary>
        /// <value>
        /// Domyślna wartość wynosi 1. Wartość jest ustawiana w konstruktorze i nie może zmienić się podczas życia obiektu gry.
        /// </value>
        public int MinLiczbaDoOdgadniecia { get; } = 1;


        readonly private int liczbaDoOdgadniecia;


        /// <summary>
        /// Typ wyliczeniowy opisujący możliwe statusy gry.
        /// </summary>
        public enum Status
        {
            /// <summary>Status gry ustawiany w momencie utworzenia obiektu gry. Zmiana tego statusu mozliwa albo gdy liczba zostanie odgadnieta, albo jawnie przerwana przez gracza.</summary>
            WTrakcie,
            /// <summary>Status gry ustawiany w momencie odgadnięcia poszukiwanej liczby.</summary>
            Zakonczona,
            /// <summary>Status gry ustawiany w momencie jawnego przerwania gry przez gracza.</summary>
            Poddana
        };

        /// <summary>
        /// Właściwość tylko do odczytu opisujaca aktualny status (<see cref="Status"/>) gry.
        /// </summary>
        /// <remarks>
        /// <para>W momencie utworzenia obiektu, uruchomienia konstruktora, zmienna przyjmuje wartość <see cref="Gra.Status.WTrakcie"/>.</para>
        /// <para>Zmiana wartości zmiennej na <see cref="Gra.Status.Poddana"/> po uruchomieniu metody <see cref="Przerwij"/>.</para>
        /// <para>Zmiana wartości zmiennej na <see cref="Gra.Status.Zakonczona"/> w metodzie <see cref="Propozycja(int)"/>, po podaniu poprawnej, odgadywanej liczby.</para>
        /// </remarks>
        public Status StatusGry { get; private set; }


        private List<Ruch> listaRuchow;

        public IReadOnlyList<Ruch> ListaRuchow { get { return listaRuchow.AsReadOnly(); } }

        /// <summary>
        /// Czas rozpoczęcia gry, ustawiany w momencie utworzenia obiektu gry, w konstruktorze. Nie można go już zmodyfikować podczas życia obiektu.
        /// </summary>
        public DateTime CzasRozpoczecia { get; }
        public DateTime? CzasZakonczenia { get; private set; }

        /// <summary>
        /// Zwraca aktualny stan gry, od chwili jej utworzenia (wywołania konstruktora) do momentu wywołania tej własciwości.
        /// </summary>
        public TimeSpan AktualnyCzasGry => DateTime.Now - CzasRozpoczecia;
        public TimeSpan CalkowityCzasGry => (StatusGry == Status.WTrakcie) ? AktualnyCzasGry : (TimeSpan)(CzasZakonczenia - CzasRozpoczecia);

        public Gra(int min, int max)
        {
            if (min >= max)
                throw new ArgumentException();

            MinLiczbaDoOdgadniecia = min;
            MaxLiczbaDoOdgadniecia = max;

            liczbaDoOdgadniecia = (new Random()).Next(MinLiczbaDoOdgadniecia, MaxLiczbaDoOdgadniecia + 1);
            CzasRozpoczecia = DateTime.Now;
            CzasZakonczenia = null;
            StatusGry = Status.WTrakcie;

            listaRuchow = new List<Ruch>();
        }

        public Gra() : this(1, 100) { }


        /// <summary>
        /// Każde zadanie pytania o wynik skutkuje dopisaniem do listy
        /// </summary>
        /// <param name="pytanie"></param>
        /// <returns></returns>
        public Odpowiedz Ocena(int pytanie)
        {
            Odpowiedz odp;
            if (pytanie == liczbaDoOdgadniecia)
            {
                odp = Odpowiedz.Trafiony;
                StatusGry = Status.Zakonczona;
                CzasZakonczenia = DateTime.Now;
                listaRuchow.Add(new Ruch(pytanie, odp, Status.Zakonczona));
            }
            else if (pytanie < liczbaDoOdgadniecia)
                odp = Odpowiedz.ZaMalo;
            else
                odp = Odpowiedz.ZaDuzo;

            //dopisz do listy
            if (StatusGry == Status.WTrakcie)
            {
                listaRuchow.Add(new Ruch(pytanie, odp, Status.WTrakcie));
            }

            return odp;
        }

        public int Przerwij()
        {
            if (StatusGry == Status.WTrakcie)
            {
                StatusGry = Status.Poddana;
                CzasZakonczenia = DateTime.Now;
                listaRuchow.Add(new Ruch(null, null, Status.WTrakcie));
            }

            return liczbaDoOdgadniecia;
        }


        // struktury wewnętrzne, pomocnicze
        public enum Odpowiedz
        {
            ZaMalo = -1,
            Trafiony = 0,
            ZaDuzo = 1
        };

        public class Ruch
        {
            public int? Liczba { get; }
            public Odpowiedz? Wynik { get; }
            public Status StatusGry { get; }
            public DateTime Czas { get; }

            public Ruch(int? propozycja, Odpowiedz? odp, Status statusGry)
            {
                this.Liczba = propozycja;
                this.Wynik = odp;
                this.StatusGry = statusGry;
                this.Czas = DateTime.Now;
            }

            public override string ToString()
            {
                return $"({Liczba}, {Wynik}, {Czas}, {StatusGry})";
            }
        }


    }
}
