namespace MarkeerRouteBackend
{
    public class GesorteerdeGemarkeerdePartij
    {
        public Guid KlokPartijId { get; set; }

        public int Prioriteit { get; set; }

        public string KlokNummer { get; set; }

        public int RouteVolgnummer { get; set; }

        public int VeilVolgordeKlok { get; set; }

        public int GeschatteTijdTotOnderKlok { get; set; }

        public string ProductNaam { get; set; }
        public string AanvoerderNaam { get; set; }
        public bool HeeftOverlap { get; set; }
        public int DebugAantalGeveild { get; set; }

        public int DebugAantalNogTeVeilen { get; set; }


    }
}
