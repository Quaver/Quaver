using System;
using Quaver.API.Enums;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Deviance
{
    public enum DevianceHighlightType
    {
        None,
        Judgement,
        MineMiss,
        Mean,
        StdDev,
        MeanAndStdDev
    }

    /// <summary>
    ///     Describes what is currently being hovered on the results judgement list or
    ///     the Mean/Std. Dev. stats so the deviance graph can highlight the matching dots/lines.
    /// </summary>
    public readonly struct DevianceHighlight : IEquatable<DevianceHighlight>
    {
        public DevianceHighlightType Type { get; }

        /// <summary>
        ///     Only meaningful when <see cref="Type"/> is <see cref="DevianceHighlightType.Judgement"/>.
        /// </summary>
        public Judgement Judgement { get; }

        private DevianceHighlight(DevianceHighlightType type, Judgement judgement)
        {
            Type = type;
            Judgement = judgement;
        }

        public static readonly DevianceHighlight None = new DevianceHighlight(DevianceHighlightType.None, default);
        public static readonly DevianceHighlight MineMiss = new DevianceHighlight(DevianceHighlightType.MineMiss, default);
        public static readonly DevianceHighlight Mean = new DevianceHighlight(DevianceHighlightType.Mean, default);
        public static readonly DevianceHighlight StdDev = new DevianceHighlight(DevianceHighlightType.StdDev, default);
        public static readonly DevianceHighlight MeanAndStdDev = new DevianceHighlight(DevianceHighlightType.MeanAndStdDev, default);

        public static DevianceHighlight ForJudgement(Judgement judgement) => new DevianceHighlight(DevianceHighlightType.Judgement, judgement);

        public bool Equals(DevianceHighlight other) => Type == other.Type && Judgement == other.Judgement;

        public override bool Equals(object obj) => obj is DevianceHighlight other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int) Type, (int) Judgement);
    }
}
