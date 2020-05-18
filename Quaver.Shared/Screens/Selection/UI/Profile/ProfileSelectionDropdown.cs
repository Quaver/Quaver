using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class ProfileSelectionDropdown : LabelledDropdown
    {
        /// <summary>
        /// </summary>
        private Bindable<UserProfile> Profile { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="profile"></param>
        public ProfileSelectionDropdown(Bindable<UserProfile> profile) : base("SELECT: ", 24, new Dropdown(GetDropdownItems(),
            new ScalableVector2(185, 30), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex()))
        {
            Profile = profile;

            Dropdown.ItemSelected += (sender, args) =>
            {
                switch (args.Index)
                {
                    // Local Profile
                    case 0:
                        Profile.Value = new UserProfile();
                        break;
                    // Online Profile
                    case 1:
                        Profile.Value = new UserProfile { IsOnline = true };
                        break;
                    default:
                        // Create new
                        if (args.Index == Dropdown.Items.Count - 1)
                        {
                            SelectCurrentItem();
                            DialogManager.Show(new CreateProfileDialog(Profile));
                            return;
                        }

                        Profile.Value = UserProfileDatabaseCache.Profiles?.Value[args.Index - 2];
                        break;
                }
            };

            Dropdown.Items.Last().Text.Tint = Color.LimeGreen;
            SelectCurrentItem();

            Profile.ValueChanged += OnProfileChanged;
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Profile.ValueChanged -= OnProfileChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems()
        {
            var options = new List<string>()
            {
                "Local",
                "Online",
            };

            if (UserProfileDatabaseCache.Profiles?.Value != null)
            {
                foreach (var profile in UserProfileDatabaseCache.Profiles.Value)
                    options.Add(profile.Username);
            }

            options.Add("Create New");

            return options;
        }

        /// <summary>
        /// </summary>
        private void SelectCurrentItem()
        {
            if (Profile.Value.Id == 0)
                Dropdown.SelectItem(!Profile.Value.IsOnline ? Dropdown.Items.First() : Dropdown.Items[1]);
            else
                Dropdown.SelectItem(Dropdown.Items[Dropdown.Options.FindIndex(x => x == Profile.Value.Username)]);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex() => 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProfileChanged(object sender, BindableValueChangedEventArgs<UserProfile> e)
        {
            if (e.Value.Id == 0)
                Dropdown.SelectItem(!e.Value.IsOnline ? Dropdown.Items.First() : Dropdown.Items[1]);
        }
    }
}