using System.ComponentModel.Composition;
using System.Windows.Input;
using Gemini.Framework.Commands;

namespace OngekiFumenEditorPlugins.KngkSupport.Commands.FastOpenFumen;

[CommandDefinition]
public class FastOpenFumenCommandDefinition : CommandDefinition
{
    public const string CommandName = "KangekiFumen.FastOpenFumen";

    [Export]
    public static CommandKeyboardShortcut KeyGesture =
        new CommandKeyboardShortcut<FastOpenFumenCommandDefinition>(new KeyGesture(Key.K, ModifierKeys.Control));

    public override string Name => CommandName;

    public override string Text => "快速打开Kangeki谱面文件";

    public override string ToolTip => Text;
}