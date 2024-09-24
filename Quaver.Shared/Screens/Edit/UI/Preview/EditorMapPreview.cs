using Quaver.API.Maps;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resnap;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Reverse;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Swap;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.ChangeMultiplierBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpmBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeHidden;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignature;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignatureBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Create;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Preview;
using Wobble.Audio.Tracks;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Edit.UI.Preview
{
    public class EditorMapPreview : SelectMapPreviewContainer
    {
        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="isPlayTesting"></param>
        /// <param name="activeLeftPanel"></param>
        /// <param name="height"></param>
        /// <param name="track"></param>
        /// <param name="qua"></param>
        public EditorMapPreview(EditorActionManager manager, Bindable<bool> isPlayTesting, Bindable<SelectContainerPanel> activeLeftPanel,
            int height, IAudioTrack track = null, Qua qua = null) : base(isPlayTesting, activeLeftPanel, height, track, qua)
        {
            HasSeekBar = false;
            DelayTime = 650;
            ActionManager = manager;

            ActionManager.HitObjectPlaced += OnHitObjectPlaced;
            ActionManager.HitObjectRemoved += OnHitObjectRemoved;
            ActionManager.HitObjectsMoved += OnHitObjectsMoved;
            ActionManager.HitObjectsFlipped += OnHitObjectsFlipped;
            ActionManager.LanesSwapped += OnLanesSwapped;
            ActionManager.HitObjectsReversed += OnHitObjectsReversed;
            ActionManager.HitObjectBatchPlaced += OnHitObjectBatchPlaced;
            ActionManager.HitObjectBatchRemoved += OnHitObjectBatchRemoved;
            ActionManager.HitObjectsResnapped += OnHitObjectsResnapped;
            ActionManager.LongNoteResized += OnLongNoteResized;
            ActionManager.ScrollVelocityAdded += OnScrollVelocityAdded;
            ActionManager.ScrollVelocityRemoved += OnScrollVelocityRemoved;
            ActionManager.ScrollVelocityBatchAdded += OnScrollVelocityBatchAdded;
            ActionManager.ScrollVelocityBatchRemoved += OnScrollVelocityBatchRemoved;
            ActionManager.ScrollVelocityOffsetBatchChanged += OnScrollVelocityOffsetBatchChanged;
            ActionManager.ScrollVelocityMultiplierBatchChanged += OnScrollVelocityMultiplierBatchChanged;
            ActionManager.TimingPointAdded += OnTimingPointAdded;
            ActionManager.TimingPointRemoved += OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded += OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved += OnTimingPointBatchRemoved;
            ActionManager.TimingPointOffsetChanged += OnTimingPointOffsetChanged;
            ActionManager.TimingPointBpmChanged += OnTimingPointBpmChanged;
            ActionManager.TimingPointBpmBatchChanged += OnTimingPointBpmBatchChanged;
            ActionManager.TimingPointSignatureChanged += OnTimingPointSignatureChanged;
            ActionManager.TimingPointSignatureBatchChanged += OnTimingPointSignatureBatchChanged;
            ActionManager.TimingPointOffsetBatchChanged += OnTimingPointOffsetBatchChanged;
            ActionManager.TimingPointHiddenChanged += OnTimingPointHiddenChanged;
            ActionManager.TimingGroupDeleted += OnTimingGroupDeleted;
            ActionManager.TimingGroupCreated += OnTimingGroupCreated;
            ActionManager.TimingGroupRenamed += OnTimingGroupRenamed;
            ActionManager.HitObjectsMovedToTimingGroup += OnHitObjectsMovedToTimingGroup;
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
            ActionManager.HitObjectsReversed -= OnHitObjectsReversed;
            ActionManager.HitObjectBatchPlaced -= OnHitObjectBatchPlaced;
            ActionManager.HitObjectBatchRemoved -= OnHitObjectBatchRemoved;
            ActionManager.HitObjectsResnapped -= OnHitObjectsResnapped;
            ActionManager.LongNoteResized -= OnLongNoteResized;
            ActionManager.ScrollVelocityAdded -= OnScrollVelocityAdded;
            ActionManager.ScrollVelocityRemoved -= OnScrollVelocityRemoved;
            ActionManager.ScrollVelocityBatchAdded -= OnScrollVelocityBatchAdded;
            ActionManager.ScrollVelocityBatchRemoved -= OnScrollVelocityBatchRemoved;
            ActionManager.ScrollVelocityOffsetBatchChanged -= OnScrollVelocityOffsetBatchChanged;
            ActionManager.ScrollVelocityMultiplierBatchChanged -= OnScrollVelocityMultiplierBatchChanged;
            ActionManager.TimingPointAdded -= OnTimingPointAdded;
            ActionManager.TimingPointRemoved -= OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded -= OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved -= OnTimingPointBatchRemoved;
            ActionManager.TimingPointOffsetChanged -= OnTimingPointOffsetChanged;
            ActionManager.TimingPointBpmChanged -= OnTimingPointBpmChanged;
            ActionManager.TimingPointBpmBatchChanged -= OnTimingPointBpmBatchChanged;
            ActionManager.TimingPointSignatureChanged -= OnTimingPointSignatureChanged;
            ActionManager.TimingPointSignatureBatchChanged -= OnTimingPointSignatureBatchChanged;
            ActionManager.TimingPointOffsetBatchChanged -= OnTimingPointOffsetBatchChanged;
            ActionManager.TimingPointHiddenChanged -= OnTimingPointHiddenChanged;
            ActionManager.TimingGroupDeleted -= OnTimingGroupDeleted;
            ActionManager.TimingGroupCreated -= OnTimingGroupCreated;
            ActionManager.TimingGroupRenamed -= OnTimingGroupRenamed;
            ActionManager.HitObjectsMovedToTimingGroup -= OnHitObjectsMovedToTimingGroup;

            base.Destroy();
        }

        public void Refresh()
        {
            if (LoadedGameplayScreen != null)
            {
                LoadedGameplayScreen.Ruleset.Playfield.Container.Parent = null;
                LoadedGameplayScreen.Destroy();
            }

            RunLoadTask();
        }

        private void OnHitObjectPlaced(object sender, EditorHitObjectPlacedEventArgs e) => Refresh();

        private void OnHitObjectRemoved(object sender, EditorHitObjectRemovedEventArgs e) => Refresh();

        private void OnHitObjectsMoved(object sender, EditorHitObjectsMovedEventArgs e) => Refresh();

        private void OnHitObjectsFlipped(object sender, EditorHitObjectsFlippedEventArgs e) => Refresh();

        private void OnLanesSwapped(object sender, EditorLanesSwappedEventArgs e) => Refresh();

        private void OnHitObjectsReversed(object sender, EditorHitObjectsReversedEventArgs e) => Refresh();

        private void OnHitObjectBatchPlaced(object sender, EditorHitObjectBatchPlacedEventArgs e) => Refresh();

        private void OnHitObjectBatchRemoved(object sender, EditorHitObjectBatchRemovedEventArgs e) => Refresh();

        private void OnHitObjectsResnapped(object sender, EditorActionHitObjectsResnappedEventArgs e) => Refresh();

        private void OnLongNoteResized(object sender, EditorLongNoteResizedEventArgs e) => Refresh();

        private void OnScrollVelocityBatchRemoved(object sender, EditorScrollVelocityBatchRemovedEventArgs e) => Refresh();

        private void OnScrollVelocityBatchAdded(object sender, EditorScrollVelocityBatchAddedEventArgs e) => Refresh();

        private void OnScrollVelocityRemoved(object sender, EditorScrollVelocityRemovedEventArgs e) => Refresh();

        private void OnScrollVelocityAdded(object sender, EditorScrollVelocityAddedEventArgs e) => Refresh();

        private void OnTimingPointBatchRemoved(object sender, EditorTimingPointBatchRemovedEventArgs e) => Refresh();

        private void OnTimingPointBatchAdded(object sender, EditorTimingPointBatchAddedEventArgs e) => Refresh();

        private void OnTimingPointRemoved(object sender, EditorTimingPointAddedEventArgs e) => Refresh();

        private void OnTimingPointAdded(object sender, EditorTimingPointAddedEventArgs e) => Refresh();

        private void OnTimingPointOffsetChanged(object sender, EditorTimingPointOffsetChangedEventArgs e) => Refresh();

        private void OnTimingPointBpmBatchChanged(object sender, EditorChangedTimingPointBpmBatchEventArgs e) => Refresh();

        private void OnTimingPointBpmChanged(object sender, EditorTimingPointBpmChangedEventArgs e) => Refresh();

        private void OnTimingPointSignatureBatchChanged(object sender, EditorChangedTimingPointSignatureBatchEventArgs e) => Refresh();

        private void OnTimingPointSignatureChanged(object sender, EditorTimingPointSignatureChangedEventArgs e) => Refresh();

        private void OnTimingPointOffsetBatchChanged(object sender, EditorChangedTimingPointOffsetBatchEventArgs e) => Refresh();

        private void OnTimingPointHiddenChanged(object sender, EditorTimingPointHiddenChangedEventArgs e) => Refresh();

        private void OnScrollVelocityOffsetBatchChanged(object sender, EditorChangedScrollVelocityOffsetBatchEventArgs e) => Refresh();

        private void OnScrollVelocityMultiplierBatchChanged(object sender, EditorChangedScrollVelocityMultiplierBatchEventArgs e) => Refresh();

        private void OnTimingGroupDeleted(object sender, EditorTimingGroupRemovedEventArgs e) => Refresh();

        private void OnTimingGroupCreated(object sender, EditorTimingGroupCreatedEventArgs e) => Refresh();

        private void OnTimingGroupRenamed(object sender, EditorTimingGroupRenamedEventArgs e) => Refresh();

        private void OnHitObjectsMovedToTimingGroup(object sender, EditorMoveObjectsToTimingGroupEventArgs e) => Refresh();
    }
}