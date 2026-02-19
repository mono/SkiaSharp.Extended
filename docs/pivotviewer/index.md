# PivotViewer

PivotViewer is a powerful data visualization control originally created by Microsoft for Silverlight. It lets you explore large collections of items with faceted filtering, grid and histogram views, smooth animated transitions, and word-wheel search. SkiaSharp.Extended brings this full experience to modern .NET.

## What is PivotViewer?

Imagine browsing 1,000 concept cars: you see them all as a grid of thumbnails. Click "Sports" in the filter pane and the grid smoothly animates to show only sports cars. Switch to graph view and see a histogram grouped by manufacturer. Click a car to see its details. That's PivotViewer.

## Quick Start

### From CXML (Silverlight compatibility)

```csharp
using SkiaSharp.Extended.PivotViewer;

// Parse a CXML file
string cxml = File.ReadAllText("collection.cxml");
var source = CxmlCollectionSource.Parse(cxml);

// Create a controller
var controller = new PivotViewerController();
controller.LoadCollection(source);
controller.SetAvailableSize(1024, 768);

// Filter: show only German cars
controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");

// Get filtered items
var items = controller.InScopeItems;

// Search
var results = controller.Search("Ferrari");
```

### Direct Data Binding (SL5 pattern)

```csharp
var nameP = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
var yearP = new PivotViewerNumericProperty("Year") { DisplayName = "Year", Format = "###0" };

var items = new List<PivotViewerItem>();
var item = new PivotViewerItem("car-1");
item.Add(nameP, "Ford GT");
item.Add(yearP, 2005.0);
items.Add(item);

controller.LoadItems(items, new[] { nameP, yearP });
```

### MAUI View

```xml
<pivotviewer:SKPivotViewerView x:Name="pvView"
    AccentColor="CornflowerBlue"
    View="grid"
    SelectedItem="{Binding Selected, Mode=TwoWay}" />
```

```csharp
var source = CxmlCollectionSource.Parse(cxmlXml);
pvView.LoadCollection(source);
pvView.SelectionChanged += (s, e) => ShowDetails(e.NewItem);
```

#### BindableProperties (SL5 API)

| Property | Type | Binding | Notes |
|---|---|---|---|
| `ItemsSource` | `IEnumerable<PivotViewerItem>` | OneWay | Bind items directly |
| `PivotProperties` | `IEnumerable<PivotViewerProperty>` | OneWay | Facet definitions |
| `SelectedItem` | `PivotViewerItem?` | TwoWay | Currently selected |
| `SelectedIndex` | `int` | TwoWay | Index of selected |
| `SortPivotProperty` | `PivotViewerProperty?` | TwoWay | Active sort facet |
| `SortDescending` | `bool` | TwoWay | Reverse sort order |
| `View` | `string` | TwoWay | "grid" or "graph" |
| `AccentColor` | `Color` | OneWay | UI accent color |
| `ItemCulture` | `CultureInfo` | OneWay | Date/number locale |
| `IsFilterPaneVisible` | `bool` | TwoWay | Show/hide filter pane |
| `CollectionUri` | `string` | OneWay | Auto-loads CXML + DZC from URI |
| `ControlBackground` | `Color` | OneWay | Filter pane / control bar background |
| `SecondaryBackground` | `Color` | OneWay | Detail pane background |
| `SecondaryForeground` | `Color` | OneWay | Secondary text color |

#### Loading from URI

The simplest way to show a PivotViewer collection is with `CollectionUri`:

```xml
<pivotviewer:SKPivotViewerView
    CollectionUri="https://example.com/collection.cxml"
    AccentColor="CornflowerBlue" />
```

This automatically downloads the CXML, parses it, loads the DZC thumbnails, and displays the collection. No code-behind needed.

## CXML File Format

