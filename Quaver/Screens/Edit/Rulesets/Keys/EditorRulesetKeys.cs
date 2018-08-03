using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.API.Enums;
using Quaver.Screens.Edit.Rulesets.Keys.Playfield;

namespace Quaver.Screens.Edit.Rulesets.Keys
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
