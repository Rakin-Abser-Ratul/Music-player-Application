using Bogus;
using Microsoft.AspNetCore.Mvc;
using Music_player_Application.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Music_player_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(long incomingSeed = 58932423, double incomingLikes = 5.0, string lang = "en-US", string viewType = "table", int page = 1)
        {
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

                MusicProperties song;

                if (locale == "uk")
                {
                    song = GenerateUkrainianSong(f, incomingSeed, currentItemIndex, charsPerSec, incomingLikes);
                }
                else
                {
                    song = GenerateEnglishSong(f, incomingSeed, currentItemIndex, charsPerSec, incomingLikes);
                }

                songs.Add(song);
            }

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isAjax ? PartialView("Index", songs) : View("Index", songs);
        }

        private MusicProperties GenerateUkrainianSong(Faker f, long seed, int index, double charsPerSec, double incomingLikes)
        {
            var song = new MusicProperties();

            song.SongId = $"song-{seed}-{index}";
            song.Artist = Transliterate(f.Name.FullName());
            song.Year = f.Date.Past(25).Year;
            song.PictureUrl = $"https://picsum.photos/seed/{song.SongId}/300";

            // Genres in Ukrainian
            var ukGenres = new[]
            {
                "Рок", "Поп", "Джаз", "Фольк", "Класика", "Електроніка", "Реп", "Метал", "Інді",
                "Синт-поп", "Техно", "Блюз", "Психоделія", "Гранж", "Дабстеп", "Етно",
                "Транс", "Соул", "Фанк", "Хардкор", "Панк", "Диско", "Лоу-фай"
            };

            // Adjectives in Ukrainian
            var adj = new[]
            {
                "Таємничий", "Золотий", "Останній", "Вечірній", "Вільний", "Забутий",
                "Швидкий", "Глибокий", "Нічний", "Прозорий", "Сталевий", "Мрійливий",
                "Космічний", "Далекий", "Рідний", "Дикий", "Світлий", "Крижаний",
                "Гірський", "Лісовий", "Міський", "Ранковий", "Північний", "Весняний"
            };

            // Emotion phrases in Ukrainian
            var emotions = new[]
            {
                "піднімай руки", "танцюй із серцем", "співай голосно",
                "мрій разом", "світло в ночі", "серця зустрічаються",
                "обійми ніч", "рухайся з ритмом", "хай музика ллється"
            };

            // Lyric templates in Ukrainian
            var ukLyricLines = new[]
            {
                "Ми шукаємо {0}.",
                "Це наш {1} звук у ночі.",
                "Ритм міста {0} б'ється в серці.",
                "Тут панує {1} мелодія.",
                "Відчуй, як {0} засинає під цей спів.",
                "Тільки {1} голос веде нас вперед.",
                "Світло згасло у {0}, лишилась музика.",
                "Наш {1} стиль не зупинити.",
                "Спогади про {0} летять крізь час.",
                "Це справжній {1} драйв!"
            };

            var choruses = new[]
            {
                "Це наш {1} ритм у місті {0}.",
                "Почувай, як {1} зливається зі звуком {0}.",
                "Вся музика {1} проходить через {0}."
            };

            int totalLines = 40;
            var lyricsBuilder = new StringBuilder();

            for (int j = 0; j < totalLines; j++)
            {
                string city = f.Address.City();      // Could be in English, transliterate anyway
                string jobArea = f.Name.JobArea();   // Could be in English, transliterate anyway

                if (j > 0 && j % 8 == 0)
                {
                    string chorusTemplate = choruses[(index + j / 8) % choruses.Length];
                    string chorus = string.Format(chorusTemplate, city, jobArea);
                    lyricsBuilder.AppendLine($"[Приспів] {Transliterate(chorus)}");
                }
                else
                {
                    string lineTemplate = ukLyricLines[(index + j) % ukLyricLines.Length];
                    string line = string.Format(lineTemplate, city, jobArea);
                    string emotion = emotions[(index + j) % emotions.Length];
                    lyricsBuilder.AppendLine(Transliterate($"{line}, {emotion}"));
                }
            }

            song.Lyrics = lyricsBuilder.ToString();
            song.Genre = ukGenres[index % ukGenres.Length];
            song.song = Transliterate($"{adj[index % adj.Length]} {f.Name.JobTitle()}");
            song.Album = Transliterate(f.Address.City() + " Selection");
            song.record = Transliterate(f.Company.CompanyName() + " Records");

            int seconds = (int)Math.Ceiling(song.Lyrics.Length / charsPerSec);
            song.duration = TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");

            var likeRng = new Random((int)(seed + index + 999));
            int baseLikes = (int)Math.Floor(incomingLikes);
            song.Likes = (likeRng.NextDouble() < (incomingLikes - baseLikes)) ? baseLikes + 1 : baseLikes;

            return song;
        }

        private MusicProperties GenerateEnglishSong(Faker f, long seed, int index, double charsPerSec, double incomingLikes)
        {
            var song = new MusicProperties();

            song.SongId = $"song-{seed}-{index}";
            song.Artist = f.Name.FullName();
            song.Year = f.Date.Past(25).Year;
            song.PictureUrl = $"https://picsum.photos/seed/{song.SongId}/300";

            song.Genre = f.Music.Genre();
            song.song = f.Commerce.ProductName();
            song.Album = f.Commerce.ProductName();
            song.record = f.Company.CompanyName() + " Records";

            var lyricsBuilder = new StringBuilder();
            for (int j = 0; j < 12; j++)
                lyricsBuilder.AppendLine(f.Rant.Review("music"));

            song.Lyrics = $"{lyricsBuilder}\n{f.Company.CatchPhrase()}";
            int seconds = (int)Math.Ceiling(song.Lyrics.Length / charsPerSec);
            song.duration = TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");

            var likeRng = new Random((int)(seed + index + 999));
            int baseLikes = (int)Math.Floor(incomingLikes);
            song.Likes = (likeRng.NextDouble() < (incomingLikes - baseLikes)) ? baseLikes + 1 : baseLikes;

            return song;
        }

        /// <summary>
        /// Transliterate Cyrillic Ukrainian text to English letters
        /// </summary>
        private string Transliterate(string text)
        {
            var translit = new Dictionary<char, string>
            {
                {'А',"A"}, {'Б',"B"}, {'В',"V"}, {'Г',"H"}, {'Ґ',"G"}, {'Д',"D"}, {'Е',"E"},
                {'Є',"Ye"}, {'Ж',"Zh"}, {'З',"Z"}, {'И',"Y"}, {'І',"I"}, {'Ї',"Yi"}, {'Й',"Y"},
                {'К',"K"}, {'Л',"L"}, {'М',"M"}, {'Н',"N"}, {'О',"O"}, {'П',"P"}, {'Р',"R"},
                {'С',"S"}, {'Т',"T"}, {'У',"U"}, {'Ф',"F"}, {'Х',"Kh"}, {'Ц',"Ts"}, {'Ч',"Ch"},
                {'Ш',"Sh"}, {'Щ',"Shch"}, {'Ь',""}, {'Ю',"Yu"}, {'Я',"Ya"}, {'а',"a"}, {'б',"b"},
                {'в',"v"}, {'г',"h"}, {'ґ',"g"}, {'д',"d"}, {'е',"e"}, {'є',"ye"}, {'ж',"zh"},
                {'з',"z"}, {'и',"y"}, {'і',"i"}, {'ї',"yi"}, {'й',"y"}, {'к',"k"}, {'л',"l"},
                {'м',"m"}, {'н',"n"}, {'о',"o"}, {'п',"p"}, {'р',"r"}, {'с',"s"}, {'т',"t"},
                {'у',"u"}, {'ф',"f"}, {'х',"kh"}, {'ц',"ts"}, {'ч',"ch"}, {'ш',"sh"}, {'щ',"shch"},
                {'ь',""}, {'ю',"yu"}, {'я',"ya"}
            };

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (translit.ContainsKey(c)) sb.Append(translit[c]);
                else sb.Append(c);
            }
            return sb.ToString();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