Collection XML (`.cxml`) defines item collections with faceted metadata:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Collection xmlns="http://schemas.microsoft.com/collection/metadata/2009"
            xmlns:p="http://schemas.microsoft.com/livelabs/pivot/collection/2009"
            Name="Concept Cars" SchemaVersion="1.0">
  <FacetCategories>
    <FacetCategory Name="Manufacturer" Type="String" p:IsFilterVisible="true" />
    <FacetCategory Name="Year" Type="Number" Format="###0" p:IsFilterVisible="true" />
  </FacetCategories>
  <Items ImgBase="collection.dzc">
    <Item Id="1" Img="#0" Name="Ford GT" Href="http://...">
      <Facets>
        <Facet Name="Manufacturer"><String Value="Ford"/></Facet>
        <Facet Name="Year"><Number Value="2005"/></Facet>
      </Facets>
    </Item>
  </Items>
</Collection>
```

### CXML Type Mapping

| CXML Type | API Type | Filter Widget |
|---|---|---|
| `String` | `PivotViewerPropertyType.Text` | Checkbox list |
| `LongString` | `Text` + `WrappingText` flag | Detail pane only |
| `Number` | `PivotViewerPropertyType.Decimal` | Histogram + range |
| `DateTime` | `PivotViewerPropertyType.DateTime` | Timeline range |
| `Link` | `PivotViewerPropertyType.Link` | Clickable in detail |

## Filtering

Filters use **AND across properties, OR within a property** — exactly matching Silverlight behavior.

```csharp
// Show items that are (BMW OR Mercedes) AND (Year >= 2000 AND Year <= 2010)
controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
controller.FilterEngine.AddStringFilter("Manufacturer", "Mercedes");
controller.FilterEngine.AddNumericRangeFilter("Year", 2000, 2010);

// In-scope counts for filter pane (considers other active filters)
var counts = controller.GetFilterCounts("Body Style");
// Returns: { "Coupe": 5, "Sedan": 3, "SUV": 1 }
```

## Views

| View | Description |
|---|---|
| **Grid** | Items in a responsive grid. Default view. |
| **Graph** | Histogram: items grouped by selected facet, stacked as columns. |

```csharp
controller.CurrentView = "graph";
controller.SortProperty = controller.Properties.First(p => p.Id == "Manufacturer");
```

## Word Wheel Search

Type-ahead search with progressive narrowing:

```csharp
var results = controller.Search("Fer");
// Returns: [{ Text: "Ferrari Enzo", ItemCount: 1 }, { Text: "Ferrari", ItemCount: 3 }]
```

## State Serialization

Save and restore the complete viewer state (filters, sort, view, selection) as a URL-safe string:

```csharp
string state = controller.SerializeViewerState();
// "Manufacturer=BMW&$view=graph&$sort=Year&$select=car-42"

controller.SetViewerState(state);
```

## Architecture

```
SkiaSharp.Extended.PivotViewer (core, netstandard2.0 + net9.0)
├── CxmlCollectionSource        — CXML parser with type mapping
├── PivotViewerItem             — Item with string indexer + typed accessors
├── PivotViewerProperty         — Property hierarchy (String, Numeric, DateTime, Link)
├── FilterEngine                — AND/OR filtering + in-scope counts + histograms
├── GridLayoutEngine            — Grid + histogram layout computation
├── WordWheelIndex              — Prefix search with CharBucket grouping
├── ViewerStateSerializer       — URL-safe state round-trip
├── PivotViewerController       — Orchestrator: filter → sort → layout → state
├── FilterPaneModel             — Core model for filter pane UI
├── DetailPaneModel             — Core model for detail pane display
├── LayoutTransitionManager     — Animated layout transitions (EaseOutCubic)
├── HistogramBucketer           — Numeric/DateTime/String bucketing for graphs
├── PivotViewerCollectionBuilder — Fluent API for programmatic collection creation
├── BatchObservableCollection   — Bulk Add/Replace/Remove operations
├── GradualObservableCollection — Incremental loading with ItemsPerCycle
├── PivotViewerItemAdorner      — Item hover/selection adorner
├── PivotViewerGridView         — Grid view (sealed)
└── PivotViewerGraphView        — Graph/histogram view (sealed)

