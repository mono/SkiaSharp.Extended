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
<pivotviewer:SKPivotViewerView x:Name="pvView" AccentColor="CornflowerBlue" />
```

```csharp
var source = CxmlCollectionSource.Parse(cxmlXml);
pvView.LoadCollection(source);
pvView.SelectionChanged += (s, e) => ShowDetails(e.NewItem);
```

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
├── CxmlCollectionSource   — CXML parser with type mapping
├── PivotViewerItem        — Item with string indexer + typed accessors
├── PivotViewerProperty    — Property hierarchy (String, Numeric, DateTime, Link)
├── FilterEngine           — AND/OR filtering + in-scope counts + histograms
├── GridLayoutEngine       — Grid + histogram layout computation
├── WordWheelIndex         — Prefix search with CharBucket grouping
├── ViewerStateSerializer  — URL-safe state round-trip
└── PivotViewerController  — Orchestrator: filter → sort → layout → state

SkiaSharp.Extended.UI.Maui.PivotViewer (MAUI)
└── SKPivotViewerView      — ContentView with grid/graph rendering + gestures
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
4. **Added**: `GetValues<T>()`, `TryGetSingleValue<T>()` typed accessors on items.
5. **Added**: Async-first loading with `CancellationToken`.
6. **Added**: `IDisposable` throughout for proper cleanup.

## Learn More

- [Silverlight PivotViewer Documentation](https://learn.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/hh390416) — Microsoft Learn (archived)
- [OpenLink HTML5 PivotViewer](https://github.com/openlink/html5pivotviewer) — Open-source JavaScript implementation
- [CXML Schema](https://github.com/mattleibow/PivotViewPlayground/tree/main/resources/schemas) — XSD schemas for CXML format
