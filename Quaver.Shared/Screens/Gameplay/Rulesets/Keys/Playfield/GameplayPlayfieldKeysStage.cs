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
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Health;
using Quaver.Shared.Screens.Gameplay.UI;
using Quaver.Shared.Screens.Gameplay.UI.Health;
using Quaver.Shared.Screens.Gameplay.UI.Multiplayer;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield
{
    public class GameplayPlayfieldKeysStage
    {
        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     Reference to the parent playfield.
        /// </summary>
        public GameplayPlayfieldKeys Playfield { get; }

        /// <summary>
        ///     The container that holds all of the HitObjects.
        /// </summary>
        public Container HitObjectContainer { get; private set; }

        /// <summary>
        ///     The Container that holds every Timing Line object
        /// </summary>
        public Container TimingLineContainer { get; private set; }

        /// <summary>
        ///     The container that holds all hits.
        /// </summary>
        public Container HitContainer { get; private set; }

        /// <summary>
        ///     The left side of the stage.
        /// </summary>
        public Sprite StageLeft { get; private set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        public Sprite StageRight { get; private set; }

        /// <summary>
        ///     The BG Mask of the stage.
        /// </summary>
        public Sprite BgMask { get; private set; }

        /// <summary>
        ///     The HitPositionOverlay of the stage.
        /// </summary>
        public Sprite HitPositionOverlay { get; private set; }

        /// <summary>
        ///     The list of receptors
        /// </summary>
        public List<Sprite> Receptors { get; private set; }

        /// <summary>
        ///     The list of column lighting objects.
        /// </summary>
        public List<ColumnLighting> ColumnLightingObjects { get; private set; }

        /// <summary>
        ///     The distant overlay sprite.
        /// </summary>
        public Sprite DistantOverlay { get; private set; }

        /// <summary>
        ///     Container that contains the elements for lane cover.
        /// </summary>
        public Container LaneCoverContainer { get; private set; }

        /// <summary>
        ///     Sprite that displays the current combo.
        /// </summary>
        public NumberDisplay ComboDisplay { get; private set; }

        /// <summary>
        ///     The combo in the previous frame. Used to determine if we should update it.
        /// </summary>
        private int OldCombo { get; set; }

        /// <summary>
        ///     The original value for the combo display's Y position,
        ///     so we can use this to set it back after it's done with its animation.
        /// </summary>
        public float OriginalComboDisplayY { get; set; }

        /// <summary>
        ///     The HitError bar.
        /// </summary>
        public HitErrorBar HitError { get; private set; }

        /// <summary>
        ///     The JudgementHitBurst Sprite.
        /// </summary>
        public JudgementHitBurst JudgementHitBurst { get; private set; }

        /// <summary>
        ///     When hitting an object, this is the sprite that will be shown at
        ///     the hitposition.
        /// </summary>
        public List<HitLighting> HitLightingObjects { get; private set; }

        /// <summary>
        ///     The health bar for the stage.
        /// </summary>
        public HealthBar HealthBar { get; private set; }

        /// <summary>
        ///     Displays the name of the song.
        /// </summary>
        private SongInformation SongInfo { get; set; }

        /// <summary>
        ///     Displays last place/elimination alerts for battle royale
        /// </summary>
        private BattleRoyaleAlert BattleRoyaleAlert { get; set; }

        /// <summary>
        ///     Make a quicker and shorter reference to the game skin
        /// </summary>
        private SkinKeys Skin => SkinManager.Skin.Keys[Screen.Map.Mode];

        /// <summary>
        /// </summary>
        private BattleRoyalePlayerEliminated BattleRoyalePlayerEliminated { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="playfield"></param>
        public GameplayPlayfieldKeysStage(GameplayScreen screen, GameplayPlayfieldKeys playfield)
        {
            Screen = screen;
            Playfield = playfield;

            CreateStageLeft();
            CreateStageRight();
            CreateBgMask();

            // Depending on what the skin.ini's value is, we'll want to either initialize
            // the receptors first, or the playfield first.
            if (Skin.ReceptorsOverHitObjects)
            {
                CreateTimingLineContainer();
                CreateHitObjectContainer();
                CreateHitContainer();
                CreateReceptorsAndLighting();
                CreateHitPositionOverlay();
            }
            else
            {
                CreateReceptorsAndLighting();
                CreateHitPositionOverlay();
                CreateTimingLineContainer();
                CreateHitObjectContainer();
                CreateHitContainer();
            }

            CreateDistantOverlay();

            // Depending on what the config value is, we'll display ui elements over the lane cover.
            // Note: Lane cover will always be displayed over the receptors due to the creation order.
            if (ConfigManager.UIElementsOverLaneCover.Value)
            {
                CreateLaneCoverOverlay();
                CreateComboDisplay();
                CreateHitError();
                CreateHitLighting();
                CreateJudgementHitBurst();

                if (OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Battle_Royale &&
                    ConfigManager.EnableBattleRoyaleAlerts.Value)
                {
                    CreateBattleRoyaleAlert();
                    CreateBattleRoyaleEliminated();
                }


                CreateSongInfo();
            }
            else
            {
                CreateComboDisplay();
                CreateHitError();
                CreateJudgementHitBurst();
                CreateHitLighting();

                if (OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Battle_Royale &&
                    ConfigManager.EnableBattleRoyaleAlerts.Value)
                {
                    CreateBattleRoyaleAlert();
                    CreateBattleRoyaleEliminated();
                }

                CreateSongInfo();
                CreateLaneCoverOverlay();
            }

            CreateHealthBar();
        }

        /// <summary>
        ///     Updates the stage and keeps everything in-tact.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime) => UpdateComboDisplay(gameTime);

        /// <summary>
        ///     Creates the left side of the stage.
        /// </summary>
        private void CreateStageLeft()
        {
            // Create the left side of the stage.
            var stageLeftX = Skin.StageLeftBorder.Width * WindowManager.Height / Skin.StageLeftBorder.Height;

            StageLeft = new Sprite()
            {
                Parent = Playfield.BackgroundContainer,
                Image = Skin.StageLeftBorder,
                Size = new ScalableVector2(stageLeftX, WindowManager.Height),
                Position = new ScalableVector2(-stageLeftX + 1, 0),
                Alignment = Alignment.TopLeft
            };
        }

        /// <summary>
        ///     Creates the right side of the stage.
        /// </summary>
        private void CreateStageRight()
        {
            // Create the right side of the stage.
            var stageRightX = Skin.StageRightBorder.Width * WindowManager.Height / Skin.StageRightBorder.Height;

            StageRight = new Sprite
            {
                Image = Skin.StageRightBorder,
                Size = new ScalableVector2(stageRightX, WindowManager.Height),
                Position = new ScalableVector2(stageRightX - 1, 0),
                Parent = Playfield.BackgroundContainer,
                Alignment = Alignment.TopRight
            };
        }

        /// <summary>
        ///     Creates the BG Mask of the stage.
        /// </summary>
        private void CreateBgMask()
        {
            var imageRatio = (double)Skin.StageBgMask.Width / Skin.StageBgMask.Height;
            var columnRatio = Playfield.Width / WindowManager.Height;
            var bgMaskSize = (float)Math.Max(WindowManager.Height * columnRatio / imageRatio, WindowManager.Height);

            BgMask = new Sprite
            {
                Image = Skin.StageBgMask,
                Alpha = Skin.BgMaskAlpha,
                Size = new ScalableVector2(Playfield.Width, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = Playfield.BackgroundContainer
            };
        }

        /// <summary>
        ///     Creates the HitPositionOverlay for the stage.
        /// </summary>
        private void CreateHitPositionOverlay()
        {
            // Create Stage HitPosition Overlay
            var sizeY = Skin.StageHitPositionOverlay.Height * Playfield.Width / Skin.StageHitPositionOverlay.Width;
            var offsetY = Playfield.LaneSize * ((float)Skin.NoteReceptorsUp[0].Height / Skin.NoteReceptorsUp[0].Width);
            var width = Playfield.Width;

            float y;
            switch (GameplayRulesetKeys.ScrollDirection)
            {
                case ScrollDirection.Down:
                    y = Playfield.ReceptorPositionY.First() - sizeY + Skin.HitPosOffsetY;
                    break;
                case ScrollDirection.Up:
                    y = Playfield.ReceptorPositionY.First() + offsetY - Skin.HitPosOffsetY;
                    break;
                case ScrollDirection.Split:
                    y = Playfield.ReceptorPositionY.First() - sizeY + Skin.HitPosOffsetY;
                    width = Playfield.Width / 2;

                    var splitHitPositionOverlay = new Sprite
                    {
                        Parent = Playfield.ForegroundContainer,
                        Image = Skin.StageHitPositionOverlay,
                        Rotation = 180,
                        Size = new ScalableVector2(width, sizeY),
                        X = width,
                        Y = Playfield.ReceptorPositionY.Last() + offsetY - Skin.HitPosOffsetY
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            HitPositionOverlay = new Sprite
            {
                Parent = Playfield.ForegroundContainer,
                Image = Skin.StageHitPositionOverlay,
                Rotation = GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Up) ? 180 : 0,
                Size = new ScalableVector2(width, sizeY),
                Y = y
            };
        }

        /// <summary>
        ///     Creates the receptors and column lighting
        /// </summary>
        private void CreateReceptorsAndLighting()
        {
            Receptors = new List<Sprite>();
            ColumnLightingObjects = new List<ColumnLighting>();

            // Go through and create the receptors and column lighting objects.
            for (var i = 0; i < Screen.Map.GetKeyCount(); i++)
            {
                var scale = ConfigManager.GameplayNoteScale.Value / 100f;

                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (scale != 1)
                    posX += (Playfield.LaneSize - Playfield.LaneSize * scale) / 2f;

                // Create individiaul receptor.
                Receptors.Add(new Sprite
                {
                    Parent = Playfield.ForegroundContainer,
                    Size = new ScalableVector2(Playfield.LaneSize * scale, (Playfield.LaneSize * Skin.NoteReceptorsUp[i].Height / Skin.NoteReceptorsUp[i].Width) * scale),
                    Position = new ScalableVector2(posX, Playfield.ReceptorPositionY[i]),
                    Alignment = Alignment.TopLeft,
                    Image = Skin.NoteReceptorsUp[i],
                    // todo: case statement for scroll direction
                    SpriteEffect = !Playfield.ScrollDirections[i].Equals(ScrollDirection.Down) && Skin.FlipNoteImagesOnUpscroll ? SpriteEffects.FlipVertically : SpriteEffects.None,
                });

                // Create the column lighting sprite.
                var size = Skin.ColumnLightingScale * Playfield.LaneSize * ((float)Skin.ColumnLighting.Height / Skin.ColumnLighting.Width);
                ColumnLightingObjects.Add(new ColumnLighting
                {
                    Parent = Playfield.BackgroundContainer,
                    Image = Skin.ColumnLighting,
                    Size = new ScalableVector2(Playfield.LaneSize, size),
                    Tint = Skin.ColumnColors[i],
                    X = posX,
                    Y = Playfield.ColumnLightingPositionY[i],
                    // todo: case statement for scroll direction
                    SpriteEffect = !Playfield.ScrollDirections[i].Equals(ScrollDirection.Down) && Skin.FlipNoteImagesOnUpscroll ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Alignment = Alignment.TopLeft,
                });
            }
        }

        /// <summary>
        ///     Creates the HitObjectContainer
        /// </summary>
        private void CreateHitObjectContainer() => HitObjectContainer = new Container
        {
            Size = new ScalableVector2(Playfield.Width, 0, 0, 1),
            Alignment = Alignment.TopCenter,
            Parent = Playfield.ForegroundContainer
        };

        /// <summary>
        ///     Creates the TimingLineContainer
        /// </summary>
        private void CreateTimingLineContainer() => TimingLineContainer = new Container
        {
            Size = new ScalableVector2(Playfield.Width, 0, 0, 1),
            Alignment = Alignment.TopCenter,
            Parent = Playfield.ForegroundContainer
        };

        /// <summary>
        ///     Creates the HitContainer
        /// </summary>
        private void CreateHitContainer() => HitContainer = new Container
        {
            Size = new ScalableVector2(Playfield.Width, 0, 0, 1),
            Alignment = Alignment.TopCenter,
            Parent = Playfield.ForegroundContainer
        };

        /// <summary>
        ///     Creates the distant overlay sprite.
        /// </summary>
        private void CreateDistantOverlay()
        {
            var sizeY = Skin.StageDistantOverlay.Height * Playfield.Width / Skin.StageDistantOverlay.Width;
            DistantOverlay = new Sprite
            {
                Image = Skin.StageDistantOverlay,
                Size = new ScalableVector2(Playfield.Width, sizeY),
                // todo: case statement for scroll direction
                Y = GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Down) ? -1 : 1,
                // todo: case statement for scroll direction
                Alignment = GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Down) ? Alignment.TopRight : Alignment.BotRight,
                Parent = Playfield.ForegroundContainer
            };
        }

        /// <summary>
        ///     Creates the lane cover container and overlay sprites.
        /// </summary>
        private void CreateLaneCoverOverlay()
        {
            LaneCoverContainer = new Container
            {
                Size = new ScalableVector2(Playfield.ForegroundContainer.Width, Playfield.ForegroundContainer.Height, 0, 0),
                Alignment = Alignment.TopLeft,
                Parent = Playfield.ForegroundContainer
            };

            // Apply the covers.
            ApplyTopLaneCover();
            ApplyBottomLaneCover();
        }

        /// <summary>
        ///     Creates the combo display sprite.
        /// </summary>
        private void CreateComboDisplay()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            // Create the combo display.
            ComboDisplay = new NumberDisplay(NumberDisplayType.Combo, "0", new Vector2(skin.ComboDisplayScale / 100f, skin.ComboDisplayScale / 100f))
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
                X = Skin.ComboPosX,
                Y = Skin.ComboPosY
            };

            OriginalComboDisplayY = ComboDisplay.Y;

            // Start off the map by making the display invisible.
            ComboDisplay.MakeInvisible();
        }

        /// <summary>
        ///     Updates the combo display and its animations
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateComboDisplay(GameTime gameTime)
        {
            // Gradually tween the position back to what it was originally.
            ComboDisplay.Y = MathHelper.Lerp(ComboDisplay.Y, OriginalComboDisplayY, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 30, 1) / 2);

            if (OldCombo == Screen.Ruleset.ScoreProcessor.Combo)
                return;

            // Set the new one
            ComboDisplay.UpdateValue(Screen.Ruleset.ScoreProcessor.Combo);

            // Set the position and scale  of the combo display, so that we can perform some animations.
            ComboDisplay.Y = OriginalComboDisplayY - 5;

            // Gradually tween the position back to what it was originally.
            ComboDisplay.Y = MathHelper.Lerp(ComboDisplay.Y, OriginalComboDisplayY, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 30, 1) / 2);
            OldCombo = Screen.Ruleset.ScoreProcessor.Combo;
        }

        /// <summary>
        ///     Creates the HitError Sprite.
        /// </summary>
        private void CreateHitError() => HitError = new HitErrorBar(new ScalableVector2(50, Skin.HitErrorHeight))
        {
            Parent = Playfield.ForegroundContainer,
            Alignment = Alignment.MidCenter,
            Position = new ScalableVector2(Skin.HitErrorPosX, Skin.HitErrorPosY)
        };

        /// <summary>
        ///     Creates the JudgementHitBurst sprite.
        /// </summary>
        private void CreateJudgementHitBurst()
        {
            // Default the frames to miss.
            var frames = SkinManager.Skin.Judgements[Judgement.Miss];

            // Grab the first frame for convenience.
            var firstFrame = frames[0];

            // Set size w/ scaling.
            var size = new Vector2(firstFrame.Width, firstFrame.Height) * Skin.JudgementHitBurstScale / firstFrame.Height;

            JudgementHitBurst = new JudgementHitBurst(Screen, frames, size, Skin.JudgementBurstPosY)
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
            };
        }

        /// <summary>
        ///     Creates the hitlighting sprites.
        /// </summary>
        private void CreateHitLighting()
        {
            HitLightingObjects = new List<HitLighting>();

            for (var i = 0; i < Screen.Map.GetKeyCount(); i++)
            {
                var hl = new HitLighting()
                {
                    Parent = Playfield.ForegroundContainer,
                    Visible = false,
                    Size = new ScalableVector2(Skin.HitLightingWidth, Skin.HitLightingHeight),
                    Position = new ScalableVector2(Skin.HitLightingX, Skin.HitLightingY)
                };

                var pos = GraphicsHelper.AlignRect(Alignment.MidCenter, hl.RelativeRectangle,
                    Receptors[i].ScreenRectangle);

                hl.X = pos.X - Playfield.ForegroundContainer.ScreenRectangle.X;
                hl.Y = pos.Y - Playfield.ForegroundContainer.ScreenRectangle.Y;

                // Set the spritebatch options for the hitlighting in this case
                // if it's the first object.
                if (i == 0)
                    hl.SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.Additive };
                // Use the previous object's spritebatch options so all of them use the same batch.
                else
                    hl.UsePreviousSpriteBatchOptions = true;

                HitLightingObjects.Add(hl);
            }
        }

        /// <summary>
        ///     Creates the health bar for the screen.
        /// </summary>
        private void CreateHealthBar()
        {
            var scale = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HealthBarScale;
            HealthBar = new HealthBar(SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HealthBarType,
                Playfield.Ruleset.ScoreProcessor, new Vector2(scale / 100f, scale / 100f))
            {
                Parent = Playfield.ForegroundContainer,
            };

            switch (SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HealthBarKeysAlignment)
            {
                case HealthBarKeysAlignment.LeftStage:
                    HealthBar.Parent = StageLeft;
                    HealthBar.X = -5;
                    HealthBar.Y = -10;
                    break;
                case HealthBarKeysAlignment.RightStage:
                    HealthBar.Parent = StageRight;
                    HealthBar.X = 5;
                    HealthBar.Y = -10;
                    break;
                case HealthBarKeysAlignment.TopLeft:
                    HealthBar.Parent = Playfield.Container;
                    HealthBar.Alignment = Alignment.TopLeft;
                    break;
            }

            HealthBar.X += Skin.HealthBarPosOffsetX;
            HealthBar.Y += Skin.HealthBarPosOffsetY;
        }

        /// <summary>
        ///     Creates the sprite that displays the song information.
        /// </summary>
        private void CreateSongInfo() => SongInfo = new SongInformation(Screen)
        {
            Parent = Playfield.ForegroundContainer,
            Alignment = Alignment.MidCenter,
            Y = -200
        };

        /// <summary>
        ///     Creates the sprite that displays battle royale alerts
        /// </summary>
        private void CreateBattleRoyaleAlert()
        {
            var width = SkinManager.Skin.BattleRoyaleWarning.Width;
            var height = SkinManager.Skin.BattleRoyaleWarning.Height;
            var size = new Vector2(width, height) * Skin.BattleRoyaleAlertScale / height;

            BattleRoyaleAlert = new BattleRoyaleAlert(Screen)
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
                Position = new ScalableVector2(Skin.BattleRoyaleAlertPosX, Skin.BattleRoyaleAlertPosY),
                Size = new ScalableVector2(size.X, size.Y),
            };
        }

        private void CreateBattleRoyaleEliminated() => BattleRoyalePlayerEliminated = new BattleRoyalePlayerEliminated(Screen)
        {
            Parent = Playfield.ForegroundContainer,
            Alignment = Alignment.MidCenter,
            Position = new ScalableVector2(Skin.BattleRoyaleEliminatedPosX, Skin.BattleRoyaleEliminatedPosY)
        };

        /// <summary>
        ///     Updates the given receptor and column lighting activity
        ///     (Called when pressing/releasing keys.)
        /// </summary>
        public void SetReceptorAndLightingActivity(int index, bool pressed)
        {
            Receptors[index].Image = pressed ? Skin.NoteReceptorsDown[index] : Skin.NoteReceptorsUp[index];
            ColumnLightingObjects[index].Active = pressed;
        }

        /// <summary>
        ///     Applies a top lane cover if enabled in settings.
        /// </summary>
        private void ApplyTopLaneCover()
        {
            if (!ConfigManager.LaneCoverTop.Value)
                return;

            var yAxis = -LaneCoverContainer.Height + ConfigManager.LaneCoverTopHeight.Value / 100f * LaneCoverContainer.Height;

            var laneCoverTop = new Sprite
            {
                Image = Skin.LaneCoverTop,
                Y = yAxis,
                Size = new ScalableVector2(LaneCoverContainer.Width, 700),
                Alignment = Alignment.BotLeft,
                Parent = LaneCoverContainer,
            };
        }

        /// <summary>
        ///     Applies the bottom lane cover if enabled in settings.
        /// </summary>
        private void ApplyBottomLaneCover()
        {
            if (!ConfigManager.LaneCoverBottom.Value)
                return;

            var yAxis = LaneCoverContainer.Height - ConfigManager.LaneCoverBottomHeight.Value / 100f * LaneCoverContainer.Height;

            var laneCoverBottom = new Sprite
            {
                Image = Skin.LaneCoverBottom,
                Y = yAxis,
                Size = new ScalableVector2(LaneCoverContainer.Width, 700),
                Alignment = Alignment.TopLeft,
                Parent = LaneCoverContainer,
            };
        }

        public void FadeIn()
        {
            const int time = 400;
            const Easing easing = Easing.Linear;

            BgMask.Alpha = 0;
            BgMask.FadeTo(1, Easing.Linear, time);

            Receptors.ForEach(x =>
            {
                x.Alpha = 0;
                x.FadeTo(1, Easing.Linear, time);
            });

            ComboDisplay.Digits.ForEach(x =>
            {
                x.Alpha = 0;
                x.FadeTo(1, Easing.Linear, time);
            });

            HitObjectContainer.Children.ForEach(x =>
            {
                if (x is Sprite sprite)
                {
                    sprite.Alpha = 0;
                    sprite.FadeTo(1, Easing.Linear, time - 200);
                }
            });

            HitError.Children.ForEach(x =>
            {
                if (x is Sprite sprite)
                {
                    sprite.Alpha = 0;
                    sprite.FadeTo(1, Easing.Linear, time);
                }
            });

            StageLeft.Alpha = 0;
            StageLeft.FadeTo(1, Easing.Linear, time);

            StageRight.Alpha = 0;
            StageRight.FadeTo(1, Easing.Linear, time);
        }
    }
}
