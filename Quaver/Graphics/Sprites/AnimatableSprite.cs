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
        internal List<Texture2D> Frames { get; set; }
        
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
        ///     The given frame the loop began on.
        /// </summary>
        private int FrameLoopStartedOn { get; set; }

        /// <summary>
        ///     The amount of times to loop.
        /// </summary>
        internal int TimesToLoop { get; private set; }
        
        /// <summary>
        ///     The amount of times looped so far.
        /// </summary>
        internal int TimesLooped { get; private set; }

        /// <summary>
        ///     Emitted when the sprite has finished its loop.
        /// </summary>
        internal EventHandler FinishedLooping { get; set; }

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
            PerformLoopAnimation(dt);                
            base.Update(dt);
        }

        /// <summary>
        ///     Changes the sprite's image to a specified frame.
        /// </summary>
        /// <param name="i"></param>
        /// <exception cref="ArgumentException"></exception>
        internal void ChangeTo(int i)
        {
            if (i > Frames.Count || i < 0)
                throw new ArgumentOutOfRangeException();

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
            if (CurrentFrame - 1 < 0)
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
                throw new ArgumentOutOfRangeException();
            
            if (CurrentFrame == i)
                ChangeToNext();
            
            Frames.RemoveAt(i);
        }

        /// <summary>
        ///     Start the animation frame loop at a given FPS.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fps"></param>
        /// <param name="timesToLoop">The amount of times to loop. If 0, it'll loop infinitely.</param>
        internal void StartLoop(LoopDirection direction, int fps, int timesToLoop = 0)
        {
            Direction = direction;
            LoopFramesPerSecond = fps;
            IsLooping = true;
            FrameLoopStartedOn = CurrentFrame;
            TimesLooped = 0;
            TimesToLoop = timesToLoop;
        }

        /// <summary>
        ///     To stop the animation frame loop.
        /// </summary>
        internal void StopLoop() => IsLooping = false;

        /// <summary>
        ///    Replaces all the frames with some new ones. 
        /// </summary>
        /// <param name="newFrames"></param>
        /// <exception cref="ArgumentException"></exception>
        internal void ReplaceFrames(List<Texture2D> newFrames)
        {
            if (newFrames.Count == 0)
                throw new ArgumentException("The new frames added must be greater than 0.");

            Frames = newFrames;
            ChangeTo(0);
        }
        
        /// <summary>
        ///     Handles the looping of the animation frames.
        /// </summary>
        /// <param name="dt"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void PerformLoopAnimation(double dt)
        {
            if (!IsLooping || Frames.Count <= 1)
                return;
            
            TimeSinceLastFrame += dt;

            if (!(TimeSinceLastFrame >= 1000f / LoopFramesPerSecond)) 
                return;
            
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

            // If we're back on the frame we've started on, then we need to increment our counter.
            if (FrameLoopStartedOn != CurrentFrame) 
                return;
            
            TimesLooped++;
            FinishedLooping?.Invoke(this, null);
                
            // Automatically stop the loop if we've looped the specified amount of times.
            if (TimesToLoop != 0 && TimesLooped == TimesToLoop)
                StopLoop();
        }
    }

    internal enum LoopDirection
    {
        Forward,
        Backward
    }
}