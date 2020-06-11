using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Online.API;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Dialogs.Online
{
    public class LegalAcceptanceDialog : YesNoDialog
    {
        private ScrollContainer Container { get; set;}
        
        public LegalAcceptanceDialog(string header, string name, APIRequest<string> request) : base(header, 
            $"By clicking \"Accept\", You acknowledge that you have read and agree to the\n{name}.")
        {
            Panel.Height = 800;
            
            CreateContainer();

            var text = request.ExecuteRequest();

            var legalText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), text, 20, true)
            {
                MaxWidth = Container.Width - 20,
                Alignment = Alignment.TopCenter
            };
            
            Container.AddContainedDrawable(legalText);
            Container.ContentContainer.Height = legalText.Height;

            YesButton.Image = UserInterface.AcceptButton;
            NoButton.Image = UserInterface.DeclineButton;
            
            YesButton.Y += 20;
            NoButton.Y += 20;
        }

        private void CreateContainer()
        {
            var size = new ScalableVector2(Panel.Width - 50, Panel.Height - Banner.Height - 110);
            
            Container = new ScrollContainer(size, size)
            {
                Parent = Panel,
                Alignment = Alignment.TopCenter,
                Y = Banner.Height + 20,
                Alpha = 0.50f,
                Tint = Color.Transparent,
                InputEnabled = true,
                Scrollbar = 
                { 
                    Tint = Color.White,
                    Width = 2,
                    X = 2
                },
            };
        }

        public override void Close()
        {
            Container.Visible = false;
            base.Close();
        }
    }
}