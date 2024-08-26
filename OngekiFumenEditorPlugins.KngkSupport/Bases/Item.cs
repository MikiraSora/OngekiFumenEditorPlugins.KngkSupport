using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Child : ObservableObject
{
    [property: JsonPropertyName(nameof(children))]
    [ObservableProperty]
    private int children;

    [property: JsonPropertyName(nameof(easeX))]
    [ObservableProperty]
    private string easeX;

    [property: JsonPropertyName(nameof(easeZ))]
    [ObservableProperty]
    private string easeZ;

    [property: JsonPropertyName(nameof(size))]
    [ObservableProperty]
    private int size;

    [property: JsonPropertyName(nameof(speed))]
    [ObservableProperty]
    private int speed;

    [property: JsonPropertyName(nameof(upperX))]
    [ObservableProperty]
    private int upperX;

    [property: JsonPropertyName(nameof(x))]
    [ObservableProperty]
    private int x;

    [property: JsonPropertyName(nameof(z))]
    [ObservableProperty]
    private int z;
}

public partial class Extra : ObservableObject
{
    [property: JsonPropertyName(nameof(begin))]
    [ObservableProperty]
    private int begin;

    [property: JsonPropertyName(nameof(duration))]
    [ObservableProperty]
    private int duration;

    [property: JsonPropertyName(nameof(ease))]
    [ObservableProperty]
    private string ease;

    [property: JsonPropertyName(nameof(type))]
    [ObservableProperty]
    private int type;

    [property: JsonPropertyName(nameof(value))]
    [ObservableProperty]
    private int value;
}

public partial class Item : ObservableObject
{
    [property: JsonPropertyName(nameof(children))]
    [ObservableProperty]
    private ObservableCollection<Child> children = new();

    [property: JsonPropertyName(nameof(ex))]
    [ObservableProperty]
    private bool ex;
    
    [JsonIgnore]
    public ItemType ItemType
    {
        get => (ItemType) Type;
        set => Type = (int)value;
    }

    [property: JsonPropertyName(nameof(extra))]
    [ObservableProperty]
    private ObservableCollection<Extra> extra = new();

    [property: JsonPropertyName(nameof(horming))]
    [ObservableProperty]
    private int horming;

    [property: JsonPropertyName(nameof(ignoreBpm))]
    [ObservableProperty]
    private bool ignoreBpm;

    [property: JsonPropertyName(nameof(ignoreSSpeed))]
    [ObservableProperty]
    private bool ignoreSSpeed;

    [property: JsonPropertyName(nameof(level))]
    [ObservableProperty]
    private int level;

    [property: JsonPropertyName(nameof(rotation))]
    [ObservableProperty]
    private int rotation;

    [property: JsonPropertyName(nameof(size))]
    [ObservableProperty]
    private int size;

    [property: JsonPropertyName(nameof(speed))]
    [ObservableProperty]
    private int speed;

    [property: JsonPropertyName(nameof(type))]
    [ObservableProperty]
    private int type;

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
}