/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Transitions
{
    public static class Transitioner
    {
        private static readonly object TransitionLock = new object();

        private static readonly object ForegroundElementLock = new object();

        private static string[] ForegroundElementKeys { get; set; } = Array.Empty<string>();

        /// <summary>
        ///     The blackness sprite that acts as the screen's transitioner
        /// </summary>
        public static Sprite Blackness { get; set; }


        /// <summary>
        ///     Initializes the screen transitioner.
        /// </summary>
        public static void Initialize()
        {
            lock (TransitionLock)
            {
                Blackness = new Sprite()
                {
                    Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                    Tint = Color.Black,
                    Alpha = 0
                };
            }

            WindowManager.ResolutionChanged += OnResolutionChanged;
        }

        public static void Update(GameTime gameTime)
        {
            lock (TransitionLock)
            {
                if (Blackness == null)
                    return;

                Blackness.Width = WindowManager.Width;
                Blackness.Height = WindowManager.Height;
                Blackness.Update(gameTime);

                // Wobble completes animations within a small tolerance without snapping the
                // animated value to its endpoint. A completed fade-out can otherwise leave a
                // tiny positive alpha forever, causing retained elements to be redrawn above
                // dialogs and overlays on every subsequent frame.
                if (Blackness.Animations.Count == 0)
                {
                    if (Blackness.Alpha < 0.01f)
                        Blackness.Alpha = 0;
                    else if (Blackness.Alpha > 0.99f)
                        Blackness.Alpha = 1;
                }
            }
        }

        public static bool IsAnimating
        {
            get
            {
                lock (TransitionLock)
                    return Blackness?.Animations.Count > 0;
            }
        }

        public static void Draw(GameTime gameTime) => Blackness?.Draw(gameTime);

        /// <summary>
        ///     Disposes of the the transitioner.
        /// </summary>
        public static void Dispose()
        {
            SetForegroundElements(Array.Empty<string>());

            lock (TransitionLock)
            {
                if (Blackness != null)
                {
                    Blackness.Destroy();
                    Blackness = null;
                }
            }

            WindowManager.ResolutionChanged -= OnResolutionChanged;
        }

        /// <summary>
        ///     Called when the resolution of the game changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnResolutionChanged(object sender, EventArgs e) =>
            ResizeBlackness();

        private static void ResizeBlackness()
        {
            lock (TransitionLock)
            {
                if (Blackness != null)
                    Blackness.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            }
        }

        /// <summary>
        ///     Fades the transitioner out
        /// </summary>
        public static void FadeIn(IReadOnlyCollection<string> foregroundElementKeys = null)
        {
            SetForegroundElements(foregroundElementKeys);

            lock (TransitionLock)
            {
                if (Blackness == null)
                    return;

                Blackness.ClearAnimations();
                Blackness.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                    Blackness.Alpha, 1, 300));
            }
        }

        internal static void SetForegroundElements(IReadOnlyCollection<string> foregroundElementKeys)
        {
            lock (ForegroundElementLock)
            {
                ForegroundElementKeys = foregroundElementKeys?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray() ?? Array.Empty<string>();
            }
        }

        /// <summary>
        ///     Unfades the transitioner
        /// </summary>
        public static void FadeOut()
        {
            lock (TransitionLock)
            {
                if (Blackness == null)
                    return;

                Blackness.ClearAnimations();
                Blackness.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                    Blackness.Alpha, 0, 300));
            }
        }

        /// <summary>
        ///     Temporarily hides transition foreground elements from the normal screen draw.
        ///     They are restored when the returned scope is disposed.
        /// </summary>
        public static IDisposable SuppressForegroundElements()
        {
            var elements = GetForegroundElements();
            return elements.Count == 0
                ? EmptyDisposable.Instance
                : new ForegroundElementSuppression(elements);
        }

        /// <summary>
        ///     Draws retained elements above the transition overlay.
        /// </summary>
        public static void DrawForegroundElements(GameTime gameTime)
        {
            foreach (var element in GetForegroundElements())
                element.Draw(gameTime);
        }

        private static List<Drawable> GetForegroundElements()
        {
            if (Blackness == null || Blackness.Alpha <= 0)
                return new List<Drawable>();

            string[] keys;
            lock (ForegroundElementLock)
                keys = ForegroundElementKeys.ToArray();

            var elements = new List<Drawable>(keys.Length);
            foreach (var key in keys)
            {
                if (ScreenManager.TryGetElement<Drawable>(key, out var element) &&
                    !element.IsDisposed && element.Visible)
                    elements.Add(element);
            }

            return elements;
        }

        private sealed class ForegroundElementSuppression : IDisposable
        {
            private List<DrawableVisibilityState> VisibilityStates { get; }

            private bool IsDisposed { get; set; }

            public ForegroundElementSuppression(IEnumerable<Drawable> elements)
            {
                var roots = elements.ToArray();
                VisibilityStates = roots
                    .Select(root => new DrawableVisibilityState(root, root.Visible,
                        root.SetChildrenVisibility))
                    .ToList();

                foreach (var root in roots)
                {
                    root.SetChildrenVisibility = false;
                    root.Visible = false;
                }
            }

            public void Dispose()
            {
                if (IsDisposed)
                    return;

                foreach (var state in VisibilityStates)
                {
                    if (state.Drawable.IsDisposed)
                        continue;

                    state.Drawable.Visible = state.Visible;
                    state.Drawable.SetChildrenVisibility = state.SetChildrenVisibility;
                }

                IsDisposed = true;
            }
        }

        private sealed class DrawableVisibilityState
        {
            public Drawable Drawable { get; }

            public bool Visible { get; }

            public bool SetChildrenVisibility { get; }

            public DrawableVisibilityState(Drawable drawable, bool visible, bool setChildrenVisibility)
            {
                Drawable = drawable;
                Visible = visible;
                SetChildrenVisibility = setChildrenVisibility;
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static EmptyDisposable Instance { get; } = new EmptyDisposable();

            public void Dispose()
            {
            }
        }
    }
}
