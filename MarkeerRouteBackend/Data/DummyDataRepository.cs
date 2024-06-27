using MarkeerRouteBackend.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace MarkeerRouteBackend.Data
{
    public class DummyDataRepository
    {
        public static List<DummyDataItem> Repository;
        public static List<Guid> GemarkeerdePartijen;
        private int _overlapLimiet = 10;

        public List<GesorteerdeGemarkeerdePartij> GetAankomendeGemarkeerdePartijen(int timestamp, List<KlokPartijLijst> partijLijsten)
        {
            var gemarkeerdePartijen = new List<GesorteerdeGemarkeerdePartij>();
            foreach (var klokPartijen in partijLijsten)
            {
                gemarkeerdePartijen.AddRange(
                    klokPartijen.KlokPartijen.Where(p => GemarkeerdePartijen.Contains(p.Id))
                        .Select(p => new GesorteerdeGemarkeerdePartij
                        {
                            KlokNummer = klokPartijen.KlokNummer,
                            KlokPartijId = p.Id,
                            Prioriteit = 1,
                            VeilVolgordeKlok = p.VeilVolgorde,
                            GeschatteTijdTotOnderKlok = p.VeilVolgorde * klokPartijen.GemiddeldeTijdPerPartij,
                            AanvoerderNaam = p.AanvoerderNaam,
                            ProductNaam = p.ProductNaam
                        }
                 ));
            }
            gemarkeerdePartijen = gemarkeerdePartijen.OrderBy(p => p.GeschatteTijdTotOnderKlok).ToList();
            for(int i = 0; i < gemarkeerdePartijen.Count; i++)
            {
                gemarkeerdePartijen[i].RouteVolgnummer = i;
                if(i > 0 && 
                    Math.Abs(gemarkeerdePartijen[i-1].GeschatteTijdTotOnderKlok - gemarkeerdePartijen[i].GeschatteTijdTotOnderKlok) < _overlapLimiet)
                {
                    gemarkeerdePartijen[i].HeeftOverlap = true;
                    gemarkeerdePartijen[i - 1].HeeftOverlap = true;
                }
                gemarkeerdePartijen[i].DebugAantalGeveild = GemarkeerdePartijen.Count - gemarkeerdePartijen.Count;
                gemarkeerdePartijen[i].DebugAantalNogTeVeilen = gemarkeerdePartijen.Count;
            }
            return gemarkeerdePartijen;
        }
                


        public KlokPartijLijst GetAankomendePartijen(string klok, int timestamp, int gemiddeldeVertraging)
        {
            var alleKlokPartijen = Repository.Where(p => p.AuctionInformationClockNumber == klok)
                .Select(p => new KlokPartij
                {
                    AantalInPartij = p.CurrentNumberOfPieces,
                    AanvoerderNaam = p.SupplierOrganizationName,
                    Id = p.Id,
                    ProductNaam = p.VbnProductName,
                    WerkelijkeTijdPartij = gemiddeldeVertraging,
                    IsGemarkeerd = GemarkeerdePartijen.Contains(p.Id)
                });
            int aantalPartijenAlGeweest = timestamp / gemiddeldeVertraging;
            var aankomendeKlokPartijen = alleKlokPartijen.Skip(aantalPartijenAlGeweest).ToList();

            for(int i = 0; i < aankomendeKlokPartijen.Count; i++)
            {
                aankomendeKlokPartijen[i].VeilVolgorde = i;
            }

            return new KlokPartijLijst { 
                DebugAantalGeveild = aantalPartijenAlGeweest,
                DebugAantalNogTeVeilen = aankomendeKlokPartijen.Count,
                GemiddeldeTijdPerPartij = gemiddeldeVertraging,
                KlokNummer = klok,
                KlokPartijen = aankomendeKlokPartijen
            };
        }


        public void CreateRepository()
        {
            string jsonFilePath = "dummyData.json";
            if (Repository == null)
            {
                ProcessDemoDataFile(jsonFilePath);
  
                GemarkeerdePartijen = new List<Guid>
                {
                    new Guid("76b96792-ae24-47ab-9014-c90a249d364b"),
                    new Guid("ad636066-d176-4400-970e-6cb15085276f"),
                    new Guid("48018b65-4dd4-4b88-8fee-2eaea689fb55"),
                    new Guid("69c61ad6-b837-4b09-94d1-c0ece8a02b0a"),
                    new Guid("f3848caa-c291-4cfe-b898-26e117c545c9"),
                    new Guid("58e79673-47ad-430b-af0e-37ff21a954f5"),
                    new Guid("2b3a8232-ded7-4ceb-8a6a-47635e85fe78")
                };
            }
        }


        private static List<T> GetRandomItems<T>(List<T> list, int count)
        {
            if (count > list.Count / 2)
            {
                throw new ArgumentException("Number of items too great.");
            }

            Random random = new Random();
            List<T> randomItems = new List<T>();

            for (int i = 0; i < count; i++)
            {
                int index = random.Next(list.Count);
                if (randomItems.Contains(list[index]))
                {
                    count--;
                }
                else
                {
                    randomItems.Add(list[index]);
                }
            }

            return randomItems;
        }
    

    private void ProcessDemoDataFile(string resourceName)
        {
            List<DummyDataItem>? demoClockSupplyLineDtos;
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames().Single(resource => resource.EndsWith(resourceName));
            var stream = assembly.GetManifestResourceStream(resourcePath)!;
            using (stream)
            {
                using StreamReader reader = new(stream);
                var result = reader.ReadToEnd();
                Repository = JsonConvert.DeserializeObject<List<DummyDataItem>>(result, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }

        }

        }
}
