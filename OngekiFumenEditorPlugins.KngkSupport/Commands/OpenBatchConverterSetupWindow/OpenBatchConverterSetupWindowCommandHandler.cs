using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Framework.Commands;

namespace OngekiFumenEditorPlugins.KngkSupport.Commands.FastOpenFumen;

[CommandHandler]
public class
    OpenBatchConverterSetupWindowCommandHandler : CommandHandlerBase<OpenBatchConverterSetupWindowCommandDefinition>
{
    private readonly IWindowManager _windowManager;

    [ImportingConstructor]
    public OpenBatchConverterSetupWindowCommandHandler(IWindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public override async Task Run(Command command)
    {
        await _windowManager.ShowWindowAsync(IoC.Get<IBatchConverterSetup>());
    }
}