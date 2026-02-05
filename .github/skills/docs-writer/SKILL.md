---
name: docs-writer
description: Guide for writing and improving documentation for SkiaSharp.Extended. Use this skill when creating, updating, or reviewing documentation pages, writing code examples for docs, generating screenshots for documentation, or improving existing docs.
---

# Documentation Writer

This skill provides guidance for writing effective documentation for SkiaSharp.Extended.

## Documentation Principles

### Goals
- Help **new developers discover** features
- Help **experienced developers find** advanced features and customization
- Be **helpful, consistent, and informative** - not verbose
- This is a **community docs page** - impress and help users

### Style Guidelines
- **Friendly feature names** in TOC and titles (e.g., "Blur Hash" not "SKBlurHash")
- **Conceptual first** - explain why/when, then how
- **Conversational tone** - write as if explaining to a colleague
- **Concise code examples** - show primary overloads, not all variants
- **Research origins** - credit creators, link to official resources
- **Strategic visuals** - screenshots that add clarity, not decoration
- **Second person** - use "you can..." not "users can..."

### What Makes Good Docs
- Explain the **problem it solves** before showing how to use it
- Show **before/after comparisons** where applicable
- Include **real-world usage patterns** (API responses, MAUI bindings)
- Provide **customization options** with visual comparisons when helpful
- Link to **authoritative sources** for external technologies

### API Documentation
- **Link to generated API docs** using xref syntax: `[API Reference](xref:SkiaSharp.Extended.SKBlurHash)`
- **Include summary tables** only when they reduce redirects and add clarity
- **Don't reproduce** the full API docs in conceptual pages

### Images
- Keep existing images; use descriptive names for new ones (e.g., `components-comparison.png`)
- Store in `docs/images/extended/<feature>/` or `docs/images/ui/controls/<control>/`
- Use PNGs for UI screenshots, can use GIFs for animations

## Page Structure

Each documentation page follows this pattern:

```markdown
# Feature Name

Brief intro - what it is, the problem it solves (1-2 sentences)

[Hero image or comparison - e.g., before/after, original vs processed]

## Quick Start
- Encode/create: one code example
- Decode/use: one code example

## How It Works
- Brief explanation of underlying concepts
- Credit original creators if external technology
- Visual showing key concept if helpful

## Customization
- Key parameters and options with explanations
- Tables for parameters when helpful (reduces need to check API docs)
- Visual comparisons showing effect of different parameter values

## Usage Patterns
- Real-world integration examples (API responses, data models)
- MAUI binding/converter examples where applicable
- Brief XAML usage example

## Learn More
- Link to official site (if external technology like BlurHash, Lottie)
- Link to GitHub/source repository
- Link to origin story or detailed explanations
- Link to API reference using xref
```

### Example: External Technology Attribution
For features based on external technologies (BlurHash, Lottie, etc.):

```markdown
## Learn More

- [BlurHash.sh](https://blurha.sh/) — Official demo and explanation
- [BlurHash GitHub](https://github.com/woltapp/blurhash) — Original algorithm by Dag Ågren at Wolt
- [How Wolt created BlurHash](https://careers.wolt.com/en/blog/tech/...) — The origin story
- [API Reference](xref:SkiaSharp.Extended.SKBlurHash) — Full method documentation
```

### Example: Parameter Table
When parameters need explanation:

```markdown
| Components | String Length | Best For |
| :--------: | :-----------: | :------- |
| 2x2 | ~12 chars | Simple gradients |
| 4x3 | ~28 chars | Most images (recommended) |
| 6x6 | ~76 chars | Complex images with fine detail |
```

## Screenshot Generation

### Base Library Features
Generate screenshots using a temporary console app in `/tmp/`:

```bash
mkdir -p /tmp/docs-screenshots
cd /tmp/docs-screenshots
```

Create a minimal .csproj:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="/path/to/source/SkiaSharp.Extended/SkiaSharp.Extended.csproj" />
  </ItemGroup>
</Project>
```

Write Program.cs to generate comparison images, then run:
```bash
dotnet run
```

### Screenshot Best Practices
- Generate side-by-side comparisons (e.g., different parameter values)
- Add labels to images using SKPaint/SKFont
- Use white background for consistency
- Cell size around 200x200 pixels works well
- Save directly to `docs/images/extended/<feature>/`

### Example: Comparison Image Generator
```csharp
using SkiaSharp;

