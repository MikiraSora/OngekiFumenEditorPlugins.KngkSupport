using OngekiFumenEditor.Base;
using OngekiFumenEditorPlugins.KngkSupport.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OngekiFumenEditorPlugins.KngkSupport.Utils
{
    public static class TGridExtensionMethod
    {
        public static KngkTGrid ToKngkTGrid(this TGrid ongkTGrid)
        {
            var totalOngkTUnit = ongkTGrid.TotalUnit;
            var kngkTGrid = new KngkTGrid((int)totalOngkTUnit, (int)(192 * (totalOngkTUnit - (int)totalOngkTUnit)));
            return kngkTGrid;
        }

        public static int ToKngkZ(this TGrid ongkTGrid)
        {
            return ongkTGrid.ToKngkTGrid().TotalGrid;
        }
    }
}
