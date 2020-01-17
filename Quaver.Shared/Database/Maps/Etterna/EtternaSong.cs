using SQLite;

namespace Quaver.Shared.Database.Maps.Etterna
{
    [Table("songs")]
    public class EtternaSong
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Column("VERSION")]
        public double Version { get; set; }

        [Column("TITLE")]
        public string Title { get; set; }

        [Column("SUBTITLE")]
        public string Subtitle { get; set; }

        [Column("ARTIST")]
        public string Artist { get; set; }

        [Column("TITLETRANSLIT")]
        public string TitleTranslit { get; set; }

        [Column("SUBTITLETRANSLIT")]
        public string SubtitleTranslit { get; set; }

        [Column("ARTISTTRANSLIT")]
        public string ArtistTranslit { get; set; }

        [Column("GENRE")]
        public string Genre { get; set; }

        [Column("ORIGIN")]
        public string Origin { get; set; }

        [Column("CREDIT")]
        public string Credit { get; set; }

        [Column("BANNERPATH")]
        public string BannerPath { get; set; }

        [Column("BACKGROUNDPATH")]
        public string BackgroundPath { get; set; }

        [Column("MUSICPATH")]
        public string MusicPath { get; set; }

        [Column("SAMPLESTART")]
        public float SampleStart { get; set; }

        [Column("DISPLAYBPMMIN")]
        public float DisplayBPMMin { get; set; }

        [Column("SONGFILENAME")]
        public string SongFileName { get; set; }

        [Column("MUSICLENGTH")]
        public double MusicLength { get; set; }
    }
}