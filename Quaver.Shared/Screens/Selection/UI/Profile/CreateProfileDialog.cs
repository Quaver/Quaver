using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class CreateProfileDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        private Bindable<UserProfile> Profile { get; }

        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CreateProfileDialog(Bindable<UserProfile> profile) : base("CREATE PROFILE",
            "Enter a name for your new local profile...")
        {
            Profile = profile;

            YesButton.Visible = false;
            YesButton.IsClickable = false;
            NoButton.Visible = false;
            NoButton.IsClickable = false;

            CreateTextbox();
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, "", "Give your new profile a name", s =>
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        Profile.TriggerChange();
                        return;
                    }

                    if (UserProfileDatabaseCache.Profiles.Value.Any(x => x.Username == s) ||
                        s == "Local" || s == "Online" || s == "Create New")
                    {
                        NotificationManager.Show(NotificationLevel.Error, "This profile name is already taken. Please choose another!");
                        Profile.TriggerChange();
                        return;
                    }

                    var profile = new UserProfile { Username = s };

                    var added = UserProfileDatabaseCache.CreateProfile(profile);

                    if (!added)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "There was an error while creating your profile.");
                        return;
                    }

                    Profile.Value = profile;
                })
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -44,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                MaxCharacters = 15
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            Profile.TriggerChange();

            base.Close();
        }
    }
}