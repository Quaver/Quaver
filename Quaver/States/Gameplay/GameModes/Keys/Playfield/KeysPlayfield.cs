using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.States.Gameplay.UI.Components.Health;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class KeysPlayfield : IGameplayPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Container Container { get; set; }

        /// <summary>
        ///     The background of the playfield.
        /// </summary>
        internal Container BackgroundContainer { get; }
        
        /// <summary>
        ///     The foreground of the playfield.
        /// </summary>
        internal Container ForegroundContainer { get; }

        /// <summary>
        ///     The special container for hit lighting.
        ///     We create an entirely new container for this so that we can
        ///     draw it under a new spritebatch to give it an additive blendstate.
        /// </summary>
        internal Container HitLightingContainer { get; }

        /// <summary>
        ///     Reference to the map.
        /// </summary>
        internal Qua Map { get; }

        /// <summary>
        ///     The stage for this playfield.
        /// </summary>
        internal KeysPlayfieldStage Stage { get; }

        /// <summary>
        ///     The X size of the playfield.
        /// </summary>
        internal float Width => (LaneSize + ReceptorPadding) * Map.FindKeyCountFromMode() + Padding * 2 - ReceptorPadding;

        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The health bar for the playfield.
        /// </summary>
        private HealthBar HealthBar { get; }

        /// <summary>
        ///     Padding of the playfield.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal float Padding
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return (int)(GameBase.LoadedSkin.BgMaskPadding4K * GameBase.WindowUIScale);
                    case GameMode.Keys7:
                        return (int)(GameBase.LoadedSkin.BgMaskPadding7K * GameBase.WindowUIScale);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        
        /// <summary>
        ///     The size of the each ane.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal float LaneSize
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return (int)(GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale);
                    case GameMode.Keys7:
                        return (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
       
        /// <summary>
        ///     Padding of the receptor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal float ReceptorPadding
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return (int)(GameBase.LoadedSkin.NotePadding4K * GameBase.WindowUIScale);
                    case GameMode.Keys7:
                        return (int)(GameBase.LoadedSkin.NotePadding7K * GameBase.WindowUIScale);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     The Y position of the receptors.
        /// </summary>
        internal float ReceptorPositionY
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        if (ConfigManager.DownScroll4K.Value)
                            return GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale + LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width);
                        else
                            return GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale;
                    case GameMode.Keys7:
                        if (ConfigManager.DownScroll7K.Value)
                            return GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale + LaneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width);
                        else
                            return GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     The Y position of the column lighting
        /// </summary>
        internal float ColumnLightingPositionY
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        if (ConfigManager.DownScroll4K.Value)
                            return ReceptorPositionY;
                        else
                        {
                            var receptor = GameBase.LoadedSkin.NoteReceptorsUp4K[0];
                            var hitObject = GameBase.LoadedSkin.NoteHitObjects4K[0][0];
                            
                            return ReceptorPositionY + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
                        }
                    case GameMode.Keys7:
                        if (ConfigManager.DownScroll7K.Value)
                            return ReceptorPositionY;
                        else
                        {
                            var receptor = GameBase.LoadedSkin.NoteReceptorsUp7K[0];
                            var hitObject = GameBase.LoadedSkin.NoteHitObjects7K[0][0];
                            
                            return ReceptorPositionY + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal KeysPlayfield(GameplayScreen screen)
        {
            Screen = screen;
            Map = Screen.Map;
            
            // Create the playfield's container.
            Container = new Container();
            
            // Create background container
            BackgroundContainer = new Container
            {
                Parent = Container,
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
                        
            // Create the foreground container.
            ForegroundContainer = new Container
            {
                Parent = Container,
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
            
            // Create container for hit lighting
            HitLightingContainer = new Container
            {
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
            
            // Create a new playfield stage               
            Stage = new KeysPlayfieldStage(this, Screen);
            
            // Create health bar.
            HealthBar = new HealthBar(HealthBarType.Horizontal, Screen.Ruleset.ScoreProcessor);
        }
        
        /// <summary>
        ///     Init
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            HealthBar.Initialize(state);
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
            HitLightingContainer.Destroy();
            HealthBar.UnloadContent();
        }
        
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Update the stage
            Stage.Update(dt);
        
            Container.Update(dt);
            HitLightingContainer.Update(dt);
            HealthBar.Update(dt);
        }

        /// <summary>
        ///     Destroy
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Container.Draw();
            GameBase.SpriteBatch.End();
            
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            HitLightingContainer.Draw();
            GameBase.SpriteBatch.End();   
            
            // Draw health bar (Has its own spritebatch system).
            HealthBar.Draw();
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void HandleFailure(double dt)
        {
            FadeOut(dt);
        }

        /// <summary>
        ///     Fades out the playfield & entire stage if specified.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fadeStage"></param>
        private void FadeOut(double dt, bool fadeStage = false)
        {
            // Fade out all of the active HitObjects.
            var manager = (KeysHitObjectManager) Screen.Ruleset.HitObjectManager;

            // Fade out the objects in the pool.
            for (var i = 0; i < manager.ObjectPool.Count && i < manager.PoolSize; i++)
            {
                var o = (KeysHitObject) manager.ObjectPool[i];
                o.FadeOut(dt);
            }
            
            foreach (var hitObject in manager.HeldLongNotes)
            {
                var o = (KeysHitObject) hitObject;
                o.FadeOut(dt);
            }
            
            foreach (var hitObject in manager.DeadNotes)
            {
                var o = (KeysHitObject) hitObject;
                o.FadeOut(dt);
            }
            
            if (fadeStage)
                Stage.FadeOut(dt);
        }
    }
}