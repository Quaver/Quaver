using System;
using Quaver.API.Maps;
using Quaver.Shared.Graphics.Notifications;
using TagLib.Matroska;

namespace Quaver.Shared.Screens.Edit.Actions.Preview
{
    public class EditorActionChangePreviewTime : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangePreviewTime;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public int Time { get; }

        public int OriginalTime { get; }

        public EditorActionChangePreviewTime(EditorActionManager actionManager, Qua workingMap, int time)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;

            OriginalTime = WorkingMap.SongPreviewTime;
            Time = time;
        }

        public void Perform()
        {
            WorkingMap.SongPreviewTime = Time;

            var timeStr = TimeSpan.FromMilliseconds(Time).ToString(@"mm\:ss\.fff");
            NotificationManager.Show(NotificationLevel.Info, $"Preview time changed to: {timeStr}");

            ActionManager.TriggerEvent(Type, new EditorChangedPreviewTimeEventArgs(Time, OriginalTime));
        }

        public void Undo() => new EditorActionChangePreviewTime(ActionManager, WorkingMap, OriginalTime).Perform();
    }
}