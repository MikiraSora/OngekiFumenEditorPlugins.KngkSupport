using OngekiFumenEditor.Base;

namespace OngekiFumenEditorPlugins.KngkSupport.Utils;

public static class XGridExtensionMethod
{
    public static int ToKngkX(this XGrid ongkXGrid)
    {
        return (int) (ongkXGrid.TotalUnit * 4);
    }
}