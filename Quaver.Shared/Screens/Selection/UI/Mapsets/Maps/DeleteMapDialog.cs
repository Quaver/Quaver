using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps
{
    public class DeleteMapDialog : YesNoDialog
    {
        public DeleteMapDialog(Map map, int index)
            : base("Delete Map".ToUpper(), "Are you sure you would like to delete this map?",
                () => MapManager.Delete(map, index))
        {
        }
    }
}