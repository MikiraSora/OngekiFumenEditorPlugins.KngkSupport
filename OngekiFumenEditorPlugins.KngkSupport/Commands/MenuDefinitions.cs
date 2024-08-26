using System.ComponentModel.Composition;
using Gemini.Framework.Menus;
using OngekiFumenEditorPlugins.KngkSupport.Commands.FastOpenFumen;

namespace OngekiFumenEditorPlugins.KngkSupport.Commands;

public static class MenuDefinitions
{
    [Export]
    public static MenuItemDefinition FastOpenFumenMenuItem =
        new CommandMenuItemDefinition<FastOpenFumenCommandDefinition>(
            Gemini.Modules.MainMenu.MenuDefinitions.FileNewOpenMenuGroup, 9);
}