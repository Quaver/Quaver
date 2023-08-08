using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.Preview;
using Quaver.Shared.Screens.Edit.Actions.SV.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch;

namespace Quaver.Shared.Screens.Edit.Actions.Offset
{
    public class EditorActionApplyOffset : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ApplyOffset;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private int Offset { get; }

        public EditorActionApplyOffset(EditorActionManager actiomManager, Qua workingMap, int offset)
        {
            ActionManager = actiomManager;
            WorkingMap = workingMap;
            Offset = offset;
        }

        public void Perform()
        {
            new EditorActionMoveHitObjects(ActionManager, WorkingMap, new List<HitObjectInfo>(WorkingMap.HitObjects), 0,
                Offset).Perform();

            new EditorActionChangeTimingPointOffsetBatch(ActionManager, WorkingMap,
                new List<TimingPointInfo>(WorkingMap.TimingPoints), Offset).Perform();

            new EditorActionChangeScrollVelocityOffsetBatch(ActionManager, WorkingMap, new List<SliderVelocityInfo>(WorkingMap.SliderVelocities),
                Offset).Perform();

            new EditorActionChangePreviewTime(ActionManager, WorkingMap, WorkingMap.SongPreviewTime + Offset).Perform();
            
            new EditorActionChangeBookmarkOffsetBatch(ActionManager, WorkingMap, WorkingMap.Bookmarks, Offset).Perform();
        }

        public void Undo() => new EditorActionApplyOffset(ActionManager, WorkingMap, -Offset).Perform();
    }
}