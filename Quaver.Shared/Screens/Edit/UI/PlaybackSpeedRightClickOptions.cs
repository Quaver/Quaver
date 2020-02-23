using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI
{
    public class PlaybackSpeedRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private Dictionary<DropdownItem, Sprite> Checks { get; } = new Dictionary<DropdownItem, Sprite>();

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        public PlaybackSpeedRightClickOptions(IAudioTrack track) : base(GetOptions(), new ScalableVector2(200, 40), 22)
        {
            Track = track;

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
                var speed = int.Parse(args.Text.Replace("%", "")) / 100f;

                if (track.IsDisposed)
                    return;

                track.Rate = speed;
            };

            Track.RateChanged += OnRateChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Track.RateChanged -= OnRateChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions()
        {
            var options = new Dictionary<string, Color>();

            for (var i = 0; i < 8; i++)
            {
                var val = (i + 1) * 0.25f;
                options.Add($"{val * 100}%", Color.White);
            }

            return options;
        }

        /// <summary>
        /// </summary>
        private void SetVisibleCheck()
        {
            foreach (var item in Items)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Checks[item].Visible = Track.Rate == int.Parse(item.Text.Text.Replace("%", "")) / 100f;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRateChanged(object sender, TrackRateChangedEventArgs e) => SetVisibleCheck();
    }
}