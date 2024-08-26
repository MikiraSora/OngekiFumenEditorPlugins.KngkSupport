using System.Text.Json;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.EditorObjects;
using OngekiFumenEditor.Base.EditorObjects.LaneCurve;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Base.OngekiObjects.Beam;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Base.OngekiObjects.Lane;
using OngekiFumenEditor.Base.OngekiObjects.Wall;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Parser.DefaultImpl.Ogkr;
using OngekiFumenEditor.Utils;
using OngekiFumenEditorPlugins.KngkSupport.Bases;

namespace OngekiFumenEditorPlugins.KngkSupport.Parsers.Kngk;

//[Export(typeof(IFumenDeserializable))]
//[Export(typeof(DefaultKngkFumenParser))]
public class DefaultKngkFumenParser : IFumenDeserializable
{
    public const string FormatName = "Kangeki Fumen File";

    public static readonly string[] FumenFileExtensions = {".kngk"};
    public Dictionary<string, ICommandParser> CommandParsers { get; } = new();

    public string[] SupportFumenFileExtensions => FumenFileExtensions;

    public async Task<OngekiFumen> DeserializeAsync(Stream stream)
    {
        //todo: .kngk -> KngkFumen -> OngekiFumen

        var kangekiFumen = await DeserializeAsKangekiFumenAsync(stream);
        var ongekiFumen = new OngekiFumen();

        ConvertInfo(ongekiFumen, kangekiFumen);
        ConvertLanes(ongekiFumen, kangekiFumen);
        ConvertBullets(ongekiFumen, kangekiFumen);
        ConvertLasers(ongekiFumen, kangekiFumen);
        ConvertBells(ongekiFumen, kangekiFumen);
        ConvertNotes(ongekiFumen, kangekiFumen);
        ConvertFlicks(ongekiFumen, kangekiFumen);
        ConvertCompositions(ongekiFumen, kangekiFumen);

        return ongekiFumen;
    }

    public string FileFormatName => FormatName;

    public async Task<KngkFumen> DeserializeAsKangekiFumenAsync(Stream stream)
    {
        var kangekiFumen = await JsonSerializer.DeserializeAsync<KngkFumen>(stream);
        if (kangekiFumen is null)
        {
            Log.LogError("Can't parse kangeki fumen file.");
            return default;
        }

        return kangekiFumen;
    }

    private IEnumerable<ConnectableStartObject> ConvertKngkLaneToConnectables(IEnumerable<Point> points,
        LaneType genLaneType)
    {
        if (points is null)
            yield break;

        bool checkAndSetupObj(OngekiMovableObjectBase obj, Point point)
        {
            obj.XGrid.Unit = point.X / 4;
            obj.XGrid.NormalizeSelf();

            obj.TGrid = new KngkTGrid(0, point.Z).AsOngkTGrid();
            obj.TGrid.NormalizeSelf();

            //check if point is over limitation bar300.
            return point.Z < 57791;
        }

        var currentStart = default(ConnectableStartObject);
        LaneCurvePathControlObject prepareCurveControl = default;

        foreach (var point in points)
            if (point.Line)
            {
                if (currentStart is null)
                {
                    //as start
                    ConnectableStartObject start = genLaneType switch
                    {
                        LaneType.Beam => new BeamStart(),
                        LaneType.Left => new LaneLeftStart(),
                        LaneType.Center => new LaneCenterStart(),
                        LaneType.Right => new LaneRightStart(),
                        LaneType.Colorful => new ColorfulLaneStart(),
                        LaneType.Enemy => new EnemyLaneStart(),
                        LaneType.WallLeft => new WallLeftStart(),
                        LaneType.WallRight => new WallRightStart(),
                        LaneType.AutoPlayFader => new AutoplayFaderLaneStart(),
                        _ => throw new ArgumentOutOfRangeException(nameof(genLaneType), genLaneType, null)
                    };
                    if (checkAndSetupObj(start, point))
                    {
                        currentStart = start;
                        Log.LogDebug($"start: {point} -> {start}");
                    }
                }
                else
                {
                    if (point.Curve > 1)
                    {
                        //as curve control
                        var curveControl = new LaneCurvePathControlObject();
                        if (checkAndSetupObj(curveControl, point))
                        {
                            Log.LogDebug($"curve gen: {point} -> {curveControl}");
                            prepareCurveControl = curveControl;
                        }
                    }
                    else
                    {
                        //as next
                        var next = currentStart.CreateChildObject();
                        checkAndSetupObj(next, point);
                        if (prepareCurveControl is not null)
                        {
                            Log.LogDebug($"curve apply: {prepareCurveControl} -> {next}");
                            next.AddControlObject(prepareCurveControl);
                            prepareCurveControl = default;
                        }

                        currentStart.AddChildObject(next);
                        Log.LogDebug($"next: {point} -> {next}");
                    }
                }
            }
            else
            {
                if (currentStart is not null)
                {
                    //as end
                    var end = currentStart.CreateChildObject();
                    if (checkAndSetupObj(end, point))
                    {
                        if (prepareCurveControl is not null)
                        {
                            Log.LogDebug($"curve apply: {prepareCurveControl} -> {end}");
                            end.AddControlObject(prepareCurveControl);
                            prepareCurveControl = default;
                        }

                        currentStart.AddChildObject(end);
                        Log.LogDebug($"end: {point} -> {end}");
                    }

                    //done.
                    yield return currentStart;
                    currentStart = null;
                }
            }

        if (currentStart is not null)
            yield return currentStart;
    }

