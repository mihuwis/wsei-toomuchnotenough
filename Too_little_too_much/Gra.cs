using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml;

namespace Too_little_too_much
{
    [DataContract]
    public class Gra
    {
        [DataMember]
        public int MaxLiczbaDoOdgadniecia { get; private set; }
        [DataMember]
        public int MinLiczbaDoOdgadniecia { get; private set; }
        [DataMember]
        readonly private int liczbaDoOdgadniecia;
        public int LiczbaDoOdgadniecia => liczbaDoOdgadniecia;

        public enum Status
        {
            WTrakcie,
            Zakonczona,
            Poddana,
            Zawieszona
        };

        [DataMember]
        public Status StatusGry { get; private set; }

        [DataMember]
        private List<Ruch> listaRuchow;

        public IReadOnlyList<Ruch> ListaRuchow { get { return listaRuchow.AsReadOnly(); } }

        [DataMember]
        public DateTime CzasRozpoczecia { get; private set; }

        [DataMember]
        public DateTime? CzasZakonczenia { get; private set; }

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
                listaRuchow.Add(new Ruch(null, null, Status.Zawieszona));
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

        public void ZapiszGreXml(string filePath)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Gra));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.WriteObject(fileStream, this);
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Zapis XML sie nie udał : {ex.Message}");
            }
        }

        public static Gra OdczytZXML(string filePath)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Gra));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    return (Gra)serializer.ReadObject(fileStream);
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Nie udało sie odczytac XML : {ex.Message}");
                return null;
            }
        }

        public override string ToString()
        {
            return $"(Status: {StatusGry}, do Odgadniecia: {LiczbaDoOdgadniecia}, count: {listaRuchow.Count()}";
        }

        [DataContract]
        public class Ruch
        {
            [DataMember]
            public int? Liczba { get; private set; }
            [DataMember]
            public Odpowiedz? Wynik { get; private set; }
            [DataMember]
            public Status StatusGry { get; private set; }
            [DataMember]
            public DateTime Czas { get; private set; }

            public Ruch(int? propozycja, Odpowiedz? odp, Status statusGry)
            {
                Liczba = propozycja;
                Wynik = odp;
                StatusGry = statusGry;
                Czas = DateTime.Now;
            }

            public override string ToString()
            {
                return $"({Liczba}, {Wynik}, {Czas}, {StatusGry})";
            }
        }



    }
}
