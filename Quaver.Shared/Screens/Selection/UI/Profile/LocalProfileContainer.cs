using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class LocalProfileContainer : Sprite
    {
        /// <summary>
        /// </summary>
        public Bindable<UserProfile> Profile { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BackgroundContainer { get; set; }

        /// <summary>
        /// </summary>
        public TaskHandler<int, int> LoadProfileTask { get; }

        /// <summary>
        /// </summary>
        private LoadingWheel Wheel { get; set; }

        /// <summary>
        /// </summary>
        private LocalProfileBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private LocalProfileTable Table { get; set; }

        /// <summary>
        /// </summary>
        private ProfileSelectionDropdown Dropdown { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus StatusText { get; set; }

        /// <summary>
        /// </summary>
        public LocalProfileContainer(Bindable<UserProfile> profile)
        {
            Profile = profile;
            Size = new ScalableVector2(564, 838);
            Alpha = 0;

            LoadProfileTask = new TaskHandler<int, int>(LoadProfile);
            CreateHeaderText();
            CreateContainer();
            CreateLoadingWheel();
            CreateStatusText();
            CreateDropdown();

            StartProfileLoadTask();

            if (ConfigManager.SelectedGameMode != null)
                ConfigManager.SelectedGameMode.ValueChanged += OnSelectedGameModeChanged;

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            Profile.ValueChanged += OnProfileChanged;

            if (UserProfileDatabaseCache.Profiles != null)
            {
                UserProfileDatabaseCache.Profiles.ItemAdded += OnProfileAdded;
                UserProfileDatabaseCache.Profiles.ItemRemoved += OnProfileRemoved;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (ConfigManager.SelectedGameMode != null)
            {
                // ReSharper disable once DelegateSubtraction
                ConfigManager.SelectedGameMode.ValueChanged -= OnSelectedGameModeChanged;
            }

            // ReSharper disable twice DelegateSubtraction
            Profile.ValueChanged -= OnProfileChanged;
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

            if (UserProfileDatabaseCache.Profiles != null)
            {
                UserProfileDatabaseCache.Profiles.ItemAdded -= OnProfileAdded;
                UserProfileDatabaseCache.Profiles.ItemRemoved -= OnProfileRemoved;
            }

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Wheel.Alpha = MathHelper.Lerp(Wheel.Alpha, LoadProfileTask.IsRunning ? 1 : 0,
                (float) (gameTime.ElapsedGameTime.TotalMilliseconds / 45f));

            base.Update(gameTime);
        }

        /// <summary>
        ///    Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeaderText()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "PROFILE", 30)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            BackgroundContainer = new Sprite
            {
                Parent = this,
                Image = UserInterface.ModifierSelectorBackground,
                Size = new ScalableVector2(Width, Height - Header.Height - 8),
                Y = Header.Y + Header.Height + 8,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => Wheel = new LoadingWheel
        {
            Parent = BackgroundContainer,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(52, 52),
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateStatusText()
        {
            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = BackgroundContainer,
                Alignment = Alignment.MidCenter,
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDropdown() => Dropdown = new ProfileSelectionDropdown(Profile)
        {
            Parent = this,
            Alignment = Alignment.TopRight
        };

        /// <summary>
        /// </summary>
        private void StartProfileLoadTask()
        {
            if (LoadProfileTask.IsRunning)
                LoadProfileTask.Cancel();

            if (Banner != null)
                Banner.Visible = false;

            if (Table != null)
                Table.Visible = false;

            StatusText.ClearAnimations();
            StatusText.FadeTo(0, Easing.Linear, 150);

            LoadProfileTask.Run(0, 250);
        }

        /// <summary>
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private int LoadProfile(int arg, CancellationToken token)
        {
            Profile.Value.PopulateStats();

            ScheduleUpdate(() =>
            {
                Table?.Destroy();
                Banner?.Destroy();

                // Not connected to the server, so display a message
                if (Profile.Value.IsOnline && !OnlineManager.Connected)
                {
                    StatusText.Text = $"You must be logged in to view your online profile".ToUpper();
                    StatusText.FadeTo(1, Easing.Linear, 250);
                    return;
                }

                Banner = new LocalProfileBanner(Profile, new ScalableVector2(Width - 4, 150))
                {
                    Parent = BackgroundContainer,
                    Alignment = Alignment.TopCenter,
                    Y = 2
                };

                Table = new LocalProfileTable(Profile, new ScalableVector2(Width - 4,
                    BackgroundContainer.Height - Banner.Y * 2 - Banner.Height))
                {
                    Parent = BackgroundContainer,
                    Alignment = Alignment.TopCenter,
                    Y = Banner.Y + Banner.Height
                };

                const int time = 250;

                Table.FadeTo(1, Easing.Linear, time);
                Banner.FadeTo(1, Easing.Linear, time);
            });

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedGameModeChanged(object sender, BindableValueChangedEventArgs<GameMode> e)
            => StartProfileLoadTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProfileChanged(object sender, BindableValueChangedEventArgs<UserProfile> e)
            => StartProfileLoadTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (!Profile.Value.IsOnline)
                return;

            if (e.Value != ConnectionStatus.Connected)
                return;

            StartProfileLoadTask();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProfileRemoved(object sender, BindableListItemRemovedEventArgs<UserProfile> e) => RecreateDropdown();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProfileAdded(object sender, BindableListItemAddedEventArgs<UserProfile> e) => RecreateDropdown();

        /// <summary>
        /// </summary>
        private void RecreateDropdown()
        {
            ScheduleUpdate(() =>
            {
                Dropdown?.Destroy();
                CreateDropdown();
            });
        }
    }
}