using Bogus;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Music_player_Application.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Music_player_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(long incomingSeed = 58933423, double incomingLikes = 5.0, string lang = "en-US", string viewType = "table", int page = 1)
        {
            // Store viewType so the Index.cshtml can use it in @if statements
            ViewBag.ViewType = viewType;

            var locale = lang.Split('-')[0];
            var songs = new List<MusicProperties>();
            const double charsPerSec = 13.0;
            const int pageSize = 15;

            int startAt = (page - 1) * pageSize;

            for (int i = 0; i < pageSize; i++)
            {
                int currentItemIndex = startAt + i;
                var f = new Faker(locale);
                f.Random = new Randomizer((int)(incomingSeed + currentItemIndex));

                var song = new MusicProperties();
                song.SongId = $"song-{incomingSeed}-{currentItemIndex}";
                song.Artist = f.Name.FullName();
                song.Year = f.Date.Past(25).Year;
                song.PictureUrl = $"https://picsum.photos/seed/{song.SongId}/300";

                if (locale == "uk")
                {
                    var ukGenres = new[] { "Рок", "Поп", "Джаз", "Фолк", "Класика", "Електроніка", "Реп", "Метал", "Інді" };
                    song.Genre = f.PickRandom(ukGenres);
                    var adj = new[] { "Таємничий", "Золотий", "Останній", "Вечірній", "Вільний" };
                    song.song = f.PickRandom(adj) + " " + f.Name.JobTitle();
                    song.Album = f.Address.City() + " Selection";
                    song.record = f.Company.CompanyName() + " Рекордс";

                    // Using a loop to grow the lyrics
                    string builtLyrics = "";
                    for (int j = 0; j < 12; j++)
                    {
                        builtLyrics += $"Ми шукаємо {f.Address.City()}. Це наш {f.Name.JobArea()} звук. \n";
                    }

                    song.Lyrics = builtLyrics;
                }
                else
                {
                    song.Genre = f.Music.Genre();
                    song.song = f.Commerce.ProductName();
                    song.Album = f.Commerce.Department();
                    song.record = f.Company.CompanyName() + " Records";

                    // Build bigger lyrics using a loop
                    string builtLyrics = "";
                    for (int j = 0; j < 12; j++)
                    {
                        // Adding a review and a new line each time
                        builtLyrics += f.Rant.Review("music") + " " + f.Address.City() + "\n";
                    }

                    // Combine the looped lyrics with a final catchphrase
                    song.Lyrics = $"{builtLyrics}\n{f.Company.CatchPhrase()}";
                }
                int seconds = (int)Math.Ceiling(song.Lyrics.Length / charsPerSec);
                song.duration = TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");

                var likeRng = new Random((int)(incomingSeed + currentItemIndex + 999));
                int baseLikes = (int)Math.Floor(incomingLikes);
                song.Likes = (likeRng.NextDouble() < (incomingLikes - baseLikes)) ? baseLikes + 1 : baseLikes;

                songs.Add(song);
            }

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isAjax ? PartialView("Index", songs) : View("Index", songs);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult TestSeed()
        {
            // Hardcoded parameters for the testing phase
            const long testSeed = 58933423;
            const double testAvgLikes = 1.2;
            const int pageNum = 1;
            const int pageSize = 1000;

            var songs = new List<MusicProperties>();

            for (int i = 0; i < pageSize; i++)
            {
                // 1. Calculate the unique position for this song
                int globalIndex = ((pageNum - 1) * pageSize) + i;

                // 2. PARENT GENERATOR: For Song Data
                // Seeding with (testSeed + globalIndex) makes every row unique but stable
                var songFaker = new Faker<MusicProperties>("en")
                    .UseSeed((int)(testSeed + globalIndex))
                    .RuleFor(m => m.song, f => f.Music.Genre() + " " + f.Commerce.ProductName())
                    .RuleFor(m => m.Artist, f => f.Name.FullName())
                    .RuleFor(m => m.Album, f => f.Random.Bool(0.2f) ? "Single" : f.Commerce.ProductName())
                    .RuleFor(m => m.Genre, f => f.Music.Genre())
                    .RuleFor(m => m.Year, f => f.Random.Int(1950, 2024))
                    .RuleFor(m => m.record, f => f.Company.CompanyName() + " Records")
                    .RuleFor(m => m.duration, f => TimeSpan.FromSeconds(f.Random.Int(120, 300)).ToString(@"m\:ss"))
                    .RuleFor(m => m.SongId, f => $"song-{testSeed}-{globalIndex}")
                    .RuleFor(m => m.PictureUrl, (f, m) => $"https://picsum.photos/seed/{m.SongId}/300")
                    .RuleFor(m => m.Lyrics, f => {
                        var lines = new List<string>();
                        for (int j = 0; j < 4; j++)
                        {
                            lines.Add(j % 2 == 0 ? f.Rant.Review("music") : f.Company.CatchPhrase());
                        }
                        return string.Join("\n", lines);
                    });

                var song = songFaker.Generate();

                // 3. CHILD GENERATOR: Probability Likes Logic Fix
                // We use a different offset (+ 999) so the "roll" doesn't match the "song name"
                var likeRng = new Random((int)(testSeed + globalIndex + 999));

                // Let's be explicit to avoid the "3 likes" issue:
                int baseLikes = (int)Math.Floor(testAvgLikes); // If 1.2, this is 1
                double chanceForExtra = testAvgLikes - baseLikes; // If 1.2, this is 0.2 (20%)

                // We ROLL THE DICE once.
                double roll = likeRng.NextDouble();

                if (roll < chanceForExtra)
                {
                    song.Likes = baseLikes + 1; // Result: 2
                }
                else
                {
                    song.Likes = baseLikes; // Result: 1
                }

                // Note: It is mathematically impossible for song.Likes to be 3 
                // if baseLikes is 1 and we only add 1 based on a single roll.

                songs.Add(song);
            }

            return View(songs);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