var configs = new[] { (2, 2), (4, 3), (6, 6) };
var cellWidth = 200;
var padding = 10;

using var surface = SKSurface.Create(new SKImageInfo(totalWidth, totalHeight));
var canvas = surface.Canvas;
canvas.Clear(SKColors.White);

// Draw each variant side-by-side with labels
for (int i = 0; i < configs.Length; i++)
{
    // Generate image for this config
    // Draw to canvas at position
    // Add label below
}

using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
File.WriteAllBytes("comparison.png", data.ToArray());
```

### MAUI Controls
Discuss approach case-by-case - may require running on actual platforms. Existing GIFs in the repo can be kept.

## MAUI Integration Examples

**Always test MAUI code in a real project before including in docs.**

### Test Project Setup
```bash
mkdir -p /tmp/maui-test
cd /tmp/maui-test
dotnet new maui -n TestApp
cd TestApp
dotnet add package SkiaSharp.Views.Maui.Controls
dotnet add reference /path/to/source/SkiaSharp.Extended/SkiaSharp.Extended.csproj
dotnet build -f net9.0-maccatalyst  # or net9.0-android
```

### SKBitmap to ImageSource
Use the implicit conversion to `SKBitmapImageSource` (cross-platform, not Windows-specific):

```csharp
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

SKBitmap bitmap = /* ... */;
ImageSource source = (SKBitmapImageSource)bitmap;
```

**Do NOT use** `ToWriteableBitmap()` - that's Windows-only.

### Value Converter Pattern
Include configurable properties for flexibility:

```csharp
using System.Globalization;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Maui.Controls;

public class BlurHashConverter : IValueConverter
{
    public int Width { get; set; } = 32;
    public int Height { get; set; } = 32;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hash && !string.IsNullOrEmpty(hash))
        {
            var bitmap = SKBlurHash.DeserializeBitmap(hash, Width, Height);
            return (SKBitmapImageSource)bitmap;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

### XAML Usage
Always show how to use the converter in XAML:

```xml
<ContentPage.Resources>
    <local:BlurHashConverter x:Key="BlurHashConverter" Width="32" Height="32" />
</ContentPage.Resources>

<Image Source="{Binding BlurHash, Converter={StaticResource BlurHashConverter}}" />
```

## Building and Previewing Docs

```bash
cd docs
docfx build                    # Build docs
docfx serve _site --port 8080  # Serve locally
docfx --serve                  # Build and serve in one command
```

Verify pages render correctly:
```bash
curl -s http://localhost:8080/docs/blurhash.html | grep -o '<h1[^>]*>.*</h1>'
```

## TOC Structure

The docs TOC uses friendly names with proper nesting:

```yaml
items:
- name: Overview
  href: ../index.md

- name: SkiaSharp.Extended
  items:
  - name: Blur Hash
    href: blurhash.md
  - name: Geometry Helpers
    href: skgeometry.md
  - name: Path Interpolation
    href: skpathinterpolation.md

- name: SkiaSharp.Extended.UI.Maui
  items:
  - name: Confetti Effects
    href: skconfettiview.md
  - name: Lottie Animations
    href: sklottieview.md

- name: Migration Guides
  items:
  - name: SVG Migration
    href: svg-migration.md
```

## File Naming

When renaming doc files for friendlier URLs:
- `skblurhash.md` → `blurhash.md`
- `skgeometry.md` → `geometry.md` (if renaming)
- Update TOC href references accordingly

## Research Checklist

When documenting external technologies:
1. Who invented/created it?
2. Is there an official website or demo?
3. Is there a GitHub repository?
4. Is there an origin story or blog post?
5. What problem does it solve?
6. Are there alternatives? (mention if relevant)

## Quality Checklist

Before finalizing a doc page:
- [ ] Friendly title (not type name)
- [ ] Brief intro explaining the value proposition
- [ ] Hero image or comparison
- [ ] Quick start with working code
- [ ] Customization options explained
- [ ] MAUI integration example (if applicable)
- [ ] Learn More links (official resources + API reference)
- [ ] Build docs and verify no warnings
- [ ] Check rendered page in browser
