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
        /// <param name="maps"></param>
        /// <param name="sprite">
        /// Reference to the sprite that will be set to the game mode texture.
        /// </param>
        /// <param name="text">
        /// Reference to the text that will be set to the game mode text. Will be "" if skin has a custom game mode texture.
        /// </param>
        public static void SetGameModeTexture(List<Map> maps, Sprite sprite, SpriteTextPlus text)
        {
            var modes = maps.Select(m => m.Mode).Distinct().OrderBy(m => ModeHelper.ToKeyCount(m)).ToList();

            if (modes.Count == 0)
            {
                sprite.Image = UserInterface.KeysNonePanel;
                text.Text = "";
                return;
            }

            if (modes.Count == 1 && modes[0] == GameMode.Keys4 && SkinManager.Skin?.SongSelect?.GameMode4K != null)
            {
                sprite.Image = SkinManager.Skin.SongSelect.GameMode4K;
                text.Text = "";
                return;
            }
            if (modes.Count == 1 && modes[0] == GameMode.Keys7 && SkinManager.Skin?.SongSelect?.GameMode7K != null)
            {
                sprite.Image = SkinManager.Skin.SongSelect.GameMode7K;
                text.Text = "";
                return;
            }

            if (modes.Count == 2 && modes.Contains(GameMode.Keys4) && modes.Contains(GameMode.Keys7))
            {
                if (SkinManager.Skin?.SongSelect?.GameMode4K7K != null)
                {
                    sprite.Image = SkinManager.Skin.SongSelect.GameMode4K7K;
                    text.Text = "";
                    return;
                }

                sprite.Image = UserInterface.Keys47Panel;
                text.Text = "4K / 7K";
                return;
            }

            var color = modes.Count == 1 ? modeColors[modes[0]] : new Color(128, 128, 128);

            sprite.Image = UserInterface.ModePanel;
            sprite.Tint = color;

            var modesText = modes.Count == 1 ? ModeHelper.ToShortHand(modes[0]) : "MIXED";

            text.Text = modesText;
        }
    }
}