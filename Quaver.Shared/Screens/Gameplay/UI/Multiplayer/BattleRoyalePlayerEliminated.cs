using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Multiplayer
{
    public class BattleRoyalePlayerEliminated : Container
    {
        public SpriteTextPlus Username { get; }

        public SpriteTextPlus Eliminated { get; }

        private GameplayScreen Screen { get; }

        public BattleRoyalePlayerEliminated(GameplayScreen screen)
        {
            Screen = screen;

            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), " ", 0, false)
            {
                Parent = this,
                Tint = Color.Crimson,
                FontSize = 20,
                Alpha = 0
            };

            Eliminated = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), " has been eliminated!", 0, false)
            {
                Parent = this,
                FontSize = 20,
                X = Username.Width + 1,
                Alpha = 0
            };

            Size = new ScalableVector2(Username.Width + Eliminated.Width + 1, Eliminated.Height);

            OnlineManager.Client.OnPlayerBattleRoyaleEliminated += OnBattleRoyalePlayerEliminated;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnPlayerBattleRoyaleEliminated -= OnBattleRoyalePlayerEliminated;
            base.Destroy();
        }

        private void OnBattleRoyalePlayerEliminated(object sender, PlayerBattleRoyaleEliminatedEventArgs e)
        {
            var name = "Player";

            if (e.UserId == OnlineManager.Self.OnlineUser.Id)
                name = OnlineManager.Self.OnlineUser.Username;
            else
            {
                var view = (GameplayScreenView) Screen.View;
                var user = view.ScoreboardLeft.Users.Find(x => x.LocalScore?.PlayerId == e.UserId);

                if (user != null)
                    name = user.UsernameRaw;
            }

            Username.Text = name;
            Eliminated.X = Username.Width + 1;
            Size = new ScalableVector2(Username.Width + Eliminated.Width + 1, Eliminated.Height);

            Username.ClearAnimations();
            Eliminated.ClearAnimations();

            Username.FadeTo(1, Easing.Linear, 300).Wait(1200);
            Eliminated.FadeTo(1, Easing.Linear, 300).Wait(1200);
            Username.FadeTo(0, Easing.Linear, 500);
            Eliminated.FadeTo(0, Easing.Linear, 500);
        }
    }
}
