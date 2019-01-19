using System;
using Quaver.API.Maps;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics.Notifications;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionSetPreviewTime : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.SetPreviewTime;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private int PreviousPreviewTime { get; }

        /// <summary>
        /// </summary>
        private int Time { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="time"></param>
        public EditorActionSetPreviewTime(Qua workingMap, int time)
        {
            WorkingMap = workingMap;
            PreviousPreviewTime = WorkingMap.SongPreviewTime;
            Time = time;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            WorkingMap.SongPreviewTime = Time;

            var formattedTime = TimeSpan.FromMilliseconds(Time).ToString(@"mm\:ss\.fff");
            NotificationManager.Show(NotificationLevel.Info, $"Set new song preview time to: {formattedTime}");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            WorkingMap.SongPreviewTime = PreviousPreviewTime;

            var formattedTime = TimeSpan.FromMilliseconds(PreviousPreviewTime).ToString(@"mm\:ss\.fff");
            NotificationManager.Show(NotificationLevel.Info, $"Set song preview time back to: {formattedTime}");
        }
    }
}