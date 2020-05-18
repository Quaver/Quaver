using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DeleteMapsetDialog : YesNoDialog
    {
        public DeleteMapsetDialog(Mapset mapset, int index)
            : base("Delete Mapset".ToUpper(), "Are you sure you would like to delete this mapset?", () =>
            {
                MapManager.Delete(mapset, index);
            })
        {
        }
    }
}