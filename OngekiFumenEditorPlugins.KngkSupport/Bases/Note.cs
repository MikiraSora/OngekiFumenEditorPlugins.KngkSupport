using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Note : ObservableObject
{
    [property: JsonPropertyName(nameof(align))]
    [ObservableProperty]
    private int align;

    [property: JsonPropertyName(nameof(ex))]
    [ObservableProperty]
    private bool ex;

    [property: JsonPropertyName(nameof(height))]
    [ObservableProperty]
    private int height;

    [property: JsonPropertyName(nameof(ignoreBpm))]
    [ObservableProperty]
    private bool ignoreBpm;

    [property: JsonPropertyName(nameof(ignoreSSpeed))]
    [ObservableProperty]
    private bool ignoreSSpeed;

    /// <summary>
    ///     物件归属轨道在Lane数组的索引位置
    /// </summary>
    [property: JsonPropertyName(nameof(lane))]
    [ObservableProperty]
    private int lane;

    [property: JsonPropertyName(nameof(rotation))]
    [ObservableProperty]
    private int rotation;

    [property: JsonPropertyName(nameof(speed))]
    [ObservableProperty]
    private int speed;

    [property: JsonPropertyName(nameof(type))]
    [ObservableProperty]
    private int type;

    [property: JsonPropertyName(nameof(upperX))]
    [ObservableProperty]
    private int upperX;

    [property: JsonPropertyName(nameof(width))]
    [ObservableProperty]
    private int width;

    /// <summary>
    ///     Begin time of object effect likes Hold/Tap
    ///     The meaning is the same as KngkTGrid.TotalGrid
    /// </summary>
    [property: JsonPropertyName(nameof(zBegin))]
    [ObservableProperty]
    private int zBegin;

    /// <summary>
    ///     End time of object effect likes Hold
    ///     Same as KngkTGrid.TotalGrid
    /// </summary>
    [property: JsonPropertyName(nameof(zEnd))]
    [ObservableProperty]
    private int zEnd;

    public override string ToString()
    {
        return
            $"zTime({zBegin}, {zEnd}) size:({width}, {height}) align:{align} rotation:{rotation} speed:{speed} lane:{lane} upperX:{upperX} ex:{ex} ignoreSSpeed:{ignoreSSpeed} ignoreBpm:{ignoreBpm}";
    }
}