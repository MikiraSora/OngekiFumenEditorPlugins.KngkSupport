using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Trace : ObservableObject
{
    [property: JsonPropertyName(nameof(alpha))]
    [ObservableProperty]
    private int alpha;


    [property: JsonPropertyName(nameof(height))]
    [ObservableProperty]
    private int height;


    [property: JsonPropertyName(nameof(locked))]
    [ObservableProperty]
    private bool locked;


    [property: JsonPropertyName(nameof(path))]
    [ObservableProperty]
    private string path;


    [property: JsonPropertyName(nameof(size))]
    [ObservableProperty]
    private int size;


    [property: JsonPropertyName(nameof(view))]
    [ObservableProperty]
    private bool view;


    [property: JsonPropertyName(nameof(x))]
    [ObservableProperty]
    private int x;

    [property: JsonPropertyName(nameof(z))]
    [ObservableProperty]
    private int z;


    public override string ToString()
    {
        return
            $"x:{x} z:{z} height:{height} locked:{locked} size:{size} alpha:{alpha} view:{view} path:{path}";
    }
}