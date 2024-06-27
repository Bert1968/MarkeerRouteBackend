
namespace MarkeerRouteBackend.Models
{
    public class ActuelePartijInfo
    {
        public DateTime DebugTijd { get; set; }
        public int DebugTimestamp { get; set; }
        public List<KlokPartijLijst> KlokAankomendePartijen { get; set; }

        public List<GesorteerdeGemarkeerdePartij> GemarkeerdePartijen { get; set; }

    }
}
