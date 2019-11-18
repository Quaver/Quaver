using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Tests.CheckboxContainers
{
    public class TestCheckboxChatChannel : ChatChannel, ICheckboxContainerItem
    {
        public bool IsSelected { get; set; } = false;

        public string GetName() => Name;

        public bool GetSelectedState() => false;

        public void OnToggle()
        {
            Logger.Important($"TestCheckboxChatChannel has been toggled: {IsSelected}", LogType.Runtime);
        }
    }
}