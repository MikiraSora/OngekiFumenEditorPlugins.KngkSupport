using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Caliburn.Micro;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gemini.Framework;
using MahApps.Metro.Controls;
using OngekiFumenEditor;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Kernel.Audio;
using OngekiFumenEditor.Modules.AudioAdjustWindow;
using OngekiFumenEditor.Modules.FumenVisualEditor;
using OngekiFumenEditor.Modules.OgkiFumenListBrowser;
using OngekiFumenEditor.Modules.OgkiFumenListBrowser.Models;
using OngekiFumenEditor.Modules.OptionGeneratorTools.Kernel;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Properties;
using OngekiFumenEditor.Utils;
using OngekiFumenEditorPlugins.KngkSupport.Bases;
using OngekiFumenEditorPlugins.KngkSupport.Parsers.Kngk;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OngekiFumenEditorPlugins.KngkSupport.ViewModels;

[Export(typeof(IBatchConverterSetup))]
public partial class BatchConverterSetupWindowViewModel : WindowBase, IBatchConverterSetup
{
    private bool isBusy;

    private string outputFolderPath = string.Empty;

    private string rootFolderPath = string.Empty;
    private bool isNoCache;
    private int parallelCount;
    private readonly IFumenParserManager fumenParserManager;
    private readonly DefaultKngkFumenFormatter kngkFumenFormatter;
    private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ObservableCollection<ConvertFumenTarget> FumenTargets { get; } = new();

    [ImportingConstructor]
    public BatchConverterSetupWindowViewModel(IFumenParserManager fumenParserManager, DefaultKngkFumenFormatter kngkFumenFormatter)
    {
        DisplayName = Resources.OgkiFumenListBrowser;
        rootFolderPath = OngekiFumenEditor.Properties.OptionGeneratorToolsSetting.Default.LastLoadedGameFolder;
        isNoCache = OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.LastIsNoCache;
        outputFolderPath = OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.LastConvertOutputFolder;
        parallelCount = OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.ParallelCount;
        this.fumenParserManager = fumenParserManager;
        this.kngkFumenFormatter = kngkFumenFormatter;
        
    }

    public bool IsBusy
    {
        get => isBusy;
        set => Set(ref isBusy, value);
    }

    public bool IsNoCache
    {
        get => isNoCache;
        set
        {
            Set(ref isNoCache, value);
            OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.LastIsNoCache = IsNoCache;
            OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.Save();
        }
    }

    public int ParallelCount
    {
        get => parallelCount;
        set
        {
            Set(ref parallelCount, value);
            OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.ParallelCount = ParallelCount;
            OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.Save();
        }
    }

    public string RootFolderPath
    {
        get => rootFolderPath;
        set
        {
            Set(ref rootFolderPath, value);
            RefreshList();
        }
    }

    public string OutputFolderPath
    {
        get => outputFolderPath;
        set
        {
            Set(ref outputFolderPath, value);
        }
    }

    private async void RefreshList()
    {
        IsBusy = true;
        FumenTargets.Clear();
        var resourceMap = new Dictionary<string, string>();

        var fumenSets = await IoC.Get<IOgkiFumenListBrowser>().SearchFumenSet(RootFolderPath);

        FumenTargets.AddRange(fumenSets.Select(x => new ConvertFumenTarget
        {
            FumenSet = x
        }));
        IsBusy = false;
    }

