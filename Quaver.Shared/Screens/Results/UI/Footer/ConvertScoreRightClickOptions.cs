using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ConvertScoreRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private Dictionary<DropdownItem, Sprite> Checks { get; } = new Dictionary<DropdownItem, Sprite>();

        /// <summary>
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ConvertScoreRightClickOptions(ResultsScreen screen) : base(GetOptions(), new ScalableVector2(200, 40), 22)
        {
            Screen = screen;

            Items.ForEach(x =>
            {
                var originalText = x.Text.Text;
                x.Text.TruncateWithEllipsis((int) (Width * 0.50f));
                Options[x.Text.Text] = Options[originalText];

                var scale = 0.50f;

                // ReSharper disable once ObjectCreationAsStatement
                var check = new Sprite
                {
                    Parent = x,
                    Alignment = Alignment.MidRight,
                    X = -x.Text.X,
                    Size = new ScalableVector2(x.Height * scale, x.Height * scale),
                    UsePreviousSpriteBatchOptions = true,
                    Image = FontAwesome.Get(FontAwesomeIcon.fa_check_symbol),
                    Tint = Options[x.Text.Text],
                    Visible = false
                };

                Checks.Add(x, check);
            });

            SetVisibleCheck();

            ItemSelected += (sender, args) => Screen.ConvertScoreToJudgementWindows(JudgementWindowsDatabaseCache.Presets[args.Index]);
            Screen.Processor.ValueChanged += OnProcessorChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Screen.Processor.ValueChanged -= OnProcessorChanged;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Opened)
                SetVisibleCheck();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions()
        {
            var options = new Dictionary<string, Color>();

            foreach (var preset in JudgementWindowsDatabaseCache.Presets)
            {
                if (!options.ContainsKey(preset.Name))
                    options.Add(preset.Name, Color.White);
            }

            return options;
        }

        /// <summary>
        /// </summary>
        private void SetVisibleCheck()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                Checks[item].Visible = JudgementWindowsMatch(Screen.Processor.Value.Windows, JudgementWindowsDatabaseCache.Presets[i]);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="current"></param>
        /// <param name="preset"></param>
        /// <returns></returns>
        private static bool JudgementWindowsMatch(JudgementWindows current, JudgementWindows preset) =>
            current.Name == preset.Name &&
            current.Marvelous == preset.Marvelous &&
            current.Perfect == preset.Perfect &&
            current.Great == preset.Great &&
            current.Good == preset.Good &&
            current.Okay == preset.Okay &&
            current.Miss == preset.Miss;

        private void OnProcessorChanged(object sender, BindableValueChangedEventArgs<ScoreProcessor> e) =>
            SetVisibleCheck();
    }
}
