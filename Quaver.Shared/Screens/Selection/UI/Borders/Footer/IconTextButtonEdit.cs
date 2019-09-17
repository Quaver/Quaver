using System;
 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Graphics;
 using Quaver.Shared.Assets;
 using Quaver.Shared.Graphics.Menu.Border.Components;
 using Quaver.Shared.Graphics.Notifications;
 using Quaver.Shared.Screens.Selection.UI.Mapsets;
 using Wobble.Graphics.Sprites.Text;
 using Wobble.Managers;

 namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
 {
     public class IconTextButtonEdit : IconTextButton
     {
         public IconTextButtonEdit(SelectionScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_pencil),
             FontManager.GetWobbleFont(Fonts.LatoBlack),"Edit", (sender, args) =>
             {
                 screen.ExitToEditor();
             })
         {
         }
     }
 }