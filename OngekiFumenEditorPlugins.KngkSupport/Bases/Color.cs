using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Color : ObservableObject
{
    [property: JsonPropertyName(nameof(a))]
    [ObservableProperty]
    private double a;

    [property: JsonPropertyName(nameof(b))]
    [ObservableProperty]
    private double b;

    [property: JsonPropertyName(nameof(g))]
    [ObservableProperty]
    private double g;

    [property: JsonPropertyName(nameof(r))]
    [ObservableProperty]
    private double r;

    public Color()
    {

    }

    public Color(System.Windows.Media.Color colorIdColor)
    {
        R = colorIdColor.R / 255f;
        G = colorIdColor.G / 255f;
        B = colorIdColor.B / 255f;
        A = colorIdColor.A / 255f;
    }

    public override string ToString()
    {
        return $"RGBA({(byte) (r * 255)}, {(byte) (g * 255)}, {(byte) (b * 255)}, {(byte) (a * 255)})";
    }
}