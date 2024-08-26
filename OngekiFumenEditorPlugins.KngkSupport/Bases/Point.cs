using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Point : ObservableObject
{
    [property: JsonPropertyName(nameof(curve))]
    [ObservableProperty]
    private int curve;

    [property: JsonPropertyName(nameof(guard))]
    [ObservableProperty]
    private bool guard;

    [property: JsonPropertyName(nameof(line))]
    [ObservableProperty]
    private bool line;

    [property: JsonPropertyName(nameof(relativeX))]
    [ObservableProperty]
    private int relativeX;

    [property: JsonPropertyName(nameof(x))]
    [ObservableProperty]
    private int x;

    [property: JsonPropertyName(nameof(y))]
    [ObservableProperty]
    private int y;

    /// <summary>
    /// Same as KngkTGrid.TotalGrid
    /// </summary>
    [property: JsonPropertyName(nameof(z))]
    [ObservableProperty]
    private int z;
    
    public override string ToString()
    {
        return $"XYZ({x}, {y}, {z}) relativeX:{relativeX} line:{line} gard:{guard} curve:{curve}";
    }
}