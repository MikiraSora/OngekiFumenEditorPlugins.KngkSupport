using OngekiFumenEditor.Base;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public class KngkTGrid : GridBase
{
    /// <summary>
    /// </summary>
    /// <param name="barString">for example: "12:3.38"</param>
    public KngkTGrid(string barString) : this(0, 0)
    {
        if (string.IsNullOrWhiteSpace(barString))
            goto BAD_ARG;
        //recalculate unit/grid by parsing barString
        const int tick = 192 / 4;
        var split = barString.Split(":");
        if (split.Length != 2)
            goto BAD_ARG;

        if (!int.TryParse(split[0], out var unit) || unit < 0)
            goto BAD_ARG;
        Unit = unit;
        if (!float.TryParse(split[1], out var dw) || dw < 0)
            goto BAD_ARG;
        Grid = (int) ((int) dw * tick + tick * (dw - (int) dw));

        return;
        BAD_ARG:
        throw new ArgumentException($"barString must be like \"12:3.38\" , current barString: {barString}");
    }

    public KngkTGrid(float unit, int grid) : base(unit, grid)
    {
        GridRadix = 192;
    }

    public override string Serialize()
    {
        throw new NotImplementedException();
    }

    public TGrid AsOngkTGrid()
    {
        var tGrid = new TGrid(Unit, (int) (Grid * 1.0f / GridRadix * TGrid.DEFAULT_RES_T));
        return tGrid;
    }

    public static KngkTGrid operator +(KngkTGrid l, GridOffset r)
    {
        var unit = l.Unit + r.Unit;
        var grid = r.Grid + l.Grid;

        while (grid < 0)
        {
            unit = unit - 1;
            grid = (int) (grid + l.GridRadix);
        }

        unit += grid / l.GridRadix;
        grid = (int) (grid % l.GridRadix);

        return new KngkTGrid(unit, grid);
    }

    public static KngkTGrid operator -(KngkTGrid l, GridOffset r)
    {
        var lGrids = l.TotalGrid;
        var rGrids = r.TotalGrid(l.GridRadix);

        var grid = lGrids - rGrids;
        if (grid < 0)
            return null;

        var t = new KngkTGrid(0, grid);
        t.NormalizeSelf();

        return t;
    }
}