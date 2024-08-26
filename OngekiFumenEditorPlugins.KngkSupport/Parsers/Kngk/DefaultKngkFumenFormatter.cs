using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Text.Json;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Utils;
using OngekiFumenEditor.Utils.Ogkr;
using OngekiFumenEditorPlugins.KngkSupport.Bases;

namespace OngekiFumenEditorPlugins.KngkSupport.Parsers.Kngk;

[Export(typeof(IFumenSerializable))]
[Export(typeof(DefaultKngkFumenFormatter))]
public class DefaultKngkFumenFormatter : IFumenSerializable
{
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true
    };

    public string FileFormatName => DefaultKngkFumenParser.FormatName;
    public string[] SupportFumenFileExtensions => DefaultKngkFumenParser.FumenFileExtensions;

    public async Task<byte[]> SerializeAsync(OngekiFumen fumen)
    {
        //standarize ongeki fumen
        var task = await StandardizeFormat.Process(fumen);
        if (!task.IsSuccess)
        {
            Log.LogError($"Can't standarize ongeki fumen: {task.Message}");
            return default;
        }

        var ongekiFumen = task.SerializedFumen;
        var kangekiFumen = new KngkFumen();

        ConvertInfo(ongekiFumen, kangekiFumen);
        ConvertLanes(ongekiFumen, kangekiFumen);
        ConvertBullets(ongekiFumen, kangekiFumen);
        ConvertLasers(ongekiFumen, kangekiFumen);
        ConvertBells(ongekiFumen, kangekiFumen);
        ConvertNotes(ongekiFumen, kangekiFumen);
        ConvertFlicks(ongekiFumen, kangekiFumen);
        ConvertCompositions(ongekiFumen, kangekiFumen);

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(kangekiFumen, serializerOptions));
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

        //wall left
        var wallLeftPoints = ConvertToKngkPoints(ongekiFumen.Lanes.Where(x => x.LaneType == LaneType.WallLeft));
        AppendGuardPoints(wallLeftPoints,
            ongekiFumen.LaneBlocks.Where(x => x.Direction == LaneBlockArea.BlockDirection.Left));
        PostProcessWallPoints(wallLeftPoints, -96);

        //wall right
        var wallRightPoints = ConvertToKngkPoints(ongekiFumen.Lanes.Where(x => x.LaneType == LaneType.WallRight));
        wallRightPoints.ForEach(x => x.Line = true); //force all line is true.
        AppendGuardPoints(wallRightPoints,
            ongekiFumen.LaneBlocks.Where(x => x.Direction == LaneBlockArea.BlockDirection.Right));
        PostProcessWallPoints(wallRightPoints, 96);

        kangekiFumen.Left = new ObservableCollection<Point>(wallLeftPoints);
        kangekiFumen.Right = new ObservableCollection<Point>(wallRightPoints);

        Log.LogDebug("done.");
    }

    private void PostProcessWallPoints(List<Point> wallLeftPoints, int outsizeX)
    {
        wallLeftPoints.ForEach(x => x.Line = true); //force all line is true.
        if (wallLeftPoints.LastOrDefault()?.Z < 57791)
        {
            Log.LogDebug($"append limit points, outsizeX: {outsizeX}");
            wallLeftPoints.Add(new Point
            {
                Curve = 1,
                Line = true,
                Y = 1,
                X = outsizeX,
                Z = 57791
            });
            wallLeftPoints.Add(new Point
            {
                Curve = 1,
                Line = true,
                Y = 1,
                X = outsizeX,
                Z = 57792
            });
        }
    }

    private void AppendGuardPoints(List<Point> wallLeftPoints, IEnumerable<LaneBlockArea> lbkList)
    {
        foreach (var lbk in lbkList)
        {
            var startZ = ConvertKngkTGrid(lbk.TGrid).TotalGrid;
            var endZ = ConvertKngkTGrid(lbk.EndIndicator.TGrid).TotalGrid;
            Log.LogDebug($"lbk zRange: [{startZ},{endZ}]");

            //make affected point.Gurad = true
            foreach (var point in wallLeftPoints.Where(x => x.Z >= startZ && x.Z <= endZ))
            {
                point.Guard = point.Line;
                Log.LogDebug($"    affected point: {point}");
            }

            void interpolate(int insertZ, bool isTail)
            {
                var idx = wallLeftPoints.FindLastIndex(x => x.Z <= insertZ);
                if (idx != -1)
                {
                    var curPoint = wallLeftPoints[idx];
                    if (curPoint.Z != insertZ)
                        if (wallLeftPoints.ElementAtOrDefault(idx + 1) is Point nextPoint)
                        {
                            //interpolate new point.
                            var insertX = MathUtils.CalculateXFromTwoPointFormFormula(insertZ, curPoint.X, curPoint.Z,
                                nextPoint.X,
                                nextPoint.Z);

                            var point = new Point();

                            point.X = (int) insertX;
                            point.Z = insertZ;
                            point.Line = curPoint.Line;
                            point.Guard = point.Line && !isTail;

                            point.Y = 1;
                            point.Curve = 1;
                            point.RelativeX = 0;

                            wallLeftPoints.Insert(idx + 1, point);
                            Log.LogDebug($"    interpolate new insertZ {insertZ} idx {idx} point: {point}");
                        }
                }
            }

            interpolate(startZ, false);
            interpolate(endZ, true);
        }
    }

    private static KngkTGrid ConvertKngkTGrid(TGrid ongkTGrid)
    {
        var totalOngkTUnit = ongkTGrid.TotalUnit;
        var kngkTGrid = new KngkTGrid((int) totalOngkTUnit, (int) (192 * (totalOngkTUnit - (int) totalOngkTUnit)));
        return kngkTGrid;
    }

    private List<Point> ConvertToKngkPoints(IEnumerable<ConnectableStartObject> lanes)
    {
        var list = new List<Point>();

        foreach (var lane in lanes.OrderBy(x => x.TGrid))
        {
            Log.LogDebug($"begin convert lane:{lane}");
            foreach (var connectable in lane.Children.AsEnumerable<ConnectableObjectBase>().Prepend(lane))
            {
                var point = new Point();

                point.X = (int) (connectable.XGrid.TotalUnit * 4);
                point.Z = ConvertKngkTGrid(connectable.TGrid).TotalGrid;
                point.Line = connectable.NextObject is not null;

                point.Y = 1;
                point.Curve = 1; //ongeki fumen has been standarized already.
                point.RelativeX = 0;
                point.Guard = false; //append guard point after them.

                list.Add(point);
                Log.LogDebug($"    convert {connectable}  ->  {point}");
            }
        }

        return list;
    }

    private void ConvertInfo(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");

        kangekiFumen.Designer = ongekiFumen.MetaInfo.Creator;
        kangekiFumen.Version = 400;
        kangekiFumen.Bpm = ongekiFumen.BpmList.FirstBpm.BPM.ToString();

        Log.LogDebug("done.");
    }
}