SkiaSharp.Extended.UI.Maui.PivotViewer (MAUI)
└── SKPivotViewerView           — ContentView with full BindableProperties + gestures
```

## Programmatic Collection Builder

Create collections in code without CXML:

```csharp
using SkiaSharp.Extended.PivotViewer;

var (items, properties) = new PivotViewerCollectionBuilder()
    .AddStringProperty("Name", "Name", PivotViewerPropertyOptions.CanFilter | PivotViewerPropertyOptions.CanSearchText)
    .AddNumericProperty("Year", "Year", "###0", PivotViewerPropertyOptions.CanFilter)
    .AddItem("1", b => b.Set("Name", "Ferrari Enzo").Set("Year", 2002.0))
    .AddItem("2", b => b.Set("Name", "Lamborghini Murciélago").Set("Year", 2001.0))
    .Build();

controller.LoadItems(items, properties);
```

## Filter Pane Model

The `FilterPaneModel` provides the data structure for building filter pane UIs:

```csharp
var categories = controller.FilterPaneModel!.GetCategories(controller.Items);
foreach (var category in categories)
{
    Console.WriteLine($"{category.Property.DisplayName}:");
    foreach (var kv in category.ValueCounts!)
        Console.WriteLine($"  {kv.Key}: {kv.Value} items");
}

// Toggle a filter
controller.FilterPaneModel.ToggleStringFilter("Manufacturer", "BMW");
```

## Detail Pane Model

The `DetailPaneModel` provides facet display data for selected items:

```csharp
controller.SelectedItem = someItem;

foreach (var facet in controller.DetailPane.FacetValues)
    Console.WriteLine($"{facet.DisplayName}: {string.Join(", ", facet.Values)}");
```

## Layout Transitions

When the layout changes (filtering, resizing, view switching), items smoothly animate from their old positions to new positions using cubic easing:

```csharp
// In your render loop:
bool needsRedraw = controller.Update(deltaTime);
var bounds = controller.GetItemBounds(item);
// bounds.X, bounds.Y, bounds.Width, bounds.Height are interpolated during transition
```

## API Reference (Silverlight SL5 Compatibility)

### Enums (exact Silverlight values)

```csharp
[Flags]
enum PivotViewerPropertyOptions {
    None = 0, Private = 1, CanFilter = 2, CanSearchText = 4, WrappingText = 8
}

enum PivotViewerPropertyType { DateTime, Decimal, Text, Link }
enum CxmlCollectionState { Initialized, Loading, Loaded, Failed }
```

### Key Differences from Silverlight

1. **No LoadCollection()** — that was SL4 only. Use `CxmlCollectionSource` bridge or direct binding.
2. **`PivotViewerHyperlink.Text`** not `DisplayName` (matching SL5 DLL).
3. **`CommandsRequested`** is on the adorner, not the main control.
4. **Added**: `GetValues<T>()`, `TryGetSingleValue<T>()`, `GetPropertyValue()` typed accessors on items.
5. **Added**: Async-first loading with `CancellationToken`.
6. **Added**: `IDisposable` throughout for proper cleanup.
7. **Added**: `PivotViewerCollectionBuilder` for programmatic collection creation.

### Filter Property (State Persistence)

The `Filter` property on `PivotViewerController` provides a serialized state string — exactly matching Silverlight's `PivotViewer.Filter` property:

```csharp
// Save current viewer state
string state = controller.Filter;
// e.g. "Manufacturer=BMW&$view=graph&$sort=Year"

// Restore state (or use SerializeViewerState/SetViewerState for explicit methods)
controller.Filter = state;

// Clear all filters
controller.Filter = "";
```

### Item Templates

Zoom-based template selection matches Silverlight's `PivotViewerItemTemplateCollection`:

```csharp
controller.ItemTemplates.Add(new PivotViewerItemTemplate { MaxWidth = 100, RenderAction = RenderSmall });
controller.ItemTemplates.Add(new PivotViewerItemTemplate { MaxWidth = 300, RenderAction = RenderMedium });
controller.ItemTemplates.Add(new PivotViewerItemTemplate { MaxWidth = 600, RenderAction = RenderLarge });

