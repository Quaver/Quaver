using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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

        private EditorMetadataModeDropdown GameMode { get; set; }

        private LabelledCheckbox BpmAffectsScrollVelocity { get; set; }

        private TextboxTabControl TabControl { get; }

        private const int LabelSize = 21;

        private const int TextboxHeight = 42;

        private const int TextboxLabelSpacing = 12;

        private const int Spacing = 31;

        public EditorMetadataDialog(EditScreen screen) : base("EDIT METADATA", "Edit the values to change the metadata...")
        {
            Screen = screen;
            Panel.Height = 930;
            Panel.Image = UserInterface.MetadataPanel;

            CreateArtistTextbox();
            CreateSongTitleTextbox();
            CreateCreatorTextbox();
            CreateDifficultyNameTextbox();
            CreateSourceTextbox();
            CreateTagsTextbox();
            CreateGameModeDropdown();
            CreateBpmAffectsSvCheckbox();

            TabControl = new TextboxTabControl(new List<Textbox>()
            {
                Artist.Textbox,
                Title.Textbox,
                Creator.Textbox,
                DifficultyName.Textbox,
                Source.Textbox,
                Tags.Textbox
            })
            {
                Parent = this,
            };

            YesButton.Y += 32;
            NoButton.Y = YesButton.Y;

            YesAction = OnClickedYes;
        }

        private void CreateArtistTextbox()
        {
            Artist = new LabelledTextbox(Panel.Width * 0.90f, "Artist Name", LabelSize, TextboxHeight,
                LabelSize, TextboxLabelSpacing, "Enter the name of the artist", WorkingMap.Artist)
            {
                Parent = Panel,
                Y = Banner.Height + 20,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateSongTitleTextbox()
        {
            Title = new LabelledTextbox(Artist.Width, "Song Title", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Enter the title of the song", WorkingMap.Title)
            {
                Parent = Panel,
                Y = Artist.Y + Artist.Height + Spacing,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateCreatorTextbox()
        {
            Creator = new LabelledTextbox(Artist.Width, "Creator Username", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Enter your username", WorkingMap.Creator)
            {
                Parent = Panel,
                Y = Title.Y + Title.Height + Spacing,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateDifficultyNameTextbox()
        {
            DifficultyName = new LabelledTextbox(Artist.Width, "Difficulty Name", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Enter the name of the difficulty", WorkingMap.DifficultyName)
            {
                Parent = Panel,
                Y = Creator.Y + Creator.Height + Spacing,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateSourceTextbox()
        {
            Source = new LabelledTextbox(Artist.Width, "Source", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Enter the source of the song (album, mixtape, etc)",
                WorkingMap.Source)
            {
                Parent = Panel,
                Y = DifficultyName.Y + DifficultyName.Height + Spacing,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateTagsTextbox()
        {
            Tags = new LabelledTextbox(Artist.Width, "Tags", LabelSize, TextboxHeight, LabelSize,
                TextboxLabelSpacing, "Enter comma-separated tags to help players find your map",
                WorkingMap.Tags)
            {
                Parent = Panel,
                Y = Source.Y + Source.Height + Spacing,
                Alignment = Alignment.TopCenter,
                Textbox = { AllowSubmission = false },
                Tint = Color.Transparent
            };
        }

        private void CreateGameModeDropdown()
        {
            GameMode = new EditorMetadataModeDropdown(Screen.WorkingMap)
            {
                Parent = Panel,
                Y = Tags.Y + Tags.Height + Spacing,
                X = -80,
                Alignment = Alignment.TopRight,
            };
        }

        private void CreateBpmAffectsSvCheckbox()
        {
            BpmAffectsScrollVelocity = new LabelledCheckbox("BPM AFFECTS SV:", 20,
                new QuaverCheckbox(new Bindable<bool>(!WorkingMap.BPMDoesNotAffectScrollVelocity)) { DisposeBindableOnDestroy = true })
            {
                Parent = Panel,
                Y = GameMode.Y + 6,
                X = -GameMode.X,
                Alignment = Alignment.TopLeft,
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
            GameMode.Visible = false;
            BpmAffectsScrollVelocity.Visible = false;

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

            if (WorkingMap.TimingPoints.Count > 0)
            {
                if (BpmAffectsScrollVelocity.Checkbox.BindedValue.Value)
                    WorkingMap.DenormalizeSVs();
                else
                    WorkingMap.NormalizeSVs();
            }

            WorkingMap.Mode = GameMode.SelectedMode;

            // Remove all objects that are above lane 4
            if (WorkingMap.Mode == API.Enums.GameMode.Keys4)
                WorkingMap.HitObjects.RemoveAll(x => x.Lane > 4);

            // 7K+1
            WorkingMap.HasScratchKey = GameMode.Dropdown.SelectedIndex == EditorMetadataModeDropdown.Keys7Plus1Index;

            // Remove any objects that are in the scratch lane (8) if going from scratch to no-scratch
            if (!WorkingMap.HasScratchKey)
                WorkingMap.HitObjects.RemoveAll(x => x.Lane > 7);

            Screen.Exit(() =>
            {
                Screen.Save(true, true);
                NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");

                return new EditScreen(Screen.Map, AudioEngine.LoadMapAudioTrack(Screen.Map));
            });
        }
    }
}
