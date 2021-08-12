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
                Checks[item].Visible = Screen.Processor.Value.Windows == JudgementWindowsDatabaseCache.Presets[i];
            }
        }

        private void OnProcessorChanged(object sender, BindableValueChangedEventArgs<ScoreProcessor> e) =>
            SetVisibleCheck();
    }
}