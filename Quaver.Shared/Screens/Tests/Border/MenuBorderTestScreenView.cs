using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Border
{
    public class MenuBorderTestScreenView : ScreenView
    {
        private MenuBorder Footer { get; }

        public MenuBorderTestScreenView(Screen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new MenuBorder(MenuBorderType.Header, new List<Drawable>
                {
                    new MenuBorderLogo(),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_home), FontManager.GetWobbleFont(Fonts.LatoBlack),"Home"),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive), FontManager.GetWobbleFont(Fonts.LatoBlack),"Download"),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_speech_bubbles_comment_option), FontManager.GetWobbleFont(Fonts.LatoBlack),"Community Chat"),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_bug), FontManager.GetWobbleFont(Fonts.LatoBlack),"Report Bugs"),
                },
                new List<Drawable>
                {
                    new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_reorder_option)) { Size = new ScalableVector2(30, 30)},
                    new MenuBorderUser(),
                    new DrawableSessionTime()
                })
            {
                Parent = Container,
                Alignment = Alignment.TopLeft
            };

            // ReSharper disable once ObjectCreationAsStatement
            Footer = new MenuBorder(MenuBorderType.Footer, new List<Drawable>
            {
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left), FontManager.GetWobbleFont(Fonts.LatoBlack),"Back"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_settings), FontManager.GetWobbleFont(Fonts.LatoBlack),"Options"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_open_folder), FontManager.GetWobbleFont(Fonts.LatoBlack),"Create Playlist"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_archive_black_box), FontManager.GetWobbleFont(Fonts.LatoBlack),"Export"),
            },
                new List<Drawable>()
                {
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_play_button), FontManager.GetWobbleFont(Fonts.LatoBlack),"Play"),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette), FontManager.GetWobbleFont(Fonts.LatoBlack),"Modifiers"),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil), FontManager.GetWobbleFont(Fonts.LatoBlack),"Edit"),
                    new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_exchange_arrows), FontManager.GetWobbleFont(Fonts.LatoBlack),"Random"),
                })
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
            {
                Footer.Y = Footer.Height + 6;
                Footer.ClearAnimations();
                Footer.MoveToY(0, Easing.OutQuint, 600);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.S))
                Footer.AnimatedLine.Visible = !Footer.AnimatedLine.Visible;

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}
