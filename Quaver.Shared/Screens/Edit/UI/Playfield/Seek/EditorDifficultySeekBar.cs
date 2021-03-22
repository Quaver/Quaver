using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Graphics.Graphs;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize;
using Wobble.Audio.Tracks;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Seek
{
    public class EditorDifficultySeekBar : DifficultySeekBar
    {
        private EditorActionManager ActionManager { get; }

        public EditorDifficultySeekBar(EditorActionManager actionManager, Qua map, ModIdentifier mods, ScalableVector2 size,
            int maxBars = 120, int barSize = 3, IAudioTrack track = null, bool alignRightToLeft = false, float barWidthScale = 1)
            : base(map, mods, size, maxBars, barSize, track, alignRightToLeft, barWidthScale)
        {
            ActionManager = actionManager;
            ActionManager.HitObjectPlaced += OnHitObjectPlaced;
            ActionManager.HitObjectRemoved += OnHitObjectRemoved;
            ActionManager.HitObjectsMoved += OnHitObjectsMoved;
            ActionManager.HitObjectsFlipped += OnHitObjectsFlipped;
            ActionManager.HitObjectBatchPlaced += OnHitObjectBatchPlaced;
            ActionManager.HitObjectBatchRemoved += OnHitObjectBatchRemoved;
            ActionManager.LongNoteResized += OnLongNoteResized;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ActionManager.HitObjectPlaced -= OnHitObjectPlaced;
            ActionManager.HitObjectRemoved -= OnHitObjectRemoved;
            ActionManager.HitObjectsMoved -= OnHitObjectsMoved;
            ActionManager.HitObjectsFlipped -= OnHitObjectsFlipped;
            ActionManager.HitObjectBatchPlaced -= OnHitObjectBatchPlaced;
            ActionManager.HitObjectBatchRemoved -= OnHitObjectBatchRemoved;
            ActionManager.LongNoteResized -= OnLongNoteResized;
            base.Destroy();
        }

        public void Refresh() => CreateBars();

        private void OnHitObjectPlaced(object sender, EditorHitObjectPlacedEventArgs e) => Refresh();

        private void OnHitObjectRemoved(object sender, EditorHitObjectRemovedEventArgs e) => Refresh();

        private void OnHitObjectsMoved(object sender, EditorHitObjectsMovedEventArgs e) => Refresh();

        private void OnHitObjectsFlipped(object sender, EditorHitObjectsFlippedEventArgs e) => Refresh();

        private void OnHitObjectBatchPlaced(object sender, EditorHitObjectBatchPlacedEventArgs e) => Refresh();

        private void OnHitObjectBatchRemoved(object sender, EditorHitObjectBatchRemovedEventArgs e) => Refresh();

        private void OnLongNoteResized(object sender, EditorLongNoteResizedEventArgs e) => Refresh();
    }
}