using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Maps;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModPanelContainer : Container
    {
        /// <summary>
        /// </summary>
        public Bindable<bool> IsActive { get; } = new Bindable<bool>(false);

        private EditScreen Screen { get; }

        private EditorAutoModPanel Panel { get; }

        private ScalableVector2 PanelPosition { get; set; }

        private bool DialogsOpen { get; set; }

        public EditorAutoModPanelContainer(EditScreen screen)
        {
            Screen = screen;

            var mapset = new List<Qua> {Screen.WorkingMap};
            Screen.Map.Mapset.Maps.ForEach(x => mapset.Add(x.LoadQua()));

            Panel = new EditorAutoModPanel(Screen.WorkingMap, mapset)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            ChangePanelPosition();

            IsActive.ValueChanged += OnActiveChanged;
        }

        public override void Update(GameTime gameTime)
        {
            var isOnScreen = RectangleF.Intersects(ScreenRectangle, new RectangleF(0, 0, WindowManager.Width, WindowManager.Height));

            if (IsActive.Value && DialogManager.Dialogs.Count == 0 && isOnScreen)
                PanelPosition = Panel.Position;

            if (DialogsOpen != DialogManager.Dialogs.Count > 0)
                ChangePanelPosition();

            DialogsOpen = DialogManager.Dialogs.Count > 0;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!IsActive.Value)
                return;

            base.Draw(gameTime);
        }

        public override void Dispose()
        {
            IsActive.Dispose();
            base.Dispose();
        }

        private void OnActiveChanged(object sender, BindableValueChangedEventArgs<bool> e) => ChangePanelPosition();

        private void ChangePanelPosition()
        {
            if (!IsActive.Value || DialogManager.Dialogs.Count > 0)
                Panel.Position = new ScalableVector2(-10000, 0);
            else
                Panel.Position = PanelPosition;
        }
    }
}