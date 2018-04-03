namespace Quaver.States.Gameplay.Mania
{
    /// <summary>
    ///     This class holds any reference variables for the gameplay state.
    ///     Most of these variables will be gone later when a better flow is implemented.
    /// </summary>
    internal static class ManiaGameplayReferences
    {
        internal static string[] JudgeNames { get; } = new string[6] { "Marvelous", "Perfect", "Great", "Good", "Okay", "Miss" };

        //todo: temp variables for scoremanager
        //internal static float PressWindowLatest { get; set; } // HitWindowPress[4]
        //internal static float ReleaseWindowLatest { get; set; } // HitWindowRelease[3]

        //todo: temp ManiaPlayfield variables
        //internal static float PlayfieldSize { get; set; } //todo: remove
        //internal static float PlayfieldLaneSize { get; set; } //todo: remove
        internal static float[] ReceptorXPosition { get; set; } //used more than once
        //internal static float ReceptorYOffset { get; set; } //used more than once
    }
}
