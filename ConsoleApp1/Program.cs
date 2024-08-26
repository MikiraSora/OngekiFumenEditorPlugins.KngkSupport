using Caliburn.Micro;
using OngekiFumenEditor;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Utils;
using OngekiFumenEditorPlugins.KngkSupport.Bases;

var app = new AppBootstrapper(false);
PlatformProvider.Current = new DefaultPlatformProvider();
Log.LogInfo("HELLO TEST CONSOLE.");

var parserManager = IoC.Get<IFumenParserManager>();
var filePath = @"F:\KANGEKI_4.00\KANGEKI_Data\UserContent\Music\ホシシズク\hoshi.kngk";
var fumen = await parserManager.GetDeserializer(filePath).DeserializeAsync(
    File.OpenRead(filePath));

var fromTGrid = new KngkTGrid(0, 0);
var toTGrid = new KngkTGrid(3, (int) (192 / 4 * 0.28));

var toBTGrid = fromTGrid + new GridOffset(0, 57791);

var time = TimeSpan.FromMilliseconds(MathUtils.CalculateBPMLength(fromTGrid.TotalUnit, toBTGrid.TotalUnit, 182,
    fromTGrid.GridRadix));

Console.ReadLine();