    protected override void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);
        if (Directory.Exists(RootFolderPath))
            RefreshList();
    }

    public void SelectFolder()
    {
        if (!FileDialogHelper.OpenDirectory(Resources.SelectGameRootFolder, out var folderPath))
            return;

        RootFolderPath = folderPath;
        OngekiFumenEditor.Properties.OptionGeneratorToolsSetting.Default.LastLoadedGameFolder = RootFolderPath;
        OngekiFumenEditor.Properties.OptionGeneratorToolsSetting.Default.Save();
    }

    public void SelectOutputFolder()
    {
        if (!FileDialogHelper.OpenDirectory("选择输出目录文件夹", out var folderPath))
            return;

        OutputFolderPath = folderPath;
        OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.LastConvertOutputFolder = OutputFolderPath;
        OngekiFumenEditorPlugins.KngkSupport.Properties.Settings.Default.Save();
    }

    public void OnSelectReverseClicked()
    {
        IsBusy = true;
        FumenTargets.ForEach(x => x.IsSelect = !x.IsSelect);
        IsBusy = false;
    }


    public void OnSelectAllOrNoneClicked()
    {
        IsBusy = true;
        var needTrue = true;
        FumenTargets.ForEach(x =>
        {
            if (x.IsSelect)
                needTrue = false;
            x.IsSelect = needTrue;
        });
        if (!needTrue)
            FumenTargets.TakeWhile(x => x.IsSelect).ForEach(x => x.IsSelect = false);
        IsBusy = false;
    }

    public async void OnConvertClicked()
    {
        if (string.IsNullOrWhiteSpace(RootFolderPath))
        {
            MessageBox.Show("请先选择谱面搜索目录");
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputFolderPath))
        {
            MessageBox.Show("请先选择输出目录");
            return;
        }

        if (!FumenTargets.Any(x => x.IsSelect))
        {
            MessageBox.Show("没有谱面被选择");
            return;
        }

        IsBusy = true;

        var reporter = new ConvertProgressReporter();
        var reportTask = Task.Run(() => App.Current.Invoke(() => IoC.Get<IWindowManager>().ShowDialogAsync(new ConverterProgressReporterWindowViewModel(reporter))));
        var convertTask = Convert(FumenTargets.Where(x => x.IsSelect).Select(x => x.FumenSet), OutputFolderPath, new ConvertOptions()
        {
            NoCache = IsNoCache
        }, reporter);
        await convertTask;

        if (reporter.Tasks.All(x => x.Status == ConvertProgressReporter.TaskStatus.Success))
        {
            if (MessageBox.Show("所有谱面转换成功！是否查看输出目录?", "转换结果", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                ProcessUtils.OpenPath(OutputFolderPath);
        }
        else if (reporter.Tasks.Any(x => x.Status == ConvertProgressReporter.TaskStatus.Success))
        {
            if (MessageBox.Show("部分谱面转换成功！是否查看输出目录?", "转换结果", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                ProcessUtils.OpenPath(OutputFolderPath);
        }
        else
        {
            MessageBox.Show("所有谱面无法转换，请查看输出内容或者日志.");
        }

        await reportTask;
        IsBusy = false;
    }

    public async Task Convert(IEnumerable<OngekiFumenSet> fumenSets, string outputFolder,
        ConvertOptions options = null, ConvertProgressReporter reporter = null)
    {
        var taskMap = fumenSets.ToDictionary(x => x, x => new ConvertProgressReporter.ConvertTask
        {
            Status = ConvertProgressReporter.TaskStatus.WaitInQueue,
            Description = "等待转换...",
            Set = x
        });

        if (reporter != null)
        {
            reporter.Tasks.AddRange(taskMap.Values);
            reporter.IsRunning = true;
        }

        options = options ?? new();

        await Parallel.ForEachAsync(fumenSets, new ParallelOptions()
        {
            MaxDegreeOfParallelism = ParallelCount
        }, (set, ct) =>
        {
            var fumenFolder = Path.Combine(outputFolder, $"ogkConvter_{set.MusicId}");
            var ctx = new ConvertCtx
            {
                Set = set,
                Options = options,
                FumenFolder = fumenFolder
            };
            return DoConvertInternal(ctx, taskMap[set]);
        });

        if (reporter != null)
            reporter.IsRunning = false;
    }

    private async ValueTask DoConvertInternal(ConvertCtx ctx, ConvertProgressReporter.ConvertTask task)
    {
        Directory.CreateDirectory(ctx.FumenFolder);

        var failCounter = 0;
        var lastErrorDescription = string.Empty;

        if (!await ProcessFumenFile(ctx, task))
        {
            failCounter++;
            lastErrorDescription = task.Description;
        }
        if (!await ProcessJacket(ctx, task))
        {
            failCounter++;
            lastErrorDescription = task.Description;
        }
        if (!await ProcessAudio(ctx, task))
        {
            failCounter++;
            lastErrorDescription = task.Description;
        }

        task.Status = failCounter switch
        {
            0 => ConvertProgressReporter.TaskStatus.Success,
            3 => ConvertProgressReporter.TaskStatus.Fail,
            _ => ConvertProgressReporter.TaskStatus.Problem
        };

        task.Description = failCounter switch
        {
            0 => "转换成功",
            3 => "转换失败",
            _ => $"部分转换有问题: {lastErrorDescription}"
        };
    }

    private async ValueTask<bool> ProcessFumenFile(ConvertCtx ctx, ConvertProgressReporter.ConvertTask task)
    {
        void reportLog(string msg)
        {
            task.Description = $"[1/3] {msg}";
        }

        try
        {
            foreach (var diff in ctx.Set.Difficults)
            {
                var fumenFileName = $"{ctx.Set.MusicId}_{diff.DiffName}_{diff.Level}_{diff.DiffIdx}.kngk";
                var outputFumenFilePath = Path.Combine(ctx.FumenFolder, fumenFileName);
                if (File.Exists(outputFumenFilePath) && !ctx.Options.NoCache)
                {
                    reportLog($"[{diff.DiffIdx}] 谱面 {fumenFileName} 已存在，跳过");
                    continue;
                }

                reportLog($"[{diff.DiffIdx}] 转换谱面 {fumenFileName} 中....");
                Log.LogDebug($"[{diff.DiffIdx}] Convert {diff.FilePath} -> {outputFumenFilePath}");

                var ongkFumen = await fumenParserManager.Deserialize(diff.FilePath);
                var kngkFumen = await kngkFumenFormatter.ConvertToKngkFumen(ongkFumen);

                //add more infomation
                kngkFumen.Artist = ctx.Set.Artist;
                kngkFumen.Title = ctx.Set.Title;
                kngkFumen.Lv = diff.Level.ToString();
                kngkFumen.Difficulty = diff.DiffIdx;
                kngkFumen.Genre = ctx.Set.Genre;
                kngkFumen.BossName = ctx.Set.GetString("BossCard");
                kngkFumen.BossLv = ctx.Set.GetPathValue<int>("BossLevel");
                kngkFumen.AudioOffset = 0;
                kngkFumen.BossAttr = ctx.Set.GetPathValue<string>("WaveAttribute", "AttributeType")?.ToLowerInvariant() switch
                {
                    "fire" => 0,
                    "aqua" => 1,
                    "leaf" => 2,
                    _ => 3 //STAR
                };
                kngkFumen.ScoreTitle = string.Empty;


                var msec = MathUtils.CalculateBPMLength(TGrid.Zero, new TGrid(1, 0), ongkFumen.BpmList.FirstBpm.BPM);
                ctx.AudioOffset = TimeSpan.FromMilliseconds(msec);

                using var fs = File.OpenWrite(outputFumenFilePath);
                await JsonSerializer.SerializeAsync(fs, kngkFumen, serializerOptions);
                reportLog($"[{diff.DiffIdx}] 转换谱面 {fumenFileName} 成功!");
            }

            return true;
        }
        catch (Exception e)
        {
            task.Status = ConvertProgressReporter.TaskStatus.Fail;
            reportLog($"谱面转换出错:{e.Message}");
            return false;
        }
    }

    private async ValueTask<bool> ProcessAudio(ConvertCtx ctx, ConvertProgressReporter.ConvertTask task)
    {
        void reportLog(string msg)
        {
            task.Description = $"[3/3] {msg}";
        }

        try
        {
            var outputAudioFilePath = Path.Combine(ctx.FumenFolder, "audio.wav");
            if (File.Exists(outputAudioFilePath) && !ctx.Options.NoCache)
            {
                reportLog("音频已存在，跳过");
                return true;
            }

            reportLog("转换音频中....");

            var acbFilePath = ctx.Set.AudioFilePath;
            Log.LogDebug($"Convert {acbFilePath} -> {outputAudioFilePath}");
            var wavFilePath = await AcbConverter.ConvertAcbFileToWavFile(acbFilePath);

            var (isSuccess, msg) = await IoC.Get<IAudioAdjustWindow>().OffsetAudioFile(wavFilePath, outputAudioFilePath, -ctx.AudioOffset);

            //File.Copy(wavFilePath, outputAudioFilePath, true);

            reportLog("转换音频成功!");
            return true;
        }
        catch (Exception e)
        {
            task.Status = ConvertProgressReporter.TaskStatus.Fail;
            reportLog($"音频转换出错:{e.Message}");
            return false;
        }
    }

    private async ValueTask<bool> ProcessJacket(ConvertCtx ctx, ConvertProgressReporter.ConvertTask task)
    {
        void reportLog(string msg)
        {
            task.Description = $"[2/3] {msg}";
        }

        try
        {
            var outputJacketFilePath = Path.Combine(ctx.FumenFolder, "jacket.png");
            if (File.Exists(outputJacketFilePath) && !ctx.Options.NoCache)
            {
                reportLog("封面已存在，跳过");
                return true;
            }

            reportLog("转换封面中....");
            var inputJacketFilePath = ctx.Set.JacketFilePath;
            Log.LogDebug($"Convert {inputJacketFilePath} -> {outputJacketFilePath}");

            var imgData = await JacketGenerateWrapper.GetMainImageDataAsync(default, inputJacketFilePath);
            using var image = Image.LoadPixelData<Rgba32>(imgData.Data, imgData.Width, imgData.Height);
            var memoryStream = new MemoryStream();
            image.Mutate(i => i.Flip(FlipMode.Vertical));
            image.SaveAsPng(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var fs = File.OpenWrite(outputJacketFilePath);
            await memoryStream.CopyToAsync(fs);

            reportLog("转换封面成功!");
            return true;
        }
        catch (Exception e)
        {
            task.Status = ConvertProgressReporter.TaskStatus.Fail;
            reportLog($"封面转换出错:{e.Message}");
            return false;
        }
    }

    private class ConvertCtx
    {
        public OngekiFumenSet Set { get; set; }
        public ConvertOptions Options { get; set; }
        public string FumenFolder { get; set; }
        public TimeSpan AudioOffset { get; internal set; }
    }

    public partial class ConvertFumenTarget : ObservableObject
    {
        [ObservableProperty]
        private OngekiFumenSet fumenSet;

        [ObservableProperty]
        private bool isSelect;

        [RelayCommand]
        private void ToggleSelectMode()
        {
            IsSelect = !IsSelect;
        }
    }

    public partial class ConvertOptions : ObservableObject
    {
        [ObservableProperty]
        private bool noCache;
    }

    public partial class ConvertProgressReporter : ObservableObject
    {
        public enum TaskStatus
        {
            WaitInQueue,
            Converting,
            Success,
            Problem,
            Fail
        }

        [ObservableProperty]
        private bool isRunning;

        [ObservableProperty]
        private ObservableCollection<ConvertTask> tasks = new();

        public partial class ConvertTask : ObservableObject
        {
            [ObservableProperty]
            private string description;

            [ObservableProperty]
            private TaskStatus status;

            [ObservableProperty]
            private OngekiFumenSet set;
        }
    }
}