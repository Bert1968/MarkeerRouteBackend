namespace MarkeerRouteBackend.Models
{
    public class KlokPartij
    {
        public Guid Id { get; set; }
        public string ProductNaam { get; set; }
        public string AanvoerderNaam { get; set; }
        public int AantalInPartij { get; set; }
        public int VeilVolgorde { get; set; }
        public int WerkelijkeTijdPartij { get; set; }
        public bool IsGemarkeerd { get; set; }
    }
}
