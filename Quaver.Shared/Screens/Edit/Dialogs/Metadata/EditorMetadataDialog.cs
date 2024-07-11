using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Edit.Dialogs.Metadata
{
    public class EditorMetadataDialog : YesNoDialog
    {
        private EditScreen Screen { get; }

        private Qua WorkingMap => Screen.WorkingMap;

        private LabelledTextbox Artist { get; set; }

        private LabelledTextbox Title { get; set; }

        private LabelledTextbox Creator { get; set; }

        private LabelledTextbox DifficultyName { get; set; }

        private LabelledTextbox Source { get; set; }

        private LabelledTextbox Tags { get; set; }

        private LabelledTextbox KeyCount { get; set; }

        private LabelledCheckbox ScratchKey { get; set; }

        private Tooltip CurrentToolTip { get; set; }

        private LabelledCheckbox BpmAffectsScrollVelocity { get; set; }

        private Tooltip BpmAffectsScrollVelocityTooltip { get; } = new(
            "When set to ON, A BPM change will also change the Scroll Velocity.\nFor instance, going from 120BPM to 240BPM effectively creates\nan implicit 2x Scroll Velocity multiplier starting from the beginning\nof that BPM change until the next. Scroll Velocity is still applied.",
            Color.White
        );

        private LabelledCheckbox LegacyLNRendering { get; set; }

        private Tooltip LegacyLNRenderingTooltip { get; } = new(
            "When set to ON, forces the use of the old LN renderer. If your map has no Scroll\nVelocity changes, you can safely ignore this option. The current LN renderer places\nthe head and tail to the earliest and latest positions reached, respectively. The old LN\nrenderer instead places them both wherever the playfield happens to be at the time.",
            Color.White
        );

        private TextboxTabControl TabControl { get; }

        private const int LabelSize = 21;

        private const int TextboxHeight = 42;

        private const int TextboxLabelSpacing = 12;

        private const int Spacing = 23;

        public EditorMetadataDialog(EditScreen screen) : base("EDIT METADATA", "Edit the values to change the metadata...")
        {
            Screen = screen;
            Panel.Image = UserInterface.MetadataPanel;

            CreateArtistTextbox();
            CreateSongTitleTextbox();
            CreateDifficultyNameTextbox();
            CreateSourceTextbox();
            CreateTagsTextbox();
            CreateCreatorTextbox();
            CreateKeyCountTextbox();
            CreateBpmAffectsSvCheckbox();
            CreateScratchKeyCheckbox();
            CreateLegacyLNRenderingCheckbox();

            TabControl = new TextboxTabControl(new List<Textbox>()
            {
                Artist.Textbox,
                Title.Textbox,
                DifficultyName.Textbox,
                Source.Textbox,
                Tags.Textbox,
                Creator.Textbox,
                KeyCount.Textbox
            })
            {
                Parent = this,
            };

            const int Spread = 25;
            YesButton.X -= Spread;
            NoButton.X += Spread;

            YesButton.Y += 32;
            NoButton.Y = YesButton.Y;

            Panel.Height = Panel.Children.Max(x => x.Y + x.Height) + YesButton.Height + 40;

            YesAction = OnClickedYes;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!TryActivate(BpmAffectsScrollVelocityTooltip, BpmAffectsScrollVelocity) &&
                !TryActivate(LegacyLNRenderingTooltip, LegacyLNRendering))
                DeactivateTooltip();
        }

        private void CreateArtistTextbox()
        {
            Artist = new LabelledTextbox(Panel.Width * 0.9f * 0.95f * 0.5f, "Artist Name", LabelSize, TextboxHeight,
                LabelSize, TextboxLabelSpacing, "Name of the artist", WorkingMap.Artist)
            {
                Parent = Panel,
                Y = Banner.Height + 20,
                X = Panel.Width * 0.05f,
                Alignment = Alignment.TopLeft,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateSongTitleTextbox()
        {
            Title = new LabelledTextbox(Artist.Width, "Song Title", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Title of the song", WorkingMap.Title)
            {
                Parent = Panel,
                Y = Artist.Y,
                X = -Artist.X,
                Alignment = Alignment.TopRight,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateDifficultyNameTextbox()
        {
            DifficultyName = new LabelledTextbox(Artist.Width, "Difficulty Name", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Name of the difficulty", WorkingMap.DifficultyName)
            {
                Parent = Panel,
                Y = Artist.Y + Artist.Height + Spacing,
                X = Artist.X,
                Alignment = Alignment.TopLeft,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateSourceTextbox()
        {
            Source = new LabelledTextbox(Artist.Width, "Source", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Source of the song (album, mixtape, etc)",
                WorkingMap.Source)
            {
                Parent = Panel,
                Y = DifficultyName.Y,
                X = -DifficultyName.X,
                Alignment = Alignment.TopRight,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateTagsTextbox()
        {
            Tags = new LabelledTextbox(Panel.Width * 0.9f, "Tags", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Comma-separated tags to help players find your map",
                WorkingMap.Tags)
            {
                Parent = Panel,
                Y = DifficultyName.Y + DifficultyName.Height + Spacing,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateCreatorTextbox()
        {
            Creator = new LabelledTextbox(Artist.Width, "Creator Username", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Your username", WorkingMap.Creator)
            {
                Parent = Panel,
                Y = Tags.Y + Tags.Height + Spacing,
                X = Artist.X,
                Alignment = Alignment.TopLeft,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateKeyCountTextbox()
        {
            var regex = "";
            for (var i = 1; i <= ModeHelper.MaxKeyCount; i++)
            {
                regex += $"^{i}$";
                if (i != ModeHelper.MaxKeyCount)
                    regex += "|";
            }

            KeyCount = new LabelledTextbox(Artist.Width, "Key Count", LabelSize, TextboxHeight, LabelSize, 
            TextboxLabelSpacing, $"1-{ModeHelper.MaxKeyCount}", ModeHelper.ToKeyCount(WorkingMap.Mode).ToString())
            {
                Parent = Panel,
                Y = Creator.Y,
                X = -Creator.X,
                Alignment = Alignment.TopRight,
                Tint = Color.Transparent,
                Textbox =
                {
                    AllowSubmission = false,
                    AllowedCharacters = new Regex(regex),
                }
            };
        }

        private void CreateBpmAffectsSvCheckbox()
        {
            BpmAffectsScrollVelocity = new LabelledCheckbox("BPM AFFECTS SV:", 20,
                new QuaverCheckbox(new Bindable<bool>(!WorkingMap.BPMDoesNotAffectScrollVelocity)) { DisposeBindableOnDestroy = true })
            {
                Parent = Panel,
                Y = KeyCount.Y + KeyCount.Height + Spacing,
                X = -(Artist.X + KeyCount.Width + (Panel.Width * 0.05f)),
                Alignment = Alignment.TopRight,
            };
        }

        private void CreateScratchKeyCheckbox()
        {
            ScratchKey = new LabelledCheckbox("SCRATCH KEY:", 20,
                new QuaverCheckbox(new Bindable<bool>(WorkingMap.HasScratchKey)) { DisposeBindableOnDestroy = true })
            {
                Parent = Panel,
                Y = BpmAffectsScrollVelocity.Y,
                X = -BpmAffectsScrollVelocity.X,
                Alignment = Alignment.TopLeft,
            };
        }

        private void CreateLegacyLNRenderingCheckbox()
        {
            LegacyLNRendering = new LabelledCheckbox("LEGACY LN RENDERING:", 20,
                new QuaverCheckbox(new Bindable<bool>(WorkingMap.LegacyLNRendering)) { DisposeBindableOnDestroy = true })
            {
                Parent = Panel,
                Y = BpmAffectsScrollVelocity.Y + BpmAffectsScrollVelocity.Height + Spacing,
                X = BpmAffectsScrollVelocity.X,
                Alignment = Alignment.TopRight,
            };
        }

        public override void Close()
        {
            Artist.Visible = false;
            Title.Visible = false;
            Creator.Visible = false;
            DifficultyName.Visible = false;
            Source.Visible = false;
            Tags.Visible = false;
            KeyCount.Visible = false;
            BpmAffectsScrollVelocity.Visible = false;
            LegacyLNRendering.Visible = false;
            ScratchKey.Visible = false;

            base.Close();
        }

        private void OnClickedYes()
        {
            if (Screen.Map.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot edit the metadata for a map loaded from another game.");
                return;
            }

            WorkingMap.Artist = Artist.Textbox.RawText.Trim();
            WorkingMap.Title = Title.Textbox.RawText.Trim();
            WorkingMap.Creator = Creator.Textbox.RawText.Trim();
            WorkingMap.DifficultyName = DifficultyName.Textbox.RawText.Trim();
            WorkingMap.Source = Source.Textbox.RawText.Trim();
            WorkingMap.Tags = Tags.Textbox.RawText.Trim();
            WorkingMap.LegacyLNRendering = LegacyLNRendering.Checkbox.BindedValue.Value;

            if (WorkingMap.TimingPoints.Count > 0)
            {
                if (BpmAffectsScrollVelocity.Checkbox.BindedValue.Value)
                    WorkingMap.DenormalizeSVs();
                else
                    WorkingMap.NormalizeSVs();
            }

            if (string.IsNullOrEmpty(KeyCount.Textbox.RawText.Trim()))
                return;
            WorkingMap.Mode = ModeHelper.FromKeyCount(int.Parse(KeyCount.Textbox.RawText.Trim()));

            // 7K+1
            WorkingMap.HasScratchKey = ScratchKey.Checkbox.BindedValue.Value;

            var keyCount = ModeHelper.ToKeyCount(WorkingMap.Mode, WorkingMap.HasScratchKey);
            WorkingMap.HitObjects.RemoveAll(x => x.Lane > keyCount);

            Screen.Exit(() =>
            {
                Screen.Save(true, true);
                NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");

                return new EditScreen(Screen.Map, AudioEngine.LoadMapAudioTrack(Screen.Map));
            }
            );
        }

        private void DeactivateTooltip()
        {
            if (CurrentToolTip is null)
                return;

            CurrentToolTip = null;
            Screen.DeactivateTooltip();
        }

        private bool TryActivate(Tooltip tooltip, Drawable drawable)
        {
            if (drawable is null || !drawable.IsHovered())
                return false;

            if (tooltip == CurrentToolTip)
                return true;

            CurrentToolTip = tooltip;
            Screen.ActivateTooltip(tooltip);
            Screen.ActiveTooltip.Parent = this;
            return true;
        }
    }
}
