/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Hitsounds
{
    public class EditorHitsoundsPanel : Sprite
    {
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ClearButton { get; set; }

        /// <summary>
        /// </summary>
        public Dictionary<HitSounds, DrawableHitsound> Hitsounds { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<HitSounds> SelectedObjectHitsounds { get; } = new Bindable<HitSounds>(0);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorHitsoundsPanel(EditorScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorHitsoundsPanel;

            CreateHeader();
            CreateHitsounds();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            HeaderBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 38),
                Tint = Color.Transparent,
            };

            // CreateClearButton();
        }

        /// <summary>
        /// </summary>
        private void CreateClearButton() => ClearButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_double_sided_eraser),
            (sender, args) =>
            {
                var ruleset = (EditorRulesetKeys) Screen.Ruleset;

                if (ruleset.SelectedHitObjects.Count == 0)
                    return;

                ruleset.ActionManager.Perform(new EditorActionRemoveHitsoundKeys(ruleset,
                    new List<DrawableEditorHitObject>(ruleset.SelectedHitObjects), 0));

            })
        {
            Parent = HeaderBackground,
            Alignment = Alignment.MidRight,
            Size = new ScalableVector2(20, 20),
            Tint = ColorHelper.HexToColor("#dbb7bb"),
            X = -8
        };

        /// <summary>
        /// </summary>
        private void CreateHitsounds()
        {
            Hitsounds = new Dictionary<HitSounds, DrawableHitsound>
            {
                {HitSounds.Whistle, new DrawableHitsound(this, HitSounds.Whistle, FontAwesome.Get(FontAwesomeIcon.fa_white_flag_symbol), "Whistle")},
                {HitSounds.Finish, new DrawableHitsound(this, HitSounds.Finish, FontAwesome.Get(FontAwesomeIcon.fa_bell_musical_tool), "Finish")},
                {HitSounds.Clap, new DrawableHitsound(this, HitSounds.Clap, FontAwesome.Get(FontAwesomeIcon.fa_hand_pointing_upward), "Clap")}
            };

            var i = 0;

            foreach (var h in Hitsounds)
            {
                h.Value.Parent = this;
                h.Value.Y = HeaderBackground.Height + h.Value.Height * i;

                i++;
            }
        }
    }
}