using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics
{
    public class ClanTag : SpriteTextPlus
    {
        public bool HasClan { get; private set; }

        public Color BaseColor { get; private set; } = Color.Beige;

        public ClanTag(int fontSize) : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "", fontSize)
        {
            Visible = false;
        }

        public void UpdateFromUser(OnlineUser? user, Color? fallbackColor = null) =>
            UpdateFromClan(user?.ClanTag, user?.ClanAccentColor, fallbackColor);

        public void UpdateFromClan(string? clanTag, string? clanAccentColor, Color? fallbackColor = null)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                Clear();
                return;
            }

            BaseColor = GetColor(clanAccentColor, fallbackColor ?? Color.Beige);
            Text = $"[{clanTag}]";
            Tint = BaseColor;
            HasClan = true;
            Visible = true;
        }

        public void Clear()
        {
            Text = "";
            HasClan = false;
            Visible = false;
        }

        private static Color GetColor(string? hex, Color fallback)
        {
            if (string.IsNullOrEmpty(hex))
                return fallback;

            try
            {
                return ColorHelper.HexToColor(hex);
            }
            catch
            {
                return fallback;
            }
        }
    }
}
