using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.Skinning;

namespace Quaver.States.Gameplay.UI
{
    internal class HitErrorBar : QuaverContainer
    {
        /// <summary>
        ///     The type of hit error this is.
        /// </summary>
        internal HitErrorType Type { get; }

        /// <summary>
        ///     The middle 0ms line for the hit error bar.
        /// </summary>
        private QuaverSprite MiddleLine { get; }

        /// <summary>
        ///     The size of the hit error object pool.
        /// </summary>
        private int PoolSize { get; } = 32;

        /// <summary>
        ///     The list of lines that are currently in the hit error.
        /// </summary>
        private List<QuaverSprite> LineObjectPool { get; }

        /// <summary>
        ///     The current index we're in within the object pool.
        ///     Initialized to -1 because we add to it each time we add a judgement.
        /// </summary>
        private int CurrentLinePoolIndex { get; set; } = -1;

        /// <summary>
        ///     the average hit chevron.
        /// </summary>
        private QuaverSprite AverageHitCheveron { get;  }

        /// <summary>
        ///     The current average hit.
        /// </summary>
        private float AverageHit { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///   Ctor -  
        /// </summary>
        /// <param name="type"></param>
        /// <param name="size"></param>
        internal HitErrorBar(HitErrorType type, UDim2D size)
        {
            Type = type;
            Size = size;

            switch (Type)
            {
                case HitErrorType.Quaver:
                    // Create the middle line bar.
                    MiddleLine = new QuaverSprite()
                    {
                        Size = new UDim2D(2, 0, 0, 1),
                        Alignment = Alignment.MidCenter,
                        Parent = this
                    };
                    break;
                case HitErrorType.Legacy:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Create the object pool and initialize all of the sprites.
            LineObjectPool = new List<QuaverSprite>();          
            for (var i = 0; i < PoolSize; i++)
            {
                LineObjectPool.Add(new QuaverSprite()
                {
                    Parent = this,
                    Size = new UDim2D(4, 0, 0, 1),
                    Alignment = Alignment.MidCenter,
                    Alpha = 0
                });
            }

            // Create the average hit chevron.
            AverageHitCheveron = new QuaverSprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = 1,
                Image = FontAwesome.CaretDown,
                PosY = -SizeY - 3,
                Size = new UDim2D(8, 8)
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Gradually fade out the line.
            foreach (var line in LineObjectPool)
                line.Alpha = GraphicsHelper.Tween(0, line.Alpha, Math.Min(dt / 960, 1));
            
            // Tween the chevron to the last hit
            if (CurrentLinePoolIndex != -1)
                AverageHitCheveron.PosX = GraphicsHelper.Tween(LineObjectPool[CurrentLinePoolIndex].PosX, AverageHitCheveron.PosX, Math.Min(dt / 360, 1));
            
            base.Update(dt);
        }

        /// <summary>
        ///     Adds a judgement to the hit error at a given hit time.
        /// </summary>
        internal void AddJudgement(Judgement j, double hitTime)
        {
            CurrentLinePoolIndex++;

            if (CurrentLinePoolIndex >= PoolSize)
                CurrentLinePoolIndex = 0;

            LineObjectPool[CurrentLinePoolIndex].Tint = GameBase.LoadedSkin.JudgeColors[(int) j];
            LineObjectPool[CurrentLinePoolIndex].PosX = -(float) hitTime;
            LineObjectPool[CurrentLinePoolIndex].Alpha = 0.5f;
        }
    }

    /// <summary>
    ///     The type of hit error that will be displayed to the user.
    /// </summary>
    internal enum HitErrorType
    {
        Quaver,  // Normal, default styled.
        Legacy // osu! styled hit error bar.
    }
}