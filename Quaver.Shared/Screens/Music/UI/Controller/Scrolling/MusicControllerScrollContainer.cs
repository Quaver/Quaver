using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Music.UI.Controller.Scrolling
{
    public class MusicControllerScrollContainer : PoolableScrollContainer<Mapset>
    {
        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableSongs { get; }

        /// <summary>
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        /// </summary>
        public MusicControllerSongContainer SongContainer { get; }

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="songContainer"></param>
        /// <param name="availableSongs"></param>
        /// <param name="searchQuery"></param>
        /// <param name="size"></param>
        public MusicControllerScrollContainer(MusicControllerSongContainer songContainer, Bindable<List<Mapset>> availableSongs,
            Bindable<string> searchQuery, ScalableVector2 size) : base(availableSongs.Value, 16, 0, size, size)
        {
            AvailableSongs = availableSongs;
            CurrentSearchQuery = searchQuery;
            SongContainer = songContainer;

            Tint = ColorHelper.HexToColor("#2F2F2F");
            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 10;

            InputEnabled = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            Alpha = 1;

            AvailableItems = AvailableSongs.Value;

            CreateLoadingWheel();
            SetPoolStartingIndex();
            CreatePool();
            SnapToSelected();

            CurrentSearchQuery.ValueChanged += OnSearchChanged;
            AvailableSongs.ValueChanged += OnAvailableSongsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            PositionAndContainPoolObjects();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            AvailableSongs.ValueChanged -= OnAvailableSongsChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Mapset> CreateObject(Mapset item, int index) => new MusicControllerSong(this, item, index);

        /// <summary>
        /// </summary>
        private void SetPoolStartingIndex()
        {
            var index = AvailableSongs.Value.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

            PoolStartingIndex = MathHelper.Clamp(index, 0, AvailableSongs.Value.Count - PoolSize - 1);

            if (PoolStartingIndex == -1)
                PoolStartingIndex = 0;
        }

        /// <summary>
        /// </summary>
        private void SnapToSelected()
        {
            ContentContainer.Animations.Clear();

            ContentContainer.Y = -MusicControllerSong.SongHeight * PoolStartingIndex;
            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;
        }

        /// <summary>
        ///     Makes sure all of the objects in the pool are positioned properly
        ///     and contained in the container
        /// </summary>
        protected void PositionAndContainPoolObjects()
        {
            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Y = (PoolStartingIndex + i) * Pool[i].HEIGHT + PaddingTop;

                if (Pool[i].Parent != ContentContainer)
                    AddContainedDrawable(Pool[i]);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableSongsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e)
        {
            ThreadScheduler.RunAfter(() =>
            {
                lock (AvailableSongs.Value)
                {
                    AvailableItems = e.Value;

                    ScheduleUpdate(() =>
                    {
                        FadeLoadingWheel(0);

                        Pool.ForEach(x => x.Destroy());
                        Pool.Clear();
                        RecalculateContainerHeight();

                        SetPoolStartingIndex();
                        CreatePool();
                        SnapToSelected();
                    });
                }
            }, 250);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            FadeLoadingWheel(1);

            ScheduleUpdate(() =>
            {
                Pool.ForEach(x => x.Destroy());
                Pool.Clear();
                RecalculateContainerHeight();
            });
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new LoadingWheel
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(50, 50),
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        /// <param name="alpha"></param>
        private void FadeLoadingWheel(float alpha)
        {
            LoadingWheel.ClearAnimations();
            LoadingWheel.FadeTo(alpha, Easing.Linear, 50);
        }
    }
}