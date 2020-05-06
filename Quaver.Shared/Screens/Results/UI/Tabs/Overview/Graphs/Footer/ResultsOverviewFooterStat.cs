using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using YamlDotNet.Core.Tokens;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Footer
{
    public class ResultsOverviewFooterStat : Container
    {
        /// <summary>
        /// </summary>
        private string HeaderText { get; }

        /// <summary>
        /// </summary>
        private string ValueText { get; }

        /// <summary>
        /// </summary>
        private FooterStatChangeType Type { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Value { get; set; }

        /// <summary>
        /// </summary>
        private const int SPACING_Y = 6;

        /// <summary>
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public ResultsOverviewFooterStat(string header, string value, FooterStatChangeType type = FooterStatChangeType.NoChange)
        {
            HeaderText = header;
            ValueText = value;
            Type = type;

            CreateHeader();
            CreateValue();

            Size = new ScalableVector2(Math.Max(Header.Width, Value.Width), Header.Height + SPACING_Y + Value.Height);
        }

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            HeaderText, 21)
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Tint = ColorHelper.HexToColor("#FFE76B")
        };

        /// <summary>
        /// </summary>
        private void CreateValue() => Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            ValueText, 22)
        {
            Parent = this,
            Y = Header.Y + Header.Height + SPACING_Y,
            Alignment = Alignment.TopCenter
        };

        public void FadeIn(int time = 250)
        {
            Header.Alpha = 0;
            Value.Alpha = 0;

            Header.FadeTo(1, Easing.Linear, time);
            Value.FadeTo(1, Easing.Linear, time);
        }

        public static FooterStatChangeType GetChangeType(double oldValue, double newValue)
        {
            if (newValue > oldValue)
                return FooterStatChangeType.Up;

            if (oldValue > newValue)
                return FooterStatChangeType.Down;

            return FooterStatChangeType.NoChange;
        }
    }

    public enum FooterStatChangeType
    {
        Up,
        Down,
        NoChange
    }
}