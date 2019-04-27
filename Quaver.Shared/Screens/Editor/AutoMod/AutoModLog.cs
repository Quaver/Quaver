using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.AutoMod
{
    public struct AutoModLog
    {
        /// <summary>
        ///
        /// </summary>
        public AutoModLogType Type { get; }

        /// <summary>
        ///
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///
        /// </summary>
        public DrawableEditorHitObject HitObject { get; }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="hitObject"></param>
        public AutoModLog(AutoModLogType type, string message, DrawableEditorHitObject hitObject)
        {
            Type = type;
            Message = message;
            HitObject = hitObject;
        }

        /// <summary>
        ///     Returns string for Log Info
        /// </summary>
        /// <returns></returns>
        public string GetInfo() => $"{Type}: {Message}, HitObject: {HitObject.GetInfo()}";
    }
}