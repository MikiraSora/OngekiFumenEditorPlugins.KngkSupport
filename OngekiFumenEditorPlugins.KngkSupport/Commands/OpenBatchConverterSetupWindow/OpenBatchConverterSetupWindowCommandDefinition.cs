using System.ComponentModel.Composition;
using System.Windows.Input;
using Gemini.Framework.Commands;

namespace OngekiFumenEditorPlugins.KngkSupport.Commands.FastOpenFumen;

[CommandDefinition]
public class OpenBatchConverterSetupWindowCommandDefinition : CommandDefinition
{
    public const string CommandName = "KangekiFumen.OpenBatchConverterSetupWindow";

    [Export]
    public static CommandKeyboardShortcut KeyGesture =
        new CommandKeyboardShortcut<OpenBatchConverterSetupWindowCommandDefinition>(new KeyGesture(Key.K, ModifierKeys.Control));

    public override string Name => CommandName;

    public override string Text => "批量转换Ongeki->Kangeki谱面";

    public override string ToolTip => Text;
}