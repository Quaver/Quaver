/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Health;
using Quaver.Shared.Screens.Gameplay.UI;
using Quaver.Shared.Screens.Gameplay.UI.Health;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
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
        ///     Sprite that displays the current combo.
        /// </summary>
        public NumberDisplay ComboDisplay { get; private set; }

        /// <summary>
        ///     The original value for the combo display's Y position,
        ///     so we can use this to set it back after it's done with its animation.
        /// </summary>
        private float OriginalComboDisplayY { get; set; }

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
        ///     Make a quicker and shorter reference to the game skin
        /// </summary>
        private SkinKeys Skin => SkinManager.Skin.Keys[Screen.Map.Mode];

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
            CreateHitPositionOverlay();

            // Depending on what the skin.ini's value is, we'll want to either initialize
            // the receptors first, or the playfield first.
            if (Skin.ReceptorsOverHitObjects)
            {
                CreateTimingLineContainer();
                CreateHitObjectContainer();
                CreateReceptorsAndLighting();
            }
            else
            {
                CreateReceptorsAndLighting();
                CreateTimingLineContainer();
                CreateHitObjectContainer();
            }

            // Create Stage Elements
            CreateDistantOverlay();
            CreateComboDisplay();
            CreateHitError();
            CreateJudgementHitBurst();
            CreateHitLighting();
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

            HitPositionOverlay = new Sprite
            {
                Parent = Playfield.ForegroundContainer,
                Image = Skin.StageHitPositionOverlay,
                Size = new ScalableVector2(Playfield.Width, sizeY),
                // todo: case statement for scroll direction
                Y = GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Down) ? Playfield.ReceptorPositionY[0] + Skin.HitPosOffsetY
                                                    : Playfield.ReceptorPositionY[0] + offsetY + sizeY - Skin.HitPosOffsetY,
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
                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;

                // Create individiaul receptor.
                Receptors.Add(new Sprite
                {
                    Parent = Playfield.ForegroundContainer,
                    Size = new ScalableVector2(Playfield.LaneSize, Playfield.LaneSize * Skin.NoteReceptorsUp[i].Height / Skin.NoteReceptorsUp[i].Width),
                    Position = new ScalableVector2(posX, Playfield.ReceptorPositionY[i]),
                    Alignment = Alignment.TopLeft,
                    Image = Skin.NoteReceptorsUp[i],
                    // todo: case statement for scroll direction
                    SpriteEffect = !GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Down) && Skin.FlipNoteImagesOnUpscroll ? SpriteEffects.FlipVertically : SpriteEffects.None,
                });

                // Create the column lighting sprite.
                var lightingY = Skin.ColumnLightingScale * Playfield.LaneSize * ((float)Skin.ColumnLighting.Height / Skin.ColumnLighting.Width);

                ColumnLightingObjects.Add(new ColumnLighting
                {
                    Parent = Playfield.BackgroundContainer,
                    Image = Skin.ColumnLighting,
                    Size = new ScalableVector2(Playfield.LaneSize, lightingY),
                    Tint = Skin.ColumnColors[i],
                    X = posX,
                    // todo: case statement for scroll direction
                    Y = GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Down) ? Playfield.ColumnLightingPositionY[i] - lightingY : Playfield.ColumnLightingPositionY[i],
                    // todo: case statement for scroll direction
                    SpriteEffect = !GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.Down) && Skin.FlipNoteImagesOnUpscroll ? SpriteEffects.FlipVertically : SpriteEffects.None,
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
        ///     Creates the combo display sprite.
        /// </summary>
        private void CreateComboDisplay()
        {
            // Create the combo display.
            ComboDisplay = new NumberDisplay(NumberDisplayType.Combo, "0", new Vector2(1, 1))
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
                Y = Skin.ComboPosY
            };

            OriginalComboDisplayY = ComboDisplay.Y;
            ComboDisplay.X = -ComboDisplay.TotalWidth / 2f;

            // Start off the map by making the display invisible.
            ComboDisplay.MakeInvisible();
        }

        /// <summary>
        ///     Updates the combo display and its animations
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateComboDisplay(GameTime gameTime)
        {
            // Grab the old value
            var oldCombo = ComboDisplay.Value;

            // Set the new one
            ComboDisplay.Value = Screen.Ruleset.ScoreProcessor.Combo.ToString();

            // If the combo needs repositioning, do so accordingly.
            if (oldCombo.Length != ComboDisplay.Value.Length)
                ComboDisplay.X = -ComboDisplay.TotalWidth / 2f;

            // Set the position and scale  of the combo display, so that we can perform some animations.
            if (oldCombo != ComboDisplay.Value && ComboDisplay.Visible)
                ComboDisplay.Y = OriginalComboDisplayY - 5;

            // Gradually tween the position back to what it was originally.
            ComboDisplay.Y = MathHelper.Lerp(ComboDisplay.Y, OriginalComboDisplayY, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 30, 1) / 2);
        }

        /// <summary>
        ///     Creates the HitError Sprite.
        /// </summary>
        private void CreateHitError() => HitError = new HitErrorBar(new ScalableVector2(50, 10))
        {
            Parent = Playfield.ForegroundContainer,
            Alignment = Alignment.MidCenter,
            Position = new ScalableVector2(0, 55)
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

            JudgementHitBurst = new JudgementHitBurst(frames, size, Skin.JudgementBurstPosY)
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
                var skin = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode];
                var reference = skin.HitLighting[0];
                var scale = reference.Width / Receptors[i].Width * skin.HitLightingScale;
                var height = Playfield.LaneSize * reference.Height / reference.Width;
                var hl = new HitLighting()
                {
                    Parent = Receptors[i],
                    Alignment = Alignment.TopCenter,
                    Width = Playfield.LaneSize * scale,
                    Height = height * scale,
                    Y = MapManager.Selected.Value.Mode.Equals(ScrollDirection.Down) ? - (skin.HitLightingY + height) * scale / 4 : (skin.HitLightingY - height) * scale / 4,
                    Rotation = HitObjectManagerKeys.HitObjectRotations[Playfield.Ruleset.Mode][i],
                    Visible = false
                };

                // Set the spritebatch options for the hitlighting in this case
                // if it's the first object.
                if (i == 0)
                    hl.SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.Additive };
                // Use the previous object's spritebatch options so all of them use the same batch.
                else
                    hl.UsePreviousSpriteBatchOptions = true;

                Console.WriteLine(hl.Size.X +", "+hl.Size.Y);
                HitLightingObjects.Add(hl);
            }
        }

        /// <summary>
        ///     Creates the health bar for the screen.
        /// </summary>
        private void CreateHealthBar()
        {
            HealthBar = new HealthBar(SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HealthBarType, Playfield.Ruleset.ScoreProcessor)
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Updates the given receptor and column lighting activity
        ///     (Called when pressing/releasing keys.)
        /// </summary>
        public void SetReceptorAndLightingActivity(int index, bool pressed)
        {
            Receptors[index].Image = pressed ? Skin.NoteReceptorsDown[index] : Skin.NoteReceptorsUp[index];
            ColumnLightingObjects[index].Active = pressed;
        }
    }
}
