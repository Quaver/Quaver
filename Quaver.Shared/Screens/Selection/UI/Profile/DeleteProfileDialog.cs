using System;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class DeleteProfileDialog : YesNoDialog
    {
        public DeleteProfileDialog(Bindable<UserProfile> profile) : base("DELETE PROFILE",
            "Are you sure you would like to delete this profile?", () =>
            {
                UserProfileDatabaseCache.DeleteProfile(profile.Value);
                profile.Value = new UserProfile();
            })
        {
        }
    }
}