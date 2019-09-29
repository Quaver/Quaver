using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Create;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create
{
    public class CreatePlaylistContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private CreatePlaylistDialog Dialog { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        ///     Contains the fields to create a new playlist
        /// </summary>
        private Sprite Container { get; set; }

        /// <summary>
        ///     Displays the banner of the playlist
        /// </summary>
        private Sprite Banner { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BannerBlackness { get; set; }

        /// <summary>
        ///     The font size used for labels
        /// </summary>
        private int LabelFontSize { get; } = 22;

        /// <summary>
        ///     The size of the textbox
        /// </summary>
        private int TextboxHeight { get; } = 42;

        /// <summary>
        ///     The amount of spacing between labels and textboxes
        /// </summary>
        private int LabelSpacing { get; } = 6;

        /// <summary>
        ///     The width of each textbox
        /// </summary>
        private int TextboxWidth { get; } = 600;

        /// <summary>
        ///     The font size that each textbox will have
        /// </summary>
        private int TextboxFontSize { get; } = 22;

        /// <summary>
        ///     The amount of y spacing between each textbox
        /// </summary>
        private int TextboxSpacingY { get; } = 26;

        /// <summary>
        ///     Used to handle tabbing through the form
        /// </summary>
        private TextboxTabControl TabControl { get; }

        /// <summary>
        ///     The field to type in the name
        /// </summary>
        private LabelledTextbox Name { get; set; }

        /// <summary>
        ///     The field to type in the playlist description
        /// </summary>
        private LabelledTextbox Description { get; set; }

        /// <summary>
        ///     The button to submit the form and create the playlist
        /// </summary>
        private IconButton CreateButton { get; set; }

        /// <summary>
        ///     The button to cancel the dialog
        /// </summary>
        private IconButton CancelButton { get; set; }

        /// <summary>
        ///     Informs the user that they can drag and drop to add a banner
        /// </summary>
        public SpriteTextPlus DragAndDropText { get; set; }

        /// <summary>
        ///     The path of the custom playlist banner
        /// </summary>
        private string BannerPath { get; set; }

        /// <summary>
        ///     If the banner is currently in the middle of loading
        /// </summary>
        private bool IsBannerLoading { get; set; }

        /// <summary>
        /// </summary>
        public CreatePlaylistContainer(CreatePlaylistDialog dialog)
        {
            Dialog = dialog;

            Alpha = 0f;
            Size = new ScalableVector2(660, 424);

            CreateHeader();
            CreateContainer();
            CreateBanner();

            TabControl = new TextboxTabControl(new List<Textbox>()) {  Parent = this };
            CreateNameTextbox();
            CreateDescriptionTextbox();
            CreateButtonCreate();
            CreateButtonCancel();

            GameBase.Game.Window.FileDropped += OnFileDropped;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            GameBase.Game.Window.FileDropped += OnFileDropped;

            if (Banner.Image != UserInterface.DefaultBanner)
                Banner.Image.Dispose();

            base.Destroy();
        }

        /// <summary>
        ///     Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeader()
        {
            var header = Dialog.Playlist != null ? "Edit Playlist" : "Create New Playlist";

            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), header.ToUpper(), 26)
            {
                Parent = this,
                Alignment = Alignment.TopLeft
            };
        }

        /// <summary>
        ///     Creates <see cref="Container"/>
        /// </summary>
        private void CreateContainer()
        {
            const int spacingY = 5;

            Container = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, Height - Header.Height - spacingY),
                Y = Header.Height + spacingY,
                Tint = ColorHelper.HexToColor("#242424")
            };

            Container.AddBorder(ColorHelper.HexToColor("#0FBAE5"), 2);
        }

        /// <summary>
        ///     Creates <see cref="Banner"/>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(Width - Container.Border.Thickness * 2, 125),
                Y = Container.Border.Thickness,
                Image = UserInterface.DefaultBanner
            };

            BannerBlackness = new Sprite()
            {
                Parent = Banner,
                Size = Banner.Size,
                Tint = Color.Black,
                Alpha = 0.45f
            };

            // ReSharper disable once ObjectCreationAsStatement
            DragAndDropText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "Drag an image into the window...", 24)
            {
                Parent = Banner,
                Alignment = Alignment.MidCenter
            };
        }

        /// <summary>
        ///     Creates <see cref="Name"/>
        /// </summary>
        private void CreateNameTextbox()
        {
            Name = new LabelledTextbox(TextboxWidth, "Name", LabelFontSize, TextboxHeight, TextboxFontSize, LabelSpacing,
                "What would you like your playlist to be called?", Dialog.Playlist != null ? Dialog.Playlist.Name : "")
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = Banner.Height + 16,
                Textbox =
                {
                    AllowSubmission = false
                }
            };

            TabControl.AddTextbox(Name.Textbox);
        }

        /// <summary>
        ///     Creates <see cref="Description"/>
        /// </summary>
        private void CreateDescriptionTextbox()
        {
            Description = new LabelledTextbox(TextboxWidth, "Description", LabelFontSize, TextboxHeight, TextboxFontSize, LabelSpacing,
                "Describe your playlist in a few words", Dialog.Playlist != null ? Dialog.Playlist.Description : "")
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = Name.Y + Name.Height + TextboxSpacingY,
                Textbox =
                {
                    AllowSubmission = false
                }
            };

            TabControl.AddTextbox(Description.Textbox);
        }

        /// <summary>
        ///     Creates <see cref="CreateButton"/>
        /// </summary>
        private void CreateButtonCreate()
        {
            CreateButton = new IconButton(Dialog.Playlist == null ? UserInterface.CreateButton : UserInterface.EditPlayButton, OnSubmit)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Y = -18,
                Size = new ScalableVector2(220, 40),
                X = 100,
                SetChildrenAlpha = true
            };
        }

        /// <summary>
        ///     Called when the form has been submitted to create the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubmit(object sender, EventArgs e)
        {
            // Make the name required
            if (string.IsNullOrEmpty(Name.Textbox.RawText))
            {
                Name.Textbox.Border.ClearAnimations();
                Name.Textbox.Border.Tint = Color.Crimson;
                Name.Textbox.Border.FadeToColor(ColorHelper.HexToColor("#363636"), Easing.Linear, 600);

                return;
            }

            // Add playlist
            if (Dialog.Playlist == null)
            {
                var playlist = new Playlist
                {
                    Name = Name.Textbox.RawText,
                    Creator = ConfigManager.Username.Value ?? "Player",
                    Description = Description.Textbox.RawText
                };

                PlaylistManager.AddPlaylist(playlist, BannerPath);
            }
            // Update playlist
            else
            {
                var playlist = Dialog.Playlist;
                playlist.Name = Name.Textbox.RawText;
                playlist.Description = Description.Textbox.RawText;

                PlaylistManager.EditPlaylist(playlist, BannerPath);
            }

            if (string.IsNullOrEmpty(BannerPath))
            {
                Close();
                return;
            }

            Close();
        }

        /// <summary>
        ///     Creates <see cref="CancelButton"/>
        /// </summary>
        private void CreateButtonCancel()
        {
            CancelButton = new IconButton(UserInterface.CancelButton, (sender, args) =>
            {
                Close();
            })
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Y = CreateButton.Y,
                Size = CreateButton.Size,
                X = -CreateButton.X,
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileDropped(object sender, string e)
        {
            if (IsBannerLoading)
                return;

            if (!e.EndsWith(".png") && !e.EndsWith(".jpg") && !e.EndsWith(".jpeg"))
                return;

            IsBannerLoading = true;
            BannerPath = e;

            BannerBlackness.ClearAnimations();
            BannerBlackness.FadeTo(1, Easing.Linear, 200);

            ThreadScheduler.RunAfter(() =>
            {
                try
                {
                    if (Banner.Image != UserInterface.DefaultBanner)
                        Banner.Image.Dispose();

                    var tex = AssetLoader.LoadTexture2DFromFile(e);
                    Banner.Image = tex;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                }
                finally
                {
                    IsBannerLoading = false;
                    BannerBlackness.ClearAnimations();
                    BannerBlackness.FadeTo(0.45f, Easing.Linear, 200);
                }
            }, 200);
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            Fade();
            Dialog?.Close();
        }

        /// <summary>
        ///     Fades out the entire
        /// </summary>
        private void Fade()
        {
            const Easing easing = Easing.Linear;
            const int time = 150;

            ClearAnimations();
            FadeTo(0, easing, time);

            Header.ClearAnimations();
            Header.FadeTo(0, easing, time);

            Container.ClearAnimations();
            Container.FadeTo(0, easing, time);

            Container.Border.ClearAnimations();
            Container.Border.FadeTo(0, easing, time);

            Banner.ClearAnimations();
            Banner.FadeTo(0, easing, time);

            BannerBlackness.ClearAnimations();
            BannerBlackness.FadeTo(0, easing, time);

            Name.Label.ClearAnimations();
            Name.Label.FadeTo(0, easing, time);

            Name.Textbox.Visible = false;

            Description.Label.ClearAnimations();
            Description.Label.FadeTo(0, easing, time);

            Description.Textbox.Visible = false;

            CreateButton.IsPerformingFadeAnimations = false;
            CreateButton.ClearAnimations();
            CreateButton.FadeTo(0, easing, time);

            CancelButton.IsPerformingFadeAnimations = false;
            CancelButton.ClearAnimations();
            CancelButton.FadeTo(0, easing, time);

            DragAndDropText.ClearAnimations();
            DragAndDropText.FadeTo(0, easing, time);
        }
    }
}