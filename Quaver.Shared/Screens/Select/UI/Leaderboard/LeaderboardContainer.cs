/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
{
    public class LeaderboardContainer : Sprite
    {
        /// <summary>
        ///     Reference to the parent select screenview.
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///     All of the leaderboard sections to display scores.
        /// </summary>
        public Dictionary<LeaderboardType, LeaderboardScoreSection> Sections { get; } = new Dictionary<LeaderboardType, LeaderboardScoreSection>();

        /// <summary>
        ///     The text that displays that there are no scores available.
        /// </summary>
        private SpriteTextBitmap NoScoresAvailableText { get; set; }

        /// <summary>
        ///     Tells people that they can become a donator to access
        /// </summary>
        private SpriteTextBitmap Donator { get; set; }

        /// <summary>
        ///     To cancel tasks.
        /// </summary>
        private CancellationTokenSource Source { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="view"></param>
        public LeaderboardContainer(SelectScreenView view)
        {
            View = view;
            Size = new ScalableVector2(View.Banner.Width, 356);
            Alpha = 1;
            Image = UserInterface.LeaderboardPanel;

            CreateNoScoresAvailableText();
            CreateSections();
            SwitchSections(ConfigManager.LeaderboardSection.Value);

            MapManager.Selected.ValueChanged += OnMapChange;
            OnlineManager.Status.ValueChanged += OnOnlineStatusChange;
            ConfigManager.LeaderboardSection.ValueChanged += OnLeaderboardSectionChange;
            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChange;

            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnOnlineStatusChange;

            // ReSharper disable once DelegateSubtraction
            ConfigManager.LeaderboardSection.ValueChanged -= OnLeaderboardSectionChange;

            ModManager.ModsChanged -= OnModsChanged;

            if (Source != null)
            {
                Source.Dispose();
                Source = null;
            }

            foreach (var section in Sections.Values)
                section.Destroy();

            base.Destroy();
        }

        /// <summary>
        ///     Creates the text that displays that there are no scores available.
        /// </summary>
        private void CreateNoScoresAvailableText()
        {
            NoScoresAvailableText = new SpriteTextBitmap(FontsBitmap.GothamBold, " ")
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Visible = false,
                FontSize = 16,
                Y = -15
            };

            Donator = new SpriteTextBitmap(FontsBitmap.GothamBold, "")
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Y = NoScoresAvailableText.Y + NoScoresAvailableText.Height + 15,
                Visible = false,
                FontSize = 16
            };
        }

        /// <summary>
        ///     Creates all of the leaderboard sections that will be displayed.
        /// </summary>
        private void CreateSections()
        {
            Sections[LeaderboardType.Local] = new LeaderboardScoreSectionLocal(this)
            {
                Parent = this,
                Y = 2,
                X = 2
            };

            Sections[LeaderboardType.Global] = new LeaderboardScoreSectionGlobal(this)
            {
                Parent = this,
                Y = 2,
                X = 2
            };

            Sections[LeaderboardType.Mods] = new LeaderboardScoreSectionMods(this)
            {
                Parent = this,
                Y = 2,
                X = 2
            };

            Sections[LeaderboardType.Country] = new LeaderboardScoreSectionCountry(this)
            {
                Parent = this,
                Y = 2,
                X = 2
            };
        }

        /// <summary>
        ///     Switches to a different section on the leaderboards.
        /// </summary>
        /// <param name="type"></param>
        private void SwitchSections(LeaderboardType type)
        {
            ConfigManager.LeaderboardSection.Value = type;

            foreach (var section in Sections)
            {
                if (section.Key == type)
                {
                    section.Value.Visible = true;
                }
                else
                {
                    section.Value.Visible = false;
                }
            }

            Source?.Cancel();
            Source?.Dispose();
            Source = new CancellationTokenSource();
            ThreadScheduler.Run(() => InitiateScoreLoad(Source.Token));
        }

        /// <summary>
        ///     Cancels any existing token for loading scores and loads new scores for the current
        ///     map.
        /// </summary>
        public void LoadNewScores()
        {
            Source?.Cancel();
            Source?.Dispose();
            Source = new CancellationTokenSource();
            ThreadScheduler.Run(() => InitiateScoreLoad(Source.Token));
        }

        /// <summary>
        ///     Loads scores for the current map.
        ///
        ///     This needs to be reworked heavily. I don't like this entire system of cancelling
        ///     the token everywhere.......................................
        ///
        ///     Lord help me.
        /// </summary>
        private Task InitiateScoreLoad(CancellationToken cancellationToken = default) => Task.Run(async () =>
        {
            var section = Sections[ConfigManager.LeaderboardSection.Value];

            cancellationToken.ThrowIfCancellationRequested();

            section.IsFetching = true;
            NoScoresAvailableText.Visible = false;
            Donator.Visible = false;

            cancellationToken.ThrowIfCancellationRequested();

            // Scroll to the top of the container and reset the height of the container
            // (removes the scroll wheel)
            section.ScrollTo(0, 1);
            section.ContentContainer.Height = section.Height;

            try
            {
                section.ClearScores();
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(400, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                var map = MapManager.Selected.Value;

                map.ClearScores();
                var scores = section.FetchScores();

                if (OnlineManager.CurrentGame == null)
                    map.Scores.Value = scores.Scores;

                cancellationToken.ThrowIfCancellationRequested();
                section.IsFetching = false;

                cancellationToken.ThrowIfCancellationRequested();

                if (scores.Scores.Count == 0 && scores.PersonalBest == null)
                {
                    NoScoresAvailableText.Text = section.GetNoScoresAvailableString(map);
                    NoScoresAvailableText.Alpha = 0;
                    NoScoresAvailableText.Visible = true;

                    Donator.Alpha = 0;

                    if (!OnlineManager.IsDonator && ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                    {
                        Donator.Visible = true;
                        NoScoresAvailableText.Y = -15;
                        Donator.Y = NoScoresAvailableText.Y + NoScoresAvailableText.Height + 15;
                    }
                    else
                    {
                        NoScoresAvailableText.Y = 0;
                    }

                    Donator.ClearAnimations();
                    Donator.FadeTo(1, Easing.Linear, 150);

                    NoScoresAvailableText.ClearAnimations();
                    NoScoresAvailableText.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 150));
                }
                else
                {
                    NoScoresAvailableText.Visible = false;
                    Donator.Visible = false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                section.UpdateWithScores(map, scores, cancellationToken);
            }
            catch (Exception e)
            {
                section.IsFetching = true;
                NoScoresAvailableText.Visible = false;
                Donator.Visible = false;
            }
        });

        /// <summary>
        ///     Called whenever the selected map changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChange(object sender, BindableValueChangedEventArgs<Map> e) => LoadNewScores();

        /// <summary>
        ///     Called whenever the user's onlinestatus is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnOnlineStatusChange(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected || e.OldValue == ConnectionStatus.Connected)
                return;

            if (ConfigManager.LeaderboardSection.Value != LeaderboardType.Global && ConfigManager.LeaderboardSection.Value != LeaderboardType.Mods)
                return;

            // When the user connects again, load scores.
            LoadNewScores();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderboardSectionChange(object sender, BindableValueChangedEventArgs<LeaderboardType> e)
        {
            Sections[e.OldValue].ClearScores();
            Sections[e.OldValue].Visible = false;
            Sections[e.Value].Visible = true;

            LoadNewScores();
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            if (ConfigManager.LeaderboardSection.Value != LeaderboardType.Mods)
                return;

            LoadNewScores();
        }
    }
}
