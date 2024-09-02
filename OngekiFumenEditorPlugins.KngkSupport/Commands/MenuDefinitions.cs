using System.ComponentModel.Composition;
using Gemini.Framework.Menus;
using OngekiFumenEditorPlugins.KngkSupport.Commands.FastOpenFumen;

namespace OngekiFumenEditorPlugins.KngkSupport.Commands;

public static class MenuDefinitions
{
    [Export]
    public static MenuDefinition KangekiFumenMenu = new MenuDefinition(Gemini.Modules.MainMenu.MenuDefinitions.MainMenuBar, 7, "Kangeki");

    [Export]
    public static MenuItemGroupDefinition KangekiFumenMenuGroup = new MenuItemGroupDefinition(KangekiFumenMenu, 0);

    [Export]
    public static MenuItemDefinition OpenBatchConverterSetupWindowMenuItem = new CommandMenuItemDefinition<OpenBatchConverterSetupWindowCommandDefinition>(KangekiFumenMenuGroup, 0);
}