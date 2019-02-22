using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Skinning;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionFlipObjectsHorizontallyKeys : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.FlipHitObjectsHorizontally;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get;  }

        /// <summary>
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        /// </summary>
        private List<DrawableEditorHitObject> HitObjects { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="container"></param>
        /// <param name="hitObjects"></param>
        public EditorActionFlipObjectsHorizontallyKeys(Qua workingMap, EditorScrollContainerKeys container, List<DrawableEditorHitObject> hitObjects)
        {
            WorkingMap = workingMap;
            Container = container;
            HitObjects = hitObjects;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            var skin = SkinManager.Skin.Keys[WorkingMap.Mode];

            foreach (var h in HitObjects)
            {
                h.Info.Lane = WorkingMap.GetKeyCount() - h.Info.Lane + 1;
                h.X = Container.ScreenRectangle.X + Container.LaneSize * (h.Info.Lane - 1) + Container.DividerLineWidth;

                var index = skin.ColorObjectsBySnapDistance ? HitObjectManager.GetBeatSnap(h.Info, h.Info.GetTimingPoint(WorkingMap.TimingPoints)) : 0;

                if (h.Info.IsLongNote)
                {
                    h.Image = skin.NoteHoldHitObjects[h.Info.Lane - 1][index];

                    var ln = (DrawableEditorHitObjectLong) h;
                    ln.Body.Image = skin.NoteHoldBodies[h.Info.Lane - 1].First();
                    ln.Tail.Image = skin.NoteHoldEnds[h.Info.Lane - 1];
                    ln.ResizeLongNote();
                }
                else
                    h.Image = skin.NoteHitObjects[h.Info.Lane - 1][index];
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     It's the same logic to flip it, so just perform.
        /// </summary>
        public void Undo() => Perform();
    }
}