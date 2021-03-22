using System;
using Quaver.API.Maps.AutoMod.Issues;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModIssueClicked : EventArgs
    {
        public AutoModIssue Issue { get; }

        public EditorAutoModIssueClicked(AutoModIssue issue) => Issue = issue;
    }
}