// Select the best template for a given rendered size
var template = controller.ItemTemplates.SelectTemplate(itemWidth);
template?.RenderAction?.Invoke(canvas, item, rect);
```

## Zoom and Pan

The grid view supports interactive zoom (0.0 = fit all items, 1.0 = single item detail) and panning:

```csharp
// Programmatic zoom
controller.ZoomLevel = 0.5;  // Half-zoom: fewer, larger items

// Zoom about a point (pinch-to-zoom)
controller.ZoomAbout(1.5, screenX, screenY);  // Keeps focal point stable

// Pan (scroll through zoomed content)
controller.Pan(deltaX, deltaY);
```

The MAUI view handles pinch and pan gestures automatically.

## Item Images from DZC

When your CXML references a DZC collection (`ImgBase="collection.dzc"` and items with `Img="#N"`), you can load thumbnails from the DZC composite tiles:

```csharp
var dzc = DzcTileSource.Parse(dzcXml);
var fetcher = new HttpTileFetcher(httpClient);
var imageProvider = new CollectionImageProvider(dzc, fetcher, "collection_files");

// Load thumbnails for visible items
await imageProvider.LoadThumbnailsAsync(controller.InScopeItems);

// Wire into the controller
controller.ImageProvider = imageProvider;

// Now the MAUI view will render actual thumbnails instead of colored rectangles
```

## Learn More

- [Silverlight PivotViewer Documentation](https://learn.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/hh390416) — Microsoft Learn (archived)
- [CXML & DeepZoom Schemas](../../resources/schemas/) — XSD schemas for CXML and DZI/DZC formats
- [Test Collections](../../resources/collections/) — Sample CXML collections with DeepZoom tiles

## Events

The controller fires events for major state changes:

| Event | Fired When |
|---|---|
| `CollectionChanged` | Items or properties replaced |
| `SelectionChanged` | `SelectedItem` changes |
| `FilterChanged` | Any filter added/removed/cleared |
| `SortPropertyChanged` | Sort property changes |
| `ViewChanged` | Grid ↔ Graph view switch |

```csharp
controller.FilterChanged += (s, e) => {
    // Update filter pane badge count
    int active = controller.FilterEngine.Predicates.Count;
    badge.Text = active > 0 ? $"({active})" : "";
};
```

## Other UI Framework Integration

The core library works with any SkiaSharp rendering surface. Here's an example with `SKCanvasView` (works in Blazor, WPF, WinForms, etc.):

```csharp
// Any platform with SkiaSharp rendering:
var controller = new PivotViewerController();
controller.LoadCollection(source);
controller.SetAvailableSize(width, height);

void OnPaintSurface(SKCanvas canvas)
{
    canvas.Clear(SKColors.White);
    foreach (var pos in controller.GridLayout!.Positions)
    {
        canvas.DrawRect(
            (float)pos.X, (float)pos.Y,
            (float)pos.Width, (float)pos.Height, itemPaint);
    }
}
```

## Headless Testing

The core library is fully testable without any UI framework:

```csharp
var controller = new PivotViewerController();
controller.LoadCollection(CxmlCollectionSource.Parse(xml));
controller.SetAvailableSize(1024, 768);

// Filter
controller.FilterEngine.AddStringFilter("Type", "Sports");
Assert.True(controller.InScopeItems.Count < controller.Items.Count);

// Layout
Assert.NotNull(controller.GridLayout);
Assert.True(controller.GridLayout.Positions.Length > 0);

// Render to bitmap
using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
foreach (var pos in controller.GridLayout.Positions)
    surface.Canvas.DrawRect((float)pos.X, (float)pos.Y,
        (float)pos.Width, (float)pos.Height, paint);
```
