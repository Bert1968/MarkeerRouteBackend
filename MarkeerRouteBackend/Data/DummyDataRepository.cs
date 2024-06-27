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
        public static List<DummyDataItem>? Repository;
        public static List<Guid> GemarkeerdePartijen;
        private int _overlapLimiet = 10;
        public static int StartTimeStamp { get; private set; }

        public List<GesorteerdeGemarkeerdePartij> GetAankomendeGemarkeerdePartijen( List<KlokPartijLijst> partijLijsten)
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
                            GeschatteTijdLeesbaar = LeesbareTijd(p.VeilVolgorde * klokPartijen.GemiddeldeTijdPerPartij),
                            AanvoerderNaam = p.AanvoerderNaam,
                            ProductNaam = p.ProductNaam
                        }
                 ));
            }
            gemarkeerdePartijen = gemarkeerdePartijen.OrderBy(p => p.GeschatteTijdTotOnderKlok).ToList();
            for (int i = 0; i < gemarkeerdePartijen.Count; i++)
            {
                gemarkeerdePartijen[i].RouteVolgnummer = i;
                if (i > 0 &&
                    Math.Abs(gemarkeerdePartijen[i - 1].GeschatteTijdTotOnderKlok - gemarkeerdePartijen[i].GeschatteTijdTotOnderKlok) < _overlapLimiet)
                {
                    gemarkeerdePartijen[i].HeeftOverlap = true;
                    gemarkeerdePartijen[i - 1].HeeftOverlap = true;
                }
                gemarkeerdePartijen[i].DebugAantalGeveild = GemarkeerdePartijen.Count - gemarkeerdePartijen.Count;
                gemarkeerdePartijen[i].DebugAantalNogTeVeilen = gemarkeerdePartijen.Count;
            }
            return gemarkeerdePartijen;
        }

        private string LeesbareTijd(int secondes)
        {
            if(secondes < 60)
            {
                //Afronden op 15 seconde
                return $"~ {((secondes + 2) / 5) * 5} seconden";
            }
            else if (secondes < 180)
            {
                return $"~ {secondes / 60 + ((secondes % 60) / 30) * .50} minuten";
            }
            else
            {
                return $"~ {secondes / 60} minuten";
            }
        }

        public void DeleteMarkering(Guid id)
        {
            if(GemarkeerdePartijen.Contains(id)) 
            {
                GemarkeerdePartijen.Remove(id);
            }
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

            for (int i = 0; i < aankomendeKlokPartijen.Count; i++)
            {
                aankomendeKlokPartijen[i].VeilVolgorde = i;
            }

            return new KlokPartijLijst
            {
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
                StartTimeStamp = GetCurrentSeconds();
                GemarkeerdePartijen = new List<Guid>
                {
                    new Guid("0fc92f25-b53d-479c-9190-e500a656cfc6"),
                    new Guid("17f307c1-ea68-3263-be40-fd2386049b03"),
                    new Guid("e1d85801-a664-47fa-9bc8-e9e80a558e7f"),
                    new Guid("18463c3a-107d-417f-8673-c241051cfc05"),
                    new Guid("d335c930-da9b-3219-8082-6fdfb13804cc"),
                    new Guid("42efefde-bc3b-45bf-9b29-6403737d9ab7"),
                    new Guid("845ec058-9980-432b-a9c5-8a24d94f74dd"),
                    new Guid("756a1647-9779-326a-9bf1-1622b76bee9b"),
                    new Guid("efc99cf4-c71e-4773-9cc3-5cb1f76ac4bf"),
                    new Guid("da5abdab-a817-4c27-bc5b-097bb822b684"),
                    new Guid("b7753a74-90b1-3b49-9e40-040f75c00c64"),
                    new Guid("b854eebf-985c-3104-b89b-e6d3a3ba859f")
                };
            }
        }

        public void ResetRepository()
        {
            Repository = null;
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

        public int GetCurrentSeconds()
        {
            DateTime now = DateTime.Now;
            return now.Hour * 3600 + now.Minute * 60 + now.Second;
        }
    }
}
