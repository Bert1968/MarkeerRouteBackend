using MarkeerRouteBackend.Data;
using MarkeerRouteBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarkeerRouteBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MarkeerRouteController : ControllerBase
    {
        private readonly ILogger<MarkeerRouteController> _logger;

        private const int _veilLoopTijd = 300;
        private DummyDataRepository _repo;

        public MarkeerRouteController(ILogger<MarkeerRouteController> logger)
        {
            _logger = logger;
            _repo = new DummyDataRepository();
            _repo.CreateRepository();


        }

        [HttpGet(Name = "GetVeilVolgorde")]
        public ActuelePartijInfo Get()
        {
            int timestamp = (_repo.GetCurrentSeconds() - DummyDataRepository.StartTimeStamp) % _veilLoopTijd;
            _logger.LogInformation("{current} , {start}, {diff}, {veillooptijd}, {timestamp}",
                _repo.GetCurrentSeconds(), DummyDataRepository.StartTimeStamp, (_repo.GetCurrentSeconds() - DummyDataRepository.StartTimeStamp), _veilLoopTijd, timestamp);

            var partijInfo =  new ActuelePartijInfo
            {
                DebugTijd = DateTime.Now,
                DebugTimestamp = timestamp,
                KlokAankomendePartijen = new List<KlokPartijLijst>
                {
                    _repo.GetAankomendePartijen("C01",timestamp, 12),
                    _repo.GetAankomendePartijen("C02",timestamp, 13),
                    _repo.GetAankomendePartijen("C03",timestamp, 14)
                }
            };

            partijInfo.GemarkeerdePartijen = _repo.GetAankomendeGemarkeerdePartijen(partijInfo.KlokAankomendePartijen);
            return partijInfo;
        }

        [HttpDelete("{id:Guid}")]
        public void DeleteMarkering(Guid id)
        {
            _repo.DeleteMarkering(id);
        }

        [HttpPut(Name = "ResetRepository")]
        public void ResetRepository()
        {
            _repo.ResetRepository();
        }
    }
}
