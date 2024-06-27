namespace MarkeerRouteBackend.Models
{
    public class KlokPartijLijst
    {
        public List<KlokPartij> KlokPartijen { get; set; }

        public string KlokNummer { get; set; }

        public int GemiddeldeTijdPerPartij { get; set; }

        public int DebugAantalGeveild { get; set; }

        public int DebugAantalNogTeVeilen { get; set; }
    }
}
