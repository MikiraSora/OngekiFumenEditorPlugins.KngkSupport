using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Color : ObservableObject
{
    [property: JsonPropertyName(nameof(a))]
    [ObservableProperty]
    private float a;

    [property: JsonPropertyName(nameof(b))]
    [ObservableProperty]
    private float b;

    [property: JsonPropertyName(nameof(g))]
    [ObservableProperty]
    private float g;

    [property: JsonPropertyName(nameof(r))]
    [ObservableProperty]
    private float r;

    public override string ToString()
    {
        return $"RGBA({(byte) (r * 255)}, {(byte) (g * 255)}, {(byte) (b * 255)}, {(byte) (a * 255)})";
    }
}