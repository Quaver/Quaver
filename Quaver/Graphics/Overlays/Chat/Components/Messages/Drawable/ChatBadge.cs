using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Server.Common.Enums;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Graphics.Overlays.Chat.Components.Messages.Drawable
{
    public class ChatBadge : Sprite
    {
        /// <summary>
        ///     The user groups for this chat badge.
        /// </summary>
        private UserGroups UserGroups { get; }

        /// <summary>
        ///     The user's icon for their badge.
        /// </summary>
        private Sprite Icon { get; }

        /// <summary>
        ///     The spritetext that displays the usergroup's text.
        /// </summary>
        private SpriteTextBitmap TextUserGroup { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="userGroups"></param>
        public ChatBadge(UserGroups userGroups)
        {
            UserGroups = userGroups;

            var userGroupColor = Colors.GetUserChatColor(UserGroups);

            Tint = new Color((int) (userGroupColor.R * 0.75f), (int)(userGroupColor.G * 0.75f), (int)(userGroupColor.B * 0.75f));

            Icon = new Sprite()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft,
                Image = GetIcon(UserGroups),
                X = 10
            };

            TextUserGroup = new SpriteTextBitmap(BitmapFonts.Exo2BoldItalic, GetUserGroupName(UserGroups), 24,
                Color.White, Alignment.TopLeft, (int) WindowManager.Width)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft
            };

            TextUserGroup.Width *= 0.45f;
            TextUserGroup.Height *= 0.45f;

            Icon.Size = new ScalableVector2(TextUserGroup.Height * 0.75f, TextUserGroup.Height * 0.75f);

            TextUserGroup.X = Icon.X + Icon.Width + 5;

            Size = new ScalableVector2(Icon.X + Icon.Width + TextUserGroup.Width + 10 + 5, TextUserGroup.Height + 1);
        }

        /// <summary>
        ///     Gets a chat badge icon based on user groups.
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static Texture2D GetIcon(UserGroups groups)
        {
            if (groups.HasFlag(UserGroups.Developer))
                return FontAwesome.Code;

            // Bot
            if (groups.HasFlag(UserGroups.Bot))
                return FontAwesome.Wrench;

            // Admin
            if (groups.HasFlag(UserGroups.Admin))
                return FontAwesome.Gavel;

            return null;
        }

        /// <summary>
        ///     Returns the user group's name stringified.
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static string GetUserGroupName(UserGroups groups)
        {
            if (groups.HasFlag(UserGroups.Developer))
                return "Developer";

            // Bot
            if (groups.HasFlag(UserGroups.Bot))
                return "Bot";

            // Admin
            if (groups.HasFlag(UserGroups.Admin))
                return "Admin";

            return null;
        }
    }
}