using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class BeatSnapRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private BindableInt BeatSnap { get; }

        /// <summary>
        /// </summary>
        private Dictionary<DropdownItem, Sprite> Checks { get; } = new Dictionary<DropdownItem, Sprite>();

        /// <summary>
        /// </summary>
        private List<int> AvailableBeatSnaps { get; }

        /// <summary>
        /// </summary>
        /// <param name="beatSnap"></param>
        /// <param name="availableBeatSnaps"></param>
        public BeatSnapRightClickOptions(BindableInt beatSnap, List<int> availableBeatSnaps)
            : base(GetOptions(availableBeatSnaps), new ScalableVector2(200, 40),
            22)
        {
            BeatSnap = beatSnap;
            AvailableBeatSnaps = availableBeatSnaps;

            Items.ForEach(x =>
            {
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

            ItemSelected += (sender, args) =>
            {
                if (args.Text == "Custom")
                {
                    DialogManager.Show(new CustomBeatSnapDialog(beatSnap, AvailableBeatSnaps));
                    return;
                }

                beatSnap.Value = GetBeatSnapFromText(args.Text);
            };

            BeatSnap.ValueChanged += OnBeatSnapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            BeatSnap.ValueChanged -= OnBeatSnapChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions(List<int> availableSnaps)
        {
            var options = new Dictionary<string, Color>();

            foreach (var snap in availableSnaps)
                options.Add($"1/{StringHelper.AddOrdinal(snap)}", ColorHelper.BeatSnapToColor(snap));

            options.Add("Custom", Color.White);

            return options;
        }

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int GetBeatSnapFromText(string input)
        {
            var r = new Regex(@"1\/(\d+)\w+", RegexOptions.IgnoreCase);
            var match = r.Match(input);

            return int.Parse(match.Groups[1].ToString());
        }

        /// <summary>
        /// </summary>
        private void SetVisibleCheck()
        {
            foreach (var item in Items)
            {
                if (item.Text.Text == "Custom")
                {
                    Checks[item].Visible = false;
                    continue;
                }

                var snap = GetBeatSnapFromText(item.Text.Text);
                Checks[item].Visible = BeatSnap.Value == snap;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeatSnapChanged(object sender, BindableValueChangedEventArgs<int> e) => SetVisibleCheck();
    }
}