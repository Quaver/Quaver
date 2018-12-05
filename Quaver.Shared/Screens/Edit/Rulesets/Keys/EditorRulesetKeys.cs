/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;
using Quaver.Shared.Screens.Edit.Rulesets.Keys.Playfield;

namespace Quaver.Shared.Screens.Edit.Rulesets.Keys
{
    public class EditorRulesetKeys : EditorRuleset
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="mode"></param>
        public EditorRulesetKeys(EditorScreen screen, GameMode mode) : base(screen, mode)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override IEditorPlayfield CreatePlayfield() => new EditorPlayfieldKeys(Screen, Mode);
    }
}
