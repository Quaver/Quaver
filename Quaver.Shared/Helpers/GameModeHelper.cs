using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Skinning;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Helpers
{
    public static class GameModeHelper
    {
        private static Dictionary<GameMode, Color> modeColors = new Dictionary<GameMode, Color>
        {
            { GameMode.Keys1, new Color(128, 128, 128) },
            { GameMode.Keys2, new Color(128, 128, 128) },
            { GameMode.Keys3, new Color(128, 128, 128) },
            { GameMode.Keys4, new Color(5, 135, 229) },
            { GameMode.Keys5, new Color(128, 128, 128) },
            { GameMode.Keys6, new Color(128, 128, 128) },
            { GameMode.Keys7, new Color(155, 81, 224) },
            { GameMode.Keys8, new Color(128, 128, 128) },
            { GameMode.Keys9, new Color(128, 128, 128) },
            { GameMode.Keys10, new Color(128, 128, 128) },

        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapModes"></param>
        /// <param name="sprite">
        /// Reference to the sprite that will be set to the game mode texture.
        /// </param>
        /// <param name="text">
        /// Reference to the text that will be set to the game mode text. Will be "" if skin has a custom game mode texture.
        /// </param>
        public static void SetGameModeTexture(IEnumerable<GameMode> mapModes, Sprite sprite, SpriteTextPlus text)
        {
            var modes = mapModes.Distinct().OrderBy(m => ModeHelper.ToKeyCount(m)).ToList();
            bool k4 = modes.Count == 1 && modes[0] == GameMode.Keys4;
            bool k7 = modes.Count == 1 && modes[0] == GameMode.Keys7;
            bool k47 = modes.Count == 2 && modes.Contains(GameMode.Keys4) && modes.Contains(GameMode.Keys7);

            sprite.Tint = Color.White;

            if (modes.Count == 0)
            {
                sprite.Image = UserInterface.KeysNonePanel;
                text.Text = "";
                return;
            }

            if (k4 && SkinManager.Skin?.SongSelect?.GameMode4K != null)
            {
                sprite.Image = SkinManager.Skin.SongSelect.GameMode4K;
                text.Text = "";
                return;
            }
            if (k7 && SkinManager.Skin?.SongSelect?.GameMode7K != null)
            {
                sprite.Image = SkinManager.Skin.SongSelect.GameMode7K;
                text.Text = "";
                return;
            }

            if (k47)
            {
                if (SkinManager.Skin?.SongSelect?.GameMode4K7K != null)
                {
                    sprite.Image = SkinManager.Skin.SongSelect.GameMode4K7K;
                    text.Text = "";
                    return;
                }

                sprite.Image = UserInterface.Keys47Panel;
                text.Text = "4K/7K";
                return;
            }

            if ((!k4 && !k7 && !k47) && SkinManager.Skin?.SongSelect?.GameModeOther != null)
            {
                sprite.Image = SkinManager.Skin.SongSelect.GameModeOther;
            }
            else
            {
                var color = modes.Count == 1 ? modeColors[modes[0]] : new Color(128, 128, 128);

                sprite.Image = UserInterface.ModePanel;
                sprite.Tint = color;
            }

            string modesText = "";

            if (modes.Count <= 5)
            {
                for (int i = 0; i < modes.Count; i++)
                {
                    if (i != 0) modesText += "/";

                    modesText += modes.Count <= 3 ? ModeHelper.ToShortHand(modes[i]) : ModeHelper.ToKeyCount(modes[i]);
                }
            }
            else
            {
                modesText = "MIXED";
            }


            text.Text = modesText;
        }
    }
}