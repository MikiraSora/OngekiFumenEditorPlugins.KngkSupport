using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Laser : ObservableObject
{
    [property: JsonPropertyName(nameof(curve))]
    [ObservableProperty]
    private int curve;

    [property: JsonPropertyName(nameof(isLast))]
    [ObservableProperty]
    private bool isLast;

    [property: JsonPropertyName(nameof(level))]
    [ObservableProperty]
    private int level;

    [property: JsonPropertyName(nameof(size))]
    [ObservableProperty]
    private int size;

    [property: JsonPropertyName(nameof(upperX))]
    [ObservableProperty]
    private int upperX;

    [property: JsonPropertyName(nameof(x))]
    [ObservableProperty]
    private int x;

    [property: JsonPropertyName(nameof(y))]
    [ObservableProperty]
    private int y;

    [property: JsonPropertyName(nameof(z))]
    [ObservableProperty]
    private int z;

    public override string ToString()
    {
        return $"XYZ({x}, {y}, {z}) isLast:{isLast} upperX:{upperX} size:{size} curve:{curve} level:{level}";
    }
}