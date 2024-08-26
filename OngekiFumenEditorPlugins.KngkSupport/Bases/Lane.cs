using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class Lane: ObservableObject
{
    [property: JsonPropertyName(nameof(points))]
    [ObservableProperty]
    private List<Point> points;
}