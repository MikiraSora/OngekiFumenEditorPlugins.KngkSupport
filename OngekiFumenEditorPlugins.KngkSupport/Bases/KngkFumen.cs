using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OngekiFumenEditorPlugins.KngkSupport.Bases;

public partial class KngkFumen : ObservableObject
{
    [property: JsonPropertyName(nameof(artColors))]
    [ObservableProperty]
    private ObservableCollection<Color> artColors = new();

    [property: JsonPropertyName(nameof(artist))]
    [ObservableProperty]
    private string artist;

    [property: JsonPropertyName(nameof(artLanes))]
    [ObservableProperty]
    private ObservableCollection<Lane> artLanes = new();

    [property: JsonPropertyName(nameof(audioOffset))]
    [ObservableProperty]
    private float audioOffset;

    [property: JsonPropertyName(nameof(barDurations))]
    [ObservableProperty]
    private ObservableCollection<int> barDurations = new();

    [property: JsonPropertyName(nameof(bossAttr))]
    [ObservableProperty]
    private int bossAttr;

    [property: JsonPropertyName(nameof(bossLv))]
    [ObservableProperty]
    private int bossLv;

    [property: JsonPropertyName(nameof(bossName))]
    [ObservableProperty]
    private string bossName;

    [property: JsonPropertyName(nameof(bpm))]
    [ObservableProperty]
    private string bpm;

    [property: JsonPropertyName(nameof(creationTime))]
    [ObservableProperty]
    private int creationTime;

    [property: JsonPropertyName(nameof(designer))]
    [ObservableProperty]
    private string designer;

    [property: JsonPropertyName(nameof(difficulty))]
    [ObservableProperty]
    private int difficulty;

    [property: JsonPropertyName(nameof(events))]
    [ObservableProperty]
    private ObservableCollection<Event> events = new();

    [property: JsonPropertyName(nameof(genre))]
    [ObservableProperty]
    private string genre;

    [property: JsonPropertyName(nameof(items))]
    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [property: JsonPropertyName(nameof(laneColors))]
    [ObservableProperty]
    private ObservableCollection<Color> laneColors = new();

    [property: JsonPropertyName(nameof(lanes))]
    [ObservableProperty]
    private ObservableCollection<Lane> lanes = new();

    [property: JsonPropertyName(nameof(lasers))]
    [ObservableProperty]
    private ObservableCollection<Laser> lasers = new();

    [property: JsonPropertyName(nameof(left))]
    [ObservableProperty]
    private ObservableCollection<Point> left = new();

    [property: JsonPropertyName(nameof(leftColor))]
    [ObservableProperty]
    private Color leftColor;

    [property: JsonPropertyName(nameof(lv))]
    [ObservableProperty]
    private string lv;

    [property: JsonPropertyName(nameof(notes))]
    [ObservableProperty]
    private ObservableCollection<Note> notes = new();

    [property: JsonPropertyName(nameof(right))]
    [ObservableProperty]
    private ObservableCollection<Point> right = new();

    [property: JsonPropertyName(nameof(rightColor))]
    [ObservableProperty]
    private Color rightColor;

    [property: JsonPropertyName(nameof(scoreTitle))]
    [ObservableProperty]
    private string scoreTitle;

    [property: JsonPropertyName(nameof(title))]
    [ObservableProperty]
    private string title;

    [property: JsonPropertyName(nameof(traces))]
    [ObservableProperty]
    private ObservableCollection<Trace> traces = new();

    [property: JsonPropertyName(nameof(version))]
    [ObservableProperty]
    private int version;
}