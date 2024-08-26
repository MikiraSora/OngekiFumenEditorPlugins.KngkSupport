using System.Windows;
using Caliburn.Micro;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Microsoft.Win32;
using OngekiFumenEditor.Kernel.Audio;
using OngekiFumenEditor.Modules.FumenVisualEditor;
using OngekiFumenEditor.Modules.FumenVisualEditor.Models;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Properties;
using OngekiFumenEditor.Utils;

namespace OngekiFumenEditorPlugins.KngkSupport.Commands.FastOpenFumen;

[CommandHandler]
public class FastOpenFumenCommandHandler : CommandHandlerBase<FastOpenFumenCommandDefinition>
{
    public override async Task Run(Command command)
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Filter =
            FileDialogHelper.BuildExtensionFilter((".kngk", "Kangeki Fumen File"));
        openFileDialog.Title = "请选择在Kangeki游戏谱面文件夹里的.kngk谱面文件";
        openFileDialog.CheckFileExists = true;

        if (openFileDialog.ShowDialog() != true)
            return;
        var ogkrFilePath = openFileDialog.FileName;

        try
        {
            await TryOpenOgkrFileAsDocument(ogkrFilePath);
        }
        catch (Exception e)
        {
            var msg = $"无法打开kngk谱面:{e.Message}";
            Log.LogError(e.Message);
            MessageBox.Show(msg);
        }
    }

    public static async Task<EditorProjectDataModel> TryCreateEditorProjectDataModel(string kngkFilePath)
    {
        var (audioFile, audioDuration) = await GetAudioFilePath(kngkFilePath);

        if (!File.Exists(audioFile))
        {
            audioFile = FileDialogHelper.OpenFile(Resources.SelectAudioFileManually,
                IoC.Get<IAudioManager>().SupportAudioFileExtensionList);
            if (!File.Exists(audioFile))
                return null;
            audioDuration = await CalcAudioDuration(audioFile);
        }

        using var fs = File.OpenRead(kngkFilePath);
        var fumen = await IoC.Get<IFumenParserManager>().GetDeserializer(kngkFilePath).DeserializeAsync(fs);

        var newProj = new EditorProjectDataModel();
        newProj.FumenFilePath = kngkFilePath;
        newProj.Fumen = fumen;
        newProj.AudioFilePath = audioFile;
        newProj.AudioDuration = audioDuration;

        return newProj;
    }

    private static async Task<(string, TimeSpan)> GetAudioFilePath(string kngkFilePath)
    {
        var kngkFileDir = Path.GetDirectoryName(kngkFilePath);
        var audioFile =
            new[] {"*.wav", "*.ogg", "*.mp3"}.SelectMany(x =>
                Directory.GetFiles(kngkFileDir, x, SearchOption.TopDirectoryOnly)).FirstOrDefault();

        Log.LogDebug($"In kngk fumen folder {kngkFilePath}, pick audio file name: {Path.GetFileName(audioFile)}");
        return (audioFile, await CalcAudioDuration(audioFile));
    }

    public static async Task<string> TryFormatOpenFileName(string kngkFilePath)
    {
        var result = Path.GetFileName(kngkFilePath);

        return $"[{Resources.FastOpen}] " + result;
    }

    private static async Task<TimeSpan> CalcAudioDuration(string audioFilePath)
    {
        using var audio = await IoC.Get<IAudioManager>().LoadAudioAsync(audioFilePath);
        return audio.Duration;
    }

    private async Task<bool> TryOpenOgkrFileAsDocument(string kngkFilePath)
    {
        var newProj = await TryCreateEditorProjectDataModel(kngkFilePath);
        if (newProj is null)
            return false;

        var fumenProvider = IoC.Get<IFumenVisualEditorProvider>();
        var editor = IoC.Get<IFumenVisualEditorProvider>().Create();
        var viewAware = (IViewAware) editor;
        viewAware.ViewAttached += (sender, e) =>
        {
            var frameworkElement = (FrameworkElement) e.View;

            RoutedEventHandler loadedHandler = null;
            loadedHandler = async (sender2, e2) =>
            {
                frameworkElement.Loaded -= loadedHandler;
                await fumenProvider.Open(editor, newProj);
                var docName = await TryFormatOpenFileName(kngkFilePath);

                editor.DisplayName = docName;

                //not support yet.
                /*
                IoC.Get<IEditorRecentFilesManager>()
                    .PostRecord(new RecentRecordInfo(kngkFilePath, docName, RecentOpenType.CommandOpen));
                    */
            };
            frameworkElement.Loaded += loadedHandler;
        };

        await IoC.Get<IShell>().OpenDocumentAsync(editor);
        return true;
    }
}