namespace Music_player_Application.Models
{
    public class MusicProperties
    {
        // Keep these exactly as you had them
        public string song { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public string Lyrics { get; set; }
        public string duration { get; set; }
        public string record { get; set; } // This is a string! (e.g., "Apple Records")
        public int Year { get; set; }

        // --- NEW REQUIRED PROPERTIES ---

        // 1. To show the Likes (the heart/thumb icon in your screenshot)
        public int Likes { get; set; }

        // 2. To show the Album Art (the "Get Schwifty" image)
        public string PictureUrl { get; set; }

        // 3. A unique ID for this specific row (needed for the player to work)
        public string SongId { get; set; }
    }
}