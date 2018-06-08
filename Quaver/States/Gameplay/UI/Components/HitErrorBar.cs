using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components
{
    internal class HitErrorBar : Container
    {
        /// <summary>
        ///     The middle 0ms line for the hit error bar.
        /// </summary>
        internal Sprite MiddleLine { get; }

        /// <summary>
        ///     The size of the hit error object pool.
        /// </summary>
        private int PoolSize { get; } = 32;

        /// <summary>
        ///     The list of lines that are currently in the hit error.
        /// </summary>
        internal List<Sprite> LineObjectPool { get; }

        /// <summary>
        ///     The current index we're in within the object pool.
        ///     Initialized to -1 because we add to it each time we add a judgement.
        /// </summary>
        private int CurrentLinePoolIndex { get; set; } = -1;

        /// <summary>
        ///     the last hit chevron.
        /// </summary>
        internal Sprite LastHitCheveron { get;  }

        /// <inheritdoc />
        /// <summary>
        ///   Ctor -  
        /// </summary>
        /// <param name="type"></param>
        /// <param name="size"></param>
        internal HitErrorBar(UDim2D size)
        {
            Size = size;

            MiddleLine = new Sprite()
            {
                Size = new UDim2D(2, 0, 0, 1),
                Alignment = Alignment.MidCenter,
                Parent = this
            };

            // Create the object pool and initialize all of the sprites.
            LineObjectPool = new List<Sprite>();          
            for (var i = 0; i < PoolSize; i++)
            {
                LineObjectPool.Add(new Sprite()
                {
                    Parent = this,
                    Size = new UDim2D(4, 0, 0, 1),
                    Alignment = Alignment.MidCenter,
                    Alpha = 0
                });
            }

            // Create the hit chevron.
            LastHitCheveron = new Sprite()
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
                LastHitCheveron.PosX = GraphicsHelper.Tween(LineObjectPool[CurrentLinePoolIndex].PosX, LastHitCheveron.PosX, Math.Min(dt / 360, 1));
            
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

            LineObjectPool[CurrentLinePoolIndex].Tint = GameBase.Skin.Keys[GameBase.SelectedMap.Mode].JudgeColors[j];
            LineObjectPool[CurrentLinePoolIndex].PosX = -(float) hitTime;
            LineObjectPool[CurrentLinePoolIndex].Alpha = 0.5f;
        }
    }
}