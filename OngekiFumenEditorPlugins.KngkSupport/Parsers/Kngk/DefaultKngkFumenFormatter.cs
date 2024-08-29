using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Base.OngekiObjects.BulletPalleteEnums;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Base.OngekiObjects.Lane;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Utils;
using OngekiFumenEditor.Utils.Ogkr;
using OngekiFumenEditorPlugins.KngkSupport.Bases;
using OngekiFumenEditorPlugins.KngkSupport.Utils;
using Color = OngekiFumenEditorPlugins.KngkSupport.Bases.Color;

namespace OngekiFumenEditorPlugins.KngkSupport.Parsers.Kngk;

[Export(typeof(IFumenSerializable))]
[Export(typeof(DefaultKngkFumenFormatter))]
public partial class DefaultKngkFumenFormatter : IFumenSerializable
{
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string FileFormatName => "Kangeki Fumen File";
    public string[] SupportFumenFileExtensions => new[] {".kngk"};

    public async Task<byte[]> SerializeAsync(OngekiFumen fumen)
    {
        var kangekiFumen = await ConvertToKngkFumen(fumen);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(kangekiFumen, serializerOptions));
    }

    public async Task<KngkFumen> ConvertToKngkFumen(OngekiFumen fumen)
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

        var laneIdxMap = new Dictionary<int, int>();

        ConvertInfo(ongekiFumen, kangekiFumen);
        ConvertLanes(ongekiFumen, kangekiFumen, laneIdxMap);
        ConvertBullets(ongekiFumen, kangekiFumen);
        ConvertLasers(ongekiFumen, kangekiFumen);
        ConvertBells(ongekiFumen, kangekiFumen);
        ConvertNotes(ongekiFumen, kangekiFumen, laneIdxMap);
        ConvertFlicks(ongekiFumen, kangekiFumen);
        ConvertCompositions(ongekiFumen, kangekiFumen);

        return kangekiFumen;
    }

    private void ConvertCompositions(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");

        var eventList = new List<Event>();

        //BPM List
        foreach (var bpmChange in ongekiFumen.BpmList)
        {
            var evt = new Event();
            evt.EventType = EventType.Bpm;
            evt.Z = bpmChange.TGrid.ToKngkZ();
            evt.Value = (int) bpmChange.BPM;

            eventList.Add(evt);
        }

        //Meter List
        /*
        foreach (var meterChange in ongekiFumen.MeterChanges)
        {
            var evt = new Event();
            evt.EventType = EventType.Meter;
            evt.Z = meterChange.TGrid.ToKngkZ();
            evt.Value = (int) (meterChange.BunShi * 1.0f / meterChange.Bunbo * 192);

            eventList.Add(evt);
        }
        */

        var firstBpm = ongekiFumen.BpmList.FirstBpm.BPM;

        var currentBpm = firstBpm;
        var currentSpeed = 1d;

        var conflictTGrid = default(TGrid);
        var resolveConflictTime = -1;
        var confictZ = -1;

        void CommitSpeed(TGrid tGrid)
        {
            var evt = new Event();
            evt.EventType = EventType.Soflan;
            var weight = firstBpm / currentBpm;
            var fixedSpeed = (int) (weight * currentSpeed * 100);
            evt.Value = fixedSpeed;
            var z = tGrid.ToKngkZ();

            //check and resolve z-fighting
            if (confictZ == z && conflictTGrid < tGrid)
            {
                Log.LogDebug($"detected z-fight at {conflictTGrid} < {tGrid}, resolve {z} -> {resolveConflictTime}.");
                z = resolveConflictTime;
            }
            else
            {
                confictZ = z;
                resolveConflictTime = z;
            }

            resolveConflictTime++;
            conflictTGrid = tGrid;

            evt.Z = z;

            if (weight != 1)
                Log.LogDebug(
                    $"commit adjusted speed by different bpm at {tGrid} Z({evt.Z}): ({currentBpm:F2}/{firstBpm:F2} {weight * 100:F2}%) {currentSpeed * 100:F2}x -> {fixedSpeed:F2}x");
            else
                Log.LogDebug(
                    $"commit speed at {tGrid} Z({evt.Z}): {currentBpm:F2} {fixedSpeed:F2}x");

            if (eventList.LastOrDefault() is Event prevEvt && prevEvt.Z == evt.Z)
            {
                eventList.RemoveAt(eventList.Count - 1);
                Log.LogDebug(
                    $"remove dup speed at Z({evt.Z})");
            }

            eventList.Add(evt);
        }

        foreach (var soflanPoint in ongekiFumen.Soflans.GetCachedSoflanPositionList_PreviewMode(ongekiFumen.BpmList))
        {
            if (currentBpm != soflanPoint.Bpm.BPM)
            {
                currentBpm = soflanPoint.Bpm.BPM;
                CommitSpeed(soflanPoint.TGrid);
            }

            if (soflanPoint.Speed != currentSpeed)
            {
                currentSpeed = soflanPoint.Speed;
                CommitSpeed(soflanPoint.TGrid);
            }
        }

        //Soflan List
        /*
        foreach (var keyframeSoflan in ongekiFumen.Soflans.GenerateKeyframeSoflans(ongekiFumen.BpmList))
        {
            var evt = new Event();
            evt.EventType = EventType.Soflan;
            evt.Z = keyframeSoflan.TGrid.ToKngkZ();
            evt.Value = (int)(keyframeSoflan.Speed * 100);

            eventList.Add(evt);
        }
        */

        //boss event
        if (ongekiFumen.EnemySets.FirstOrDefault(x => x.TagTblValue == EnemySet.WaveChangeConst.Boss) is EnemySet
            enemySet)
        {
            var evt = new Event();
            evt.EventType = EventType.Boss;
            evt.Z = enemySet.TGrid.ToKngkZ();

            eventList.Add(evt);
        }

        eventList.SortBy(x => x.Z);

        kangekiFumen.Events = new ObservableCollection<Event>(eventList);

        Log.LogDebug("done.");
    }

    private void ConvertFlicks(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");

        var flickList = new List<Item>();

        foreach (var ogkrFlick in ongekiFumen.Flicks)
        {
            var kngkFlick = new Item();
            kngkFlick.ItemType = ogkrFlick.Direction switch
            {
                Flick.FlickDirection.Left => ItemType.FlickLeft,
                Flick.FlickDirection.Right => ItemType.FlickRight
            };
            kngkFlick.Z = ogkrFlick.TGrid.ToKngkZ();
            kngkFlick.X = ogkrFlick.XGrid.ToKngkX();
            kngkFlick.Ex = ogkrFlick.IsCritical;

            flickList.Add(kngkFlick);
        }

        kangekiFumen.Items.AddRange(flickList);
        Log.LogDebug("done.");
    }

    private void ConvertNotes(OngekiFumen ongekiFumen, KngkFumen kangekiFumen, Dictionary<int, int> laneIdxMap)
    {
        Log.LogDebug("begin.");

        var noteList = new List<Note>();

        void ApplyCommon<T>(Note note, T obj)
            where T : IHorizonPositionObject, ITimelineObject, ICriticalableObject, ILaneDockable
        {
            note.Type = 0;
            note.UpperX = obj.XGrid.ToKngkX();
            note.ZBegin = obj.TGrid.ToKngkZ();
            note.ZEnd = obj.TGrid.ToKngkZ();
            note.Ex = obj.IsCritical;

            note.Width = 100;
            note.Height = 100;

            if (laneIdxMap.TryGetValue(obj.ReferenceLaneStrId, out var kngkLaneIdx))
                note.Lane = kngkLaneIdx;
        }

        foreach (var ogkrTap in ongekiFumen.Taps)
        {
            var kngkTap = new Note();
            ApplyCommon(kngkTap, ogkrTap);

            noteList.Add(kngkTap);
        }

        foreach (var ogkrHold in ongekiFumen.Holds)
        {
            var kngkTap = new Note();
            ApplyCommon(kngkTap, ogkrHold);
            kngkTap.ZEnd = ogkrHold.EndTGrid.ToKngkZ();

            noteList.Add(kngkTap);
        }

        kangekiFumen.Notes.AddRange(noteList);

        Log.LogDebug("done.");
    }

    private void ConvertBells(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");

        var bellList = new List<Item>();

        foreach (var ogkrBell in ongekiFumen.Bells)
        {
            var kngkBell = new Item();
            kngkBell.ItemType = ItemType.Pudding;
            kngkBell.X = ogkrBell.XGrid.ToKngkX();
            kngkBell.Z = ogkrBell.TGrid.ToKngkZ();

            kngkBell.Size = 100;
            kngkBell.Horming = 0;
            kngkBell.Rotation = 0;
            kngkBell.Level = 1;
            kngkBell.UpperX = 0;
            kngkBell.Speed = 100;

            if (ogkrBell.ReferenceBulletPallete is BulletPallete bulletPallete)
            {
                kngkBell.Speed = (int) (bulletPallete.Speed * 100);
                kngkBell.UpperX = bulletPallete.PlaceOffset;

                if (bulletPallete.TargetValue == Target.Player)
                    kngkBell.Horming = 192;

                if (bulletPallete.SizeValue == BulletSize.Large)
                    kngkBell.Size = 130;
            }

            bellList.Add(kngkBell);
        }

        kangekiFumen.Items.AddRange(bellList);

        Log.LogDebug("done.");
    }

    private void ConvertLasers(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");

        var laserList = new List<Laser>();

        foreach (var ogkrLaser in ongekiFumen.Beams)
        {
            int topX = default;
            if (ogkrLaser.ObliqueSourceXGridOffset != null)
                topX = (int) ((ogkrLaser.ObliqueSourceXGridOffset.TotalUnit + ogkrLaser.XGrid.TotalUnit) * 4);

            foreach (var connectable in ogkrLaser.Children.AsEnumerable<ConnectableObjectBase>().Prepend(ogkrLaser))
            {
                var laserNode = new Laser();
                laserNode.IsLast = connectable.NextObject is null;
                laserNode.Z = connectable.TGrid.ToKngkZ();
                laserNode.X = connectable.XGrid.ToKngkX();
                laserNode.Y = 1;
                laserNode.Size = (int) (ogkrLaser.WidthId * 3.0f / 2) * 4;
                laserNode.Curve = 1;
                laserNode.Level = 1;

                if (ogkrLaser.ObliqueSourceXGridOffset != null)
                    laserNode.UpperX = topX - laserNode.X;

                laserList.Add(laserNode);
            }
        }

        kangekiFumen.Lasers.AddRange(laserList);

        Log.LogDebug("done.");
    }

    private void ConvertBullets(OngekiFumen ongekiFumen, KngkFumen kangekiFumen)
    {
        Log.LogDebug("begin.");
        Log.LogDebug("begin.");

        var bulletList = new List<Item>();

        foreach (var ogkrBullet in ongekiFumen.Bullets)
        {
            var kngkBullet = new Item();
            kngkBullet.X = ogkrBullet.XGrid.ToKngkX();
            kngkBullet.Z = ogkrBullet.TGrid.ToKngkZ();

            kngkBullet.Size = 100;
            kngkBullet.Horming = 0;
            kngkBullet.Rotation = 0;
            kngkBullet.UpperX = 0;

            if (ogkrBullet.ReferenceBulletPallete is not BulletPallete bulletPallete)
                continue;

            kngkBullet.Speed = (int) (bulletPallete.Speed * 100);
            kngkBullet.ItemType = bulletPallete.TypeValue switch
            {
                BulletType.Circle => ItemType.TapiocaDonut,
                BulletType.Needle => ItemType.TapiocaCrepe,
                BulletType.Square => ItemType.TapiocaCastella
            };
            kngkBullet.Level = ogkrBullet.BulletDamageTypeValue switch
            {
                Bullet.BulletDamageType.Normal => 1,
                Bullet.BulletDamageType.Hard => 2,
                Bullet.BulletDamageType.Danger => 3
            };

            if (bulletPallete.PlaceOffset != 0)
                kngkBullet.UpperX = (int) ((ogkrBullet.XGrid.TotalUnit + bulletPallete.PlaceOffset) * 4);

            if (bulletPallete.TargetValue == Target.Player)
                kngkBullet.Horming = 192;

            if (bulletPallete.SizeValue == BulletSize.Large)
                kngkBullet.Size = 130;

            bulletList.Add(kngkBullet);
        }

        kangekiFumen.Items.AddRange(bulletList);

        Log.LogDebug("done.");
    }

    private void ConvertLanes(OngekiFumen ongekiFumen, KngkFumen kangekiFumen, Dictionary<int, int> laneIdxMap)
    {
        Log.LogDebug("begin.");

        kangekiFumen.RightColor = new Color
        {
            A = 1,
            R = 0.7803922295570374,
            G = 0.0784313827753067,
            B = 0.5176470875740051
        };
        kangekiFumen.LeftColor = new Color
        {
            A = 1,
            R = 0.40392160415649416,
            G = 0.0784313827753067,
            B = 0.7803922295570374
        };

        //init kangeki lanes/colors
        kangekiFumen.Lanes.AddRange(Enumerable.Repeat(0, 28).Select(x => new Lane()));
        kangekiFumen.ArtLanes.AddRange(Enumerable.Repeat(0, 9).Select(x => new Lane()));
        kangekiFumen.LaneColors.AddRange(Enumerable.Repeat(0, 28).Select(x => new Color(Colors.Plum)));
        kangekiFumen.ArtColors.AddRange(Enumerable.Repeat(0, 9).Select(x => new Color(Colors.Plum)));

        //wall left
        var wallLeftStarts = ongekiFumen.Lanes.Where(x => x.LaneType == LaneType.WallLeft);
        var wallLeftPoints = ConvertToKngkPoints(wallLeftStarts);
        wallLeftStarts.ForEach(x => laneIdxMap[x.RecordId] = 0);
        if (wallLeftPoints.FirstOrDefault() is not Point p || p.Z > 0)
            wallLeftPoints.Insert(0, new Point
            {
                X = 0,
                Y = -1,
                Z = 0,
                Curve = 1,
                RelativeX = 0,
                Guard = false,
                Line = false
            });
        AppendGuardPoints(wallLeftPoints,
            ongekiFumen.LaneBlocks.Where(x => x.Direction == LaneBlockArea.BlockDirection.Left));
        wallLeftPoints.ForEach(x => x.Line = true); //force all wall point line is true.
        CheckAndAddTailPoints(wallLeftPoints, -96);
        kangekiFumen.Left = new ObservableCollection<Point>(wallLeftPoints);

        //wall right
        var wallRightStarts = ongekiFumen.Lanes.Where(x => x.LaneType == LaneType.WallRight);
        var wallRightPoints = ConvertToKngkPoints(wallRightStarts);
        wallRightStarts.ForEach(x => laneIdxMap[x.RecordId] = 25);
        if (wallRightPoints.FirstOrDefault() is not Point p2 || p2.Z > 0)
            wallRightPoints.Insert(0, new Point
            {
                X = 0,
                Y = -1,
                Z = 0,
                Curve = 1,
                RelativeX = 0,
                Guard = false,
                Line = false
            });
        AppendGuardPoints(wallRightPoints,
            ongekiFumen.LaneBlocks.Where(x => x.Direction == LaneBlockArea.BlockDirection.Right));
        wallRightPoints.ForEach(x => x.Line = true); //force all wall point line is true.
        CheckAndAddTailPoints(wallRightPoints, 96);
        kangekiFumen.Right = new ObservableCollection<Point>(wallRightPoints);

        //first deal with note lanes, the remaining note lanes can be used as colorful lanes
        ProcessNoteLanes(ongekiFumen, kangekiFumen, laneIdxMap, LaneType.Left, out var avaliableLeftLanes);
        ProcessNoteLanes(ongekiFumen, kangekiFumen, laneIdxMap, LaneType.Center, out var avaliableCenterLanes);
        ProcessNoteLanes(ongekiFumen, kangekiFumen, laneIdxMap, LaneType.Right, out var avaliableRightLanes);

        //for colorful lane
        var avaliableLaneIds = avaliableRightLanes.Concat(avaliableCenterLanes).Concat(avaliableLeftLanes).ToList();
        var avaliableColorfulLaneIds = Enumerable.Range(0, 9).ToList();
        foreach (var group in ongekiFumen.Lanes.OfType<ColorfulLaneStart>().GroupBy(x => x.ColorId))
        {
            var starts = group.ToList();
            var colorId = group.Key;

            ProcessColofulLanes(starts, kangekiFumen, avaliableLaneIds, avaliableColorfulLaneIds,
                new Color(colorId.Color));
        }

        Log.LogDebug("done.");
    }

    private void ProcessNoteLanes(OngekiFumen ongekiFumen, KngkFumen kangekiFumen, Dictionary<int, int> laneIdxMap,
        LaneType type,
        out List<int> avaliableNoteLanes)
    {
        var list = avaliableNoteLanes = Enumerable.Range(0, 8).Select(x => 3 * x + type switch
        {
            LaneType.Left => 0,
            LaneType.Center => 1,
            LaneType.Right => 2,
            _ => throw new ArgumentException($"laneType not support: {type}")
        }).ToList();
        foreach (var idx in list)
            kangekiFumen.Lanes[idx] = new Lane();
        var stateMap = new HashSet<LaneUsageState>();

        bool TryUseNewAvaliableNoteLane(out int laneIdx)
        {
            laneIdx = -1;
            if (list.Count > 0)
            {
                laneIdx = list[0];
                list.RemoveAt(0);
                return true;
            }

            return false;
        }

        bool TryGetCommitableLaneState(TGrid laneStartTGrid, out LaneUsageState laneUsageState)
        {
            if (stateMap.FirstOrDefault(x => x.CurrentCommitedTGrid <= laneStartTGrid) is not LaneUsageState state)
            {
                //need require new lane
                if (!TryUseNewAvaliableNoteLane(out var newIdx))
                {
                    //dump infos.
                    Log.LogError($"No more avaliable {type} lane to put converted points:");
                    Log.LogError($"    laneStartTGrid: {laneStartTGrid}");
                    foreach (var s in stateMap)
                        Log.LogError(
                            $"    state laneIdx: {s.KngkLaneIdx}, generated points: {s.Points.Count}, commited TGrid: {s.CurrentCommitedTGrid}");
                    laneUsageState = default;
                    return false;
                }

                state = new LaneUsageState
                {
                    KngkLaneIdx = newIdx
                };
                stateMap.Add(state);
            }

            laneUsageState = state;
            return true;
        }

        var starts = ongekiFumen.Lanes.Where(x => x.LaneType == type).OrderBy(x => x.TGrid).ToArray();

        foreach (var start in starts)
        {
            if (!TryGetCommitableLaneState(start.TGrid, out var laneUsageState))
            {
                Log.LogError($"Convert ogkr {type} lane to kngk {type} lane failed.");
                return;
            }

            Log.LogDebug($"begin convert lane:{start} at laneIdx:{laneUsageState.KngkLaneIdx}");
            foreach (var connectable in start.Children.AsEnumerable<ConnectableObjectBase>().Prepend(start))
            {
                var point = new Point();

                point.X = connectable.XGrid.ToKngkX();
                point.Z = connectable.TGrid.ToKngkTGrid().TotalGrid;
                point.Line = connectable.NextObject is not null;

                point.Y = 1;
                point.Curve = 1; //ongeki fumen has been standarized already.
                point.RelativeX = 0;
                point.Guard = false;

                laneUsageState.Points.Add(point);
                Log.LogDebug($"    convert {connectable}  ->  {point}");
            }

            //commit tGrid for next pickup
            laneIdxMap[start.RecordId] = laneUsageState.KngkLaneIdx + 1;
            laneUsageState.CurrentCommitedTGrid = start.MaxTGrid;
        }

        foreach (var kngkLaneIdx in avaliableNoteLanes)
        {
            kangekiFumen.Lanes[kngkLaneIdx] = new Lane
            {
                Points = new ObservableCollection<Point>
                {
                    new()
                    {
                        X = 0,
                        Y = -1,
                        Z = 0,
                        Curve = 1,
                        RelativeX = 0,
                        Guard = false,
                        Line = false
                    }
                }
            };
            CheckAndAddTailPoints(kangekiFumen.Lanes[kngkLaneIdx].Points, 0);
        }

        foreach (var state in stateMap)
        {
            var idx = state.KngkLaneIdx;
            var points = state.Points;

            if (points[0].Z > 0)
                //insert a point at Z=0
                points.Insert(0,
                    new Point
                    {
                        X = 0,
                        Y = -1,
                        Z = 0,
                        Curve = 1,
                        RelativeX = 0,
                        Guard = false,
                        Line = false
                    }
                );

            /*
            //last node of lane must Line=true
            points[points.Count - 1].Line = true;
            */
            CheckAndAddTailPoints(points, 0);

            kangekiFumen.Lanes[idx] = new Lane
            {
                Points = points
            };

            kangekiFumen.LaneColors[idx] = new Color(type switch
            {
                LaneType.Left => ColorIdConst.LaneRed.Color,
                LaneType.Center => ColorIdConst.LaneGreen.Color,
                LaneType.Right => ColorIdConst.LaneBlue.Color
            });
        }
    }

    private void ProcessColofulLanes(IEnumerable<ConnectableStartObject> starts, KngkFumen kangekiFumen,
        List<int> avaliableLaneIds,
        List<int> avaliableColorfulLaneIds, Color color)
    {
        var laneStates = new HashSet<LaneUsageState>();

        // laneIdx reference noteLane index if is nagetive number,otherwise colorfulLane index
        bool TryUseNewAvaliableLane(out int laneIdx)
        {
            laneIdx = -1;

            if (avaliableColorfulLaneIds.Count > 0)
            {
                laneIdx = avaliableColorfulLaneIds[0];
                avaliableColorfulLaneIds.RemoveAt(0);
                return true;
            }

            if (avaliableLaneIds.Count > 0)
            {
                laneIdx = -(avaliableLaneIds[0] + 1);
                avaliableLaneIds.RemoveAt(0);
                return true;
            }

            return false;
        }

        bool TryGetCommitableLaneState(TGrid laneStartTGrid, out LaneUsageState laneUsageState)
        {
            if (laneStates.FirstOrDefault(x => x.CurrentCommitedTGrid <= laneStartTGrid) is not LaneUsageState state)
            {
                //need require new lane
                if (!TryUseNewAvaliableLane(out var newIdx))
                {
                    //dump infos.
                    Log.LogError($"No more avaliable colorful({color}) lane to put converted points:");
                    Log.LogError($"    laneStartTGrid: {laneStartTGrid}");
                    foreach (var s in laneStates)
                        Log.LogError(
                            $"    state laneIdx: {s.KngkLaneIdx}, generated points: {s.Points.Count}, commited TGrid: {s.CurrentCommitedTGrid}");
                    laneUsageState = default;
                    return false;
                }

                state = new LaneUsageState
                {
                    KngkLaneIdx = newIdx
                };
                laneStates.Add(state);
            }

            laneUsageState = state;
            return true;
        }

        foreach (var start in starts)
        {
            if (!TryGetCommitableLaneState(start.TGrid, out var laneUsageState))
            {
                Log.LogError("Convert ogkr colorful lane to kngk art lane failed.");
                return;
            }

            Log.LogDebug($"begin convert lane:{start} at laneIdx:{laneUsageState.KngkLaneIdx}");
            foreach (var connectable in start.Children.AsEnumerable<ConnectableObjectBase>().Prepend(start))
            {
                var point = new Point();

                point.X = connectable.XGrid.ToKngkX();
                point.Z = connectable.TGrid.ToKngkTGrid().TotalGrid;
                point.Line = connectable.NextObject is not null;

                point.Y = 1;
                point.Curve = 1; //ongeki fumen has been standarized already.
                point.RelativeX = 0;
                point.Guard = false;

                laneUsageState.Points.Add(point);
                Log.LogDebug($"    convert {connectable}  ->  {point}");
            }

            //commit tGrid for next pickup
            laneUsageState.CurrentCommitedTGrid = start.MaxTGrid;
        }

        foreach (var state in laneStates)
        {
            var idx = state.KngkLaneIdx;
            var points = state.Points;

            if (idx < 0)
            {
                var fixIdx = -idx - 1;
                kangekiFumen.Lanes[fixIdx] = new Lane
                {
                    Points = points
                };
                kangekiFumen.LaneColors[fixIdx] = color;
            }
            else
            {
                kangekiFumen.ArtLanes[idx] = new Lane
                {
                    Points = points
                };
                kangekiFumen.ArtColors[idx] = color;
            }
        }
    }

    private void CheckAndAddTailPoints(IList<Point> points, int outsizeX)
    {
        if (points.LastOrDefault()?.Z < 57791)
        {
            Log.LogDebug($"append limit points, outsizeX: {outsizeX}");
            points.Add(new Point
            {
                Curve = 1,
                Line = true,
                Y = 1,
                X = outsizeX,
                Z = 57791
            });
            points.Add(new Point
            {
                Curve = 1,
                Line = false,
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
            var startZ = lbk.TGrid.ToKngkTGrid().TotalGrid;
            var endZ = lbk.EndIndicator.TGrid.ToKngkTGrid().TotalGrid;
            Log.LogDebug($"lbk zRange: [{startZ},{endZ}]");

            //make affected point.Gurad = true
            foreach (var point in wallLeftPoints.Where(x => x.Z >= startZ && x.Z < endZ))
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

    private List<Point> ConvertToKngkPoints(IEnumerable<ConnectableStartObject> lanes)
    {
        var list = new List<Point>();

        foreach (var start in lanes.OrderBy(x => x.TGrid))
        {
            Log.LogDebug($"begin convert lane:{start}");
            foreach (var connectable in start.Children.AsEnumerable<ConnectableObjectBase>().Prepend(start))
            {
                var point = new Point();

                point.X = connectable.XGrid.ToKngkX();
                point.Z = connectable.TGrid.ToKngkTGrid().TotalGrid;
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

    private partial class LaneUsageState : ObservableObject
    {
        [ObservableProperty]
        private TGrid currentCommitedTGrid = new();

        public int KngkLaneIdx { get; set; }
        public ObservableCollection<Point> Points { get; } = new();
    }
}