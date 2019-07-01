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
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public class ModifiersDialog : DialogScreen
    {
        /// <summary>
        ///     The actual content of the interface
        /// </summary>
        private Sprite InterfaceContainer { get; set; }

        /// <summary>
        ///     The container for the header of the interface.
        /// </summary>
        private Sprite HeaderContainer { get; set; }

        /// <summary>
        ///     The scroll container which houses all of the chat channels.
        /// </summary>
        public ScrollContainer ModifierContainer { get; private set; }

        /// <summary>
        ///     The list of modifiers that are in the dialog.
        /// </summary>
        private List<DrawableModifier> ModsList { get; set; }

        /// <summary>
        ///     The active modifiers when the dialog was opened.
        /// </summary>
        private ModIdentifier ModsWhenDialogOpen { get; }

        private bool isClosing { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ModifiersDialog() : base(0)
        {
            ModsWhenDialogOpen = ModManager.Mods;
            CreateContent();

            Clicked += (sender, args) =>
            {
                if (!GraphicsHelper.RectangleContains(InterfaceContainer.ScreenRectangle,
                    MouseManager.CurrentState.Position) && !isClosing)
                {
                    Close();
                    isClosing = true;
                }
            };

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameStarted += OnGameStarted;
                OnlineManager.Client.OnGameHostChanged += OnGameHostChanged;
            }
        }

        public override void Destroy()
        {
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameStarted -= OnGameStarted;
                OnlineManager.Client.OnGameHostChanged -= OnGameHostChanged;
            }

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateInterfaceContainer();
            CreateHeaderContainer();
            CreateModifierContainer();
            CreateModifierOptions();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) || KeyboardManager.IsUniqueKeyPress(Keys.F1) || KeyboardManager.IsUniqueKeyPress(Keys.F8))
                Close();
        }

        /// <summary>
        ///     Creates the container for the entire dialog.
        /// </summary>
        private void CreateInterfaceContainer() => InterfaceContainer = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            SetChildrenAlpha = true,
            Size = new ScalableVector2(Width, 350),
            Tint = Color.Black,
            Y = 350,
            Alpha = 0,
            Animations =
            {
                new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 400),
                new Animation(AnimationProperty.Y, Easing.OutQuint, 400, 0, 800)
            },
        };

        /// <summary>
        ///     Creates the header container for the interface.
        /// </summary>
        private void CreateHeaderContainer()
        {
            HeaderContainer = new Sprite()
            {
                Parent = InterfaceContainer,
                Size = new ScalableVector2(Width, 75),
                Tint = Colors.DarkGray,
            };

            var line = new Sprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(HeaderContainer.Width, 2),
                Tint = Colors.MainAccent
            };

            var icon = new Sprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = 25,
                Size = new ScalableVector2(HeaderContainer.Height * 0.50f, HeaderContainer.Height * 0.50f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_level_up),
            };

            var chatChannels = new SpriteText(Fonts.Exo2SemiBold, "Modifiers", 14)
            {
                Parent = icon,
                Y = -3,
                X = icon.Width + 15,
            };

            var description = new SpriteText(Fonts.Exo2SemiBold, "Switch it up for a change, and customize gameplay to your heart's desire.",
                13)
            {
                Parent = icon,
                Y = chatChannels.Y + chatChannels.Height - 2,
                X = icon.Width + 15,
            };
        }

        /// <summary>
        ///     Creates the scroll container that'll house all the modifiers
        /// </summary>
        private void CreateModifierContainer()
        {
            var size = new ScalableVector2(InterfaceContainer.Width, InterfaceContainer.Height - HeaderContainer.Height);
            ModifierContainer = new ScrollContainer(size, size)
            {
                Parent = InterfaceContainer,
                Y = HeaderContainer.Height,
                Tint = Color.Black,
                InputEnabled = true
            };

            ModifierContainer.Scrollbar.Tint = Color.White;
            ModifierContainer.Scrollbar.Width = 5;
            ModifierContainer.ScrollSpeed = 150;
            ModifierContainer.EasingType = Easing.OutQuint;
            ModifierContainer.TimeToCompleteScroll = 1500;
        }

        /// <summary>
        ///     Creates all of the modifiers to be used on-screen
        /// </summary>
        private void CreateModifierOptions()
        {
            ModsList = new List<DrawableModifier>()
            {
                new DrawableModifierSpeed(this)
                {
                    Alignment = Alignment.TopLeft
                },

                new DrawableModifierBool(this, new ModMirror())
                {
                    Alignment = Alignment.TopLeft
                },

                new DrawableModifierBool(this, new ModAutoplay())
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierBool(this, new ModNoFail())
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierBool(this, new ModNoSliderVelocities())
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierModList(this, new IGameplayModifier[]
                {
                    new ModNoLongNotes(), new ModInverse(), new ModFullLN()
                }, "Long Note Conversion", "Mix up the long notes.")
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierBool(this, new ModRandomize())
                {
                    Alignment = Alignment.TopLeft,
                },
            };

            for (var i = ModsList.Count - 1; i >= 0; i--)
            {
                if (ModsList[i] is DrawableModifierBool mod)
                {
                    if (!mod.Modifier.AllowedInMultiplayer && OnlineManager.CurrentGame != null)
                    {
                        ModsList.Remove(ModsList[i]);
                        continue;
                    }
                }

                if (ModsList[i] is DrawableModifierModList modList)
                {
                    if (modList.Modifiers.Any(x => x.OnlyMultiplayerHostCanCanChange) &&
                        OnlineManager.CurrentGame != null &&
                        OnlineManager.CurrentGame.HostId != OnlineManager.Self.OnlineUser.Id)
                    {
                        ModsList.Remove(ModsList[i]);
                    }
                }
            }

            for (var i = 0; i < ModsList.Count; i++)
            {
                var mod = ModsList[i];
                ModifierContainer.AddContainedDrawable(mod);
                mod.Y = i * mod.Height;
            }

            var proposedHeight = ModsList.Count * ModsList.First().Height;

            if (proposedHeight > ModifierContainer.ContentContainer.Height)
                ModifierContainer.ContentContainer.Height = proposedHeight;
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        private void Close(bool changeMods = true)
        {
            if (isClosing)
                return;

            if (OnlineManager.CurrentGame != null && changeMods)
            {
                if (ModsWhenDialogOpen != ModManager.Mods)
                {
                    var diffRating = OnlineManager.CurrentGame.DifficultyRating;

                    // Only update the difficulty rating when closing if the selected map is still the same.
                    // If the user switches maps, but changes modifiers, it'll be incorrect.
                    if (MapManager.Selected.Value != null && MapManager.Selected.Value.Md5Checksum == OnlineManager.CurrentGame.MapMd5)
                        diffRating = MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods);

                    var rateNow = ModHelper.GetRateFromMods(ModManager.Mods);

                    // Change the global mods of the game
                    var rateMod = (long) ModHelper.GetModsFromRate(rateNow);

                    if (rateMod == -1)
                        rateMod = 0;

                    var activeModsWithoutRate = (long) ModManager.Mods - rateMod;
                    ModIdentifier hostOnlyMods = 0L;
                    var onlyHostChangeableMods = ModManager.CurrentModifiersList.FindAll(x => x.OnlyMultiplayerHostCanCanChange);

                    if (onlyHostChangeableMods.Count != 0)
                    {
                        onlyHostChangeableMods.ForEach(x =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            activeModsWithoutRate -= (long) x.ModIdentifier;
                            hostOnlyMods |= x.ModIdentifier;
                        });
                    }

                    if (activeModsWithoutRate == -1)
                        activeModsWithoutRate = 0;

                    // If we're on regular free mod mode, when we change the rate,
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (OnlineManager.CurrentGame.FreeModType == MultiplayerFreeModType.Regular &&
                        (ModHelper.GetRateFromMods(ModsWhenDialogOpen) != rateNow || hostOnlyMods != 0)
                        && OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                    {
                        OnlineManager.Client?.MultiplayerChangeGameModifiers(rateMod + (long) hostOnlyMods, diffRating);

                        // Change the mods of ourselves minus the mods rate (gets all other activated modes)
                        OnlineManager.Client?.MultiplayerChangePlayerModifiers(activeModsWithoutRate);
                    }
                    // Free mod is enabled, but we haven't done any rate changing, so just
                    // enable player modifiers for us
                    else if (OnlineManager.CurrentGame.FreeModType != MultiplayerFreeModType.None)
                    {
                        // Free Mod + Free Rate
                        if (OnlineManager.CurrentGame.FreeModType.HasFlag(MultiplayerFreeModType.Regular)
                            && OnlineManager.CurrentGame.FreeModType.HasFlag(MultiplayerFreeModType.Rate))
                        {
                            if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                                OnlineManager.Client?.MultiplayerChangeGameModifiers((long) hostOnlyMods, diffRating);

                            OnlineManager.Client?.MultiplayerChangePlayerModifiers((long) ModManager.Mods);
                        }
                        // Either Free Mod OR Free Rate
                        else
                        {
                            switch (OnlineManager.CurrentGame.FreeModType)
                            {
                                case MultiplayerFreeModType.Regular:
                                    OnlineManager.Client?.MultiplayerChangePlayerModifiers(activeModsWithoutRate);

                                    if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                                        OnlineManager.Client?.MultiplayerChangeGameModifiers(rateMod + (long) hostOnlyMods, diffRating);
                                    break;
                                case MultiplayerFreeModType.Rate:
                                    OnlineManager.Client?.MultiplayerChangePlayerModifiers((long) ModHelper.GetModsFromRate(rateNow));

                                    if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                                        OnlineManager.Client?.MultiplayerChangeGameModifiers(activeModsWithoutRate + (long) hostOnlyMods, diffRating);
                                    break;
                            }
                        }
                    }
                    // We're host & free mod isn't enabled, so change the global game mods
                    else if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                        OnlineManager.Client?.MultiplayerChangeGameModifiers((long) ModManager.Mods, diffRating);
                }
            }

            isClosing = true;
            InterfaceContainer.Animations.Clear();
            InterfaceContainer.Animations.Add(new Animation(AnimationProperty.Y, Easing.OutQuint, InterfaceContainer.Y, InterfaceContainer.Height, 600));
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 450);
        }

        private void OnGameStarted(object sender, GameStartedEventArgs e) => Close(false);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnGameHostChanged(object sender, GameHostChangedEventArgs e)
            => Close(false);
    }
}