    private void ConvertCompositions(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        Log.LogDebug("done.");
    }

    private void ConvertFlicks(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        Log.LogDebug("done.");
    }

    private void ConvertNotes(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        Log.LogDebug("done.");
    }

    private void ConvertInfo(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");

        ongekiFumen.MetaInfo.Creator = kangekiFumen.Designer;
        var bpm = float.Parse(kangekiFumen.Bpm);
        ongekiFumen.MetaInfo.BpmDefinition.First = bpm;
        ongekiFumen.MetaInfo.BpmDefinition.Common = bpm;
        ongekiFumen.MetaInfo.BpmDefinition.Minimum = bpm;
        ongekiFumen.MetaInfo.BpmDefinition.Maximum = bpm;

        Log.LogDebug("done.");
    }

    private void ConvertBells(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        Log.LogDebug("done.");
    }

    private void ConvertLasers(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        Log.LogDebug("done.");
    }

    private void ConvertBullets(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");


        Log.LogDebug("done.");
    }

    private void ConvertLanes(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        var list = new List<ConnectableStartObject>();

        void AddLanes(IEnumerable<Point> points, LaneType genLaneType)
        {
            var lanes = ConvertKngkLaneToConnectables(points, genLaneType).ToArray();
            Log.LogDebug(
                $"Try convert kngk lane to ongk lane {genLaneType}, points.count: {points?.Count()}, generated {lanes.Length} ogkr {genLaneType} lanes");
            list.AddRange(lanes);
        }

        AddLanes(kangekiFumen.Left, LaneType.WallLeft);
        AddLanes(kangekiFumen.Right, LaneType.WallRight);

        for (var i = 0; i < 8; i++)
        {
            var bias = 3 * i;
            Log.LogDebug($"Try convert kngk lanes, bias: {bias}");
            AddLanes(kangekiFumen.Lanes.ElementAtOrDefault(bias + 0)?.Points, LaneType.Left);
            AddLanes(kangekiFumen.Lanes.ElementAtOrDefault(bias + 1)?.Points, LaneType.Center);
            AddLanes(kangekiFumen.Lanes.ElementAtOrDefault(bias + 2)?.Points, LaneType.Right);
        }

        AddLanes(kangekiFumen.Lanes.ElementAtOrDefault(24)?.Points, LaneType.AutoPlayFader);
        AddLanes(kangekiFumen.Lanes.ElementAtOrDefault(25)?.Points, LaneType.Enemy);

        ongekiFumen.AddObjects(list);
        Log.LogDebug("done.");
    }
}