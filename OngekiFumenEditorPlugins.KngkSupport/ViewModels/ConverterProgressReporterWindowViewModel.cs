using Gemini.Framework;
using static OngekiFumenEditorPlugins.KngkSupport.ViewModels.BatchConverterSetupWindowViewModel;

namespace OngekiFumenEditorPlugins.KngkSupport.ViewModels
{
    public class ConverterProgressReporterWindowViewModel : WindowBase
    {
        private ConvertProgressReporter reporter;

        public ConverterProgressReporterWindowViewModel(ConvertProgressReporter reporter)
        {
            Reporter = reporter;
        }

        public ConvertProgressReporter Reporter
        {
            get => reporter;
            set => Set(ref reporter, value);
        }
    }
}
