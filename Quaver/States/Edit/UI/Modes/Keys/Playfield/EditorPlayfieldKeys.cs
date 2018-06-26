using Quaver.API.Enums;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Modes.Keys.Playfield
{
    internal class EditorPlayfieldKeys : IEditorPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Container Container { get; }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public GameMode Mode { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="mode"></param>
        internal EditorPlayfieldKeys(EditorScreen screen, GameMode mode)
        {
            Screen = screen;
            Mode = mode;
            
            Container = new Container();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            var box = new Sprite()
            {
                Parent = Container,
                Size = new UDim2D(300, 300),
                Alignment = Alignment.MidCenter,
                Image = FontAwesome.Archive
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            Container.Update(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Container.Draw();
            GameBase.SpriteBatch.End();
        }
    }
}