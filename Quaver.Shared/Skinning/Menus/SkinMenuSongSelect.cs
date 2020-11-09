using IniFileParser.Model;
using Quaver.Shared.Config;

namespace Quaver.Shared.Skinning.Menus
{
    public class SkinMenuSongSelect : SkinMenu
    {
        public bool DisplayMapBackground { get; private set; }

        public byte? MapBackgroundBrightness { get; private set; }

        public SkinMenuSongSelect(SkinStore store, IniData config) : base(store, config)
        {
        }

        protected override void ReadConfig()
        {
            var ini = Config["SongSelect"];

            var displayMapBackground = ini["DisplayMapBackground"];
            ReadIndividualConfig(displayMapBackground, () => DisplayMapBackground = ConfigHelper.ReadBool(false, displayMapBackground));

            var mapBackgroundBrightness = ini["MapBackgroundBrightness"];
            ReadIndividualConfig(mapBackgroundBrightness, () => MapBackgroundBrightness = ConfigHelper.ReadByte(0, mapBackgroundBrightness));
        }

        protected override void LoadElements()
        {
        }
    }
}