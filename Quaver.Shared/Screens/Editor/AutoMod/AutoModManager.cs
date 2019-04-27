using System.Collections.Generic;
using ManagedBass;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.AutoMod
{
    public class AutoModManager
    {
        /// <summary>
        ///
        /// </summary>
        public List<AutoModLog> Log { get; } = new List<AutoModLog>();

        /// <summary>
        /// Add a log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="h"></param>
        public void AddLog(AutoModLogType type, string message, DrawableEditorHitObject h)
        {
            Log.Add(new AutoModLog(type, message, h));
        }

        /// <summary>
        /// Remove logs containing specific hit object
        /// </summary>
        /// <param name="h"></param>
        public void RemoveLog(DrawableEditorHitObject h)
        {
            // Look for and remove log containing hit object
            for (var i = 0; i < Log.Count; i++)
            {
                if (Log[i].HitObject == h)
                {
                    Log.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Remove a specific log
        /// </summary>
        /// <param name="log"></param>
        public void Remove(AutoModLog log)
        {
            Log.Remove(log);
        }
    }
}