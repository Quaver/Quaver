/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Rulesets
{
    public interface IEditorPlayfield : IGameScreenComponent
    {
        /// <summary>
        ///     The actual editor score we're on.
        /// </summary>
        EditorScreen Screen { get; }

        /// <summary>
        ///     Container for the entire playfield/
        /// </summary>
        Container Container { get; }

        /// <summary>
        ///     The game mode this playfield is for.
        /// </summary>
        GameMode Mode { get; }
    }
}
