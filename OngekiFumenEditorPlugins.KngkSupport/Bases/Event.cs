using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Event : ObservableObject
{
    [property: JsonPropertyName(nameof(type))]
    [ObservableProperty]
    private int type;

    [property: JsonPropertyName(nameof(value))]
    [ObservableProperty]
    private int value;

    [property: JsonPropertyName(nameof(z))]
    [ObservableProperty]
    private int z;

    [JsonIgnore]
    public EventType EventType
    {
        get => (EventType) Type;
        set => Type = (int) value;
    }

    public override string ToString()
    {
        return
            $"z:{z} value:{value} type:{type}";
    }
}