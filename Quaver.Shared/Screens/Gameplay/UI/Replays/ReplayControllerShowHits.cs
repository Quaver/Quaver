using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Replays
{
    public class ReplayControllerShowHits : Sprite
    {
        /// <summary>
        /// </summary>
        private HitObjectManagerKeys Manager { get; }

        /// <summary>
        /// </summary>
        private Checkbox Checkbox { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="size"></param>
        public ReplayControllerShowHits(GameplayScreen screen, ScalableVector2 size)
        {
            Manager = (HitObjectManagerKeys) screen.Ruleset.HitObjectManager;

            Image = UserInterface.ReplayControllerSpeedPanel;
            Size = size;

            Checkbox = new Checkbox(Size)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Name = {Text = "Show Hits"},
                Sprite = {Image = FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty)},
            };

            Checkbox.Button.Clicked += (sender, args) =>
            {
                Manager.ShowHits = !Manager.ShowHits;
                Checkbox.Sprite.Image = FontAwesome.Get(Manager.ShowHits ? FontAwesomeIcon.fa_check : FontAwesomeIcon.fa_check_box_empty);
            };
        }
    }
}