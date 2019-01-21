/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.Shared.Screens.Settings;
using Quaver.Shared.Screens.Settings.Elements;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public abstract class EditorMetadataItem : SettingsItem
    {
        /// <summary>
        ///     The intiial value of the item upon creation
        /// </summary>
        public string InitialValue { get; }

        /// <summary>
        /// </summary>
        public Action<string> SaveValue { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public EditorMetadataItem(SettingsDialog dialog, string name, Action<string> saveValue) : base(dialog, name) => throw new NotImplementedException();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="name"></param>
        /// <param name="initialValue"></param>
        /// <param name="saveValue"></param>
        public EditorMetadataItem(Drawable sprite, string name, string initialValue, Action<string> saveValue) : base(sprite, name)
        {
            InitialValue = initialValue;
            SaveValue = saveValue;
        }

        public abstract string GetValue();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool HasChanged();
    }
}