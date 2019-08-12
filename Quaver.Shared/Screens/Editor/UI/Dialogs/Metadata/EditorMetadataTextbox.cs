/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Settings;
using Quaver.Shared.Screens.Settings.Elements;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataTextbox : EditorMetadataItem
    {
        /// <summary>
        /// </summary>
        private Textbox Textbox { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public EditorMetadataTextbox(SettingsDialog dialog, string name) : base(dialog, name, null) => throw new NotImplementedException();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="name"></param>
        /// <param name="initialValue"></param>
        public EditorMetadataTextbox(Drawable sprite, string name, string initialValue, Action<string> saveValue) : base(sprite, name, initialValue, saveValue)
        {
            Textbox = new Textbox(new ScalableVector2(240, Height * 0.70f), Fonts.SourceSansProSemiBold, 13, InitialValue)
            {
                Parent = this,
                X = -10,
                Alignment = Alignment.MidRight,
                Tint = Color.Transparent,
                AllowSubmission = false
            };

            Textbox.AddBorder(Color.LightGray, 2);
        }

        public override string GetValue() => Textbox.RawText;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override bool HasChanged() => InitialValue != Textbox.RawText;
    }
}
