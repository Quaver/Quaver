using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;

namespace Quaver.Graphics.Sprites
{
    /// <inheritdoc />
    /// <summary>
    ///     An animatable sprite. When using this, it is important NOT to manually set the image property.
    /// </summary>
    internal class AnimatableSprite : Sprite
    {
        /// <summary>
        ///     The animation frames 
        /// </summary>
        internal List<Texture2D> Frames { get; }
        
        /// <summary>
        ///     The current animation frame we're on.
        /// </summary>
        internal int CurrentFrame { get; private set; }

        /// <summary>
        ///     If the animation is currently looping.
        /// </summary>
        internal bool IsLooping { get; private set; }

        /// <summary>
        ///     Animation frame time.
        /// </summary>
        internal int LoopFramesPerSecond { get; private set; }

        /// <summary>
        ///     The amount of time since the last frame in the animation.
        /// </summary>
        internal double TimeSinceLastFrame { get; private set; }

        /// <summary>
        ///     The direction the animations will loop.
        /// </summary>
        internal LoopDirection Direction { get; set; }

        /// <summary>
        ///     Ctor - if you only have the image itself, but also the rows and columns
        /// </summary>
        /// <param name="spritesheet"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        internal AnimatableSprite(Texture2D spritesheet, int rows, int columns)
        {
            Frames = GraphicsHelper.LoadSpritesheetFromTexture(spritesheet, rows, columns);
            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Ctor - If you already have the animation frames.
        /// </summary>
        /// <param name="frames"></param>
        internal AnimatableSprite(List<Texture2D> frames)
        {
            Frames = frames;
            Image = Frames[CurrentFrame];
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            if (IsLooping)
            {
                TimeSinceLastFrame += dt;

                if (TimeSinceLastFrame >= 1000f / LoopFramesPerSecond)
                {
                    switch (Direction)
                    {
                        case LoopDirection.Forward:
                            ChangeToNext();
                            break;
                        case LoopDirection.Backward:
                            ChangeToPrevious();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    TimeSinceLastFrame = 0;
                }
            }
                
            base.Update(dt);
        }

        /// <summary>
        ///     Changes the sprite's image to a specified frame.
        /// </summary>
        /// <param name="i"></param>
        /// <exception cref="ArgumentException"></exception>
        internal void ChangeTo(int i)
        {
            if (i > Frames.Count)
                throw new ArgumentException("Cannot change to frame greater than the size of the list.");

            CurrentFrame = i;
            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Changes the sprites image to the next frame.
        /// </summary>
        internal void ChangeToNext()
        {
            if (CurrentFrame + 1 > Frames.Count - 1)
                CurrentFrame = 0;
            else
                CurrentFrame++;

            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Changes the sprite to the previous frame.
        /// </summary>
        internal void ChangeToPrevious()
        {
            if (CurrentFrame - 1 <= 0)
                CurrentFrame = Frames.Count - 1;
            else
                CurrentFrame--;

            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Adds a frame to the list 
        /// </summary>
        /// <param name="frame"></param>
        internal void Add(Texture2D frame)
        {
            Frames.Add(frame);
        }

        /// <summary>
        ///     Removes a frame from the list.
        /// </summary>
        /// <param name="frame"></param>
        internal void Remove(Texture2D frame)
        {
            Frames.Remove(frame);
        }

        /// <summary>
        ///     Removes a frame a given index.
        /// </summary>
        /// <param name="i"></param>
        /// <exception cref="ArgumentException"></exception>
        internal void RemoveAt(int i)
        {
            if (i > Frames.Count || i < 0)
                throw new ArgumentException("Index specified must be in range of the list.");
            
            if (CurrentFrame == i)
                ChangeToNext();
            
            Frames.RemoveAt(i);
        }

        /// <summary>
        ///     Start the animation frame loop at a given FPS.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fps"></param>
        internal void StartLooping(LoopDirection direction, int fps)
        {
            Direction = direction;
            LoopFramesPerSecond = fps;
            IsLooping = true;
        }

        /// <summary>
        ///     To stop the animation frame loop.
        /// </summary>
        internal void StopLooping() => IsLooping = false;
    }

    internal enum LoopDirection
    {
        Forward,
        Backward
    }
}