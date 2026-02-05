# PR #79 MAUI Port - Progress Tracking Document

## Overview
This document tracks the progress of porting PR #79 (SkiaSharp.Extended.Controls) from Xamarin.Forms to .NET MAUI.

## PR #79 Original Description
> Creating a new project/package for more advanced SkiaSharp views, such as a dynamic (both hardware and software) views and a "infinite" gesture view.

## Files from PR #79 to Port
1. **Core Controls:**
   - `SKDynamicSurfaceView.cs` - View that allows switching between hardware (GL) and software (Canvas) rendering
   - `SKGestureSurfaceView.cs` - View with advanced gesture detection (pan, pinch, fling, tap, etc.)

2. **Event Args:**
   - `SKPaintDynamicSurfaceEventArgs.cs` - Event args for paint surface events
   - `SKFlingDetectedEventArgs.cs` - Fling gesture detection
   - `SKGestureEventArgs.cs` - General gesture events
   - `SKHoverDetectedEventArgs.cs` - Hover detection
   - `SKTapDetectedEventArgs.cs` - Tap detection (single, double, long press)
   - `SKTransformDetectedEventArgs.cs` - Transform events (pan, zoom, rotate)

3. **Helper Classes:**
   - `SKGestureSurfaceView.FlingTracker.cs` - Tracks fling velocities
   - `SKGestureSurfaceView.FlingTrackerEvent.cs` - Event data for fling tracking
   - `SKGestureSurfaceView.PinchValue.cs` - Pinch gesture calculation
   - `SKGestureSurfaceView.TouchEvent.cs` - Touch event data
   - `SKGestureSurfaceView.TouchMode.cs` - Touch mode enumeration

---

## Issues Identified

### Issue #1: Xamarin.Forms to MAUI Migration
**Status:** ✅ Fixed  
**Description:** The original code uses Xamarin.Forms APIs that need to be migrated to .NET MAUI.

**Changes Required:**
- Replace `Xamarin.Forms` namespace with `Microsoft.Maui.Controls`
- Replace `SkiaSharp.Views.Forms` with `SkiaSharp.Views.Maui` and `SkiaSharp.Views.Maui.Controls`
- Update `Device.StartTimer` to `Dispatcher.StartTimer`
- Replace `Rectangle` with `Rect` for layout

### Issue #2: Architecture Mismatch
**Status:** ✅ Fixed  
**Description:** PR #79 uses a `Layout<View>` based approach with dynamic child views. The existing MAUI codebase uses `TemplatedView` with control templates.

**Solution:** Adapt the design to use `TemplatedView` pattern like existing controls (SKSurfaceView, SKAnimatedSurfaceView), but include support for runtime switching between hardware/software rendering.

### Issue #3: Deprecated SKMatrix APIs
**Status:** ✅ Fixed  
**Description:** The gesture surface sample code uses `SKMatrix.MakeIdentity()`, `SKMatrix.MakeTranslation()`, `SKMatrix.MakeScale()`, `SKMatrix.MakeRotationDegrees()`, and `SKMatrix.Concat()` which are deprecated.

**Solution:** Use the new static methods: `SKMatrix.CreateIdentity()`, `SKMatrix.CreateTranslation()`, etc., and use `SKMatrix.Concat(ref result, matrix1, matrix2)` overload.

### Issue #4: Timer/Callback Thread Safety  
**Status:** ✅ Fixed  
**Description:** The `multiTapTimer` in `SKGestureSurfaceView` uses `System.Threading.Timer` which invokes callbacks on a thread pool thread, not the UI thread. This can cause issues when firing UI events.

**Solution:** Use MAUI's `Dispatcher.DispatchAsync` or ensure timer callbacks are dispatched to the UI thread.

### Issue #5: Missing Null Checks
**Status:** ✅ Fixed  
**Description:** Several places in the original code don't check for null references:
- `touches[e.Id]` access without checking if key exists
- `releasedTouch.Location` used after touch might be removed

**Solution:** Add appropriate null checks and use `TryGetValue` pattern.

### Issue #6: Incomplete Boundary Checking Logic
**Status:** 🔍 Noted (commented code)  
**Description:** The `GestureSurfacePage.xaml.cs` sample has extensive commented-out boundary checking code, suggesting the feature is incomplete.

**Decision:** Port the core controls but note this as an area for future improvement.

### Issue #7: Resource Cleanup
**Status:** ✅ Fixed  
**Description:** Event handlers and timers need proper cleanup to prevent memory leaks.

**Solution:** Implement proper disposal pattern and unsubscribe from events when view is unloaded.

### Issue #8: SKPaintDynamicSurfaceEventArgs Info Property Issue
**Status:** ✅ Fixed  
**Description:** When using GL rendering, the `Info` property constructs a new `SKImageInfo` from `BackendRenderTarget.Width/Height`. However, if `glEvent` is null, this could cause issues.

**Solution:** Add proper null checking and use the new MAUI pattern of getting size from event args.

---

## Implementation Progress

### Phase 1: Setup & Research
- [x] Analyzed PR #79 files
- [x] Understood existing MAUI patterns in the repo
- [x] Created progress tracking file
- [x] Created copilot instructions file  
- [x] Created copilot setup workflow

### Phase 2: Code Analysis & Issue Identification
- [x] Identified deprecated APIs
- [x] Identified bugs and logic issues
- [x] Documented all issues

### Phase 3: MAUI Implementation
- [x] Created event arg classes
- [x] Created SKDynamicSurfaceView for MAUI  
- [x] Created SKGestureSurfaceView for MAUI
- [x] Created helper classes (FlingTracker, PinchValue, etc.)
- [x] Created XAML resources for new controls

### Phase 4: Sample Integration
- [ ] Add demo pages for new controls
- [ ] Test gesture and touch functionality (not possible on Linux CI)

### Phase 5: Validation & Documentation
- [x] Build the solution
- [ ] Run tests
- [x] Update documentation

---

## Technical Decisions

### 1. Use TemplatedView Pattern
The existing MAUI controls (SKSurfaceView, SKAnimatedSurfaceView) use `TemplatedView` with XAML control templates. This allows for better theming and customization. We adapted the PR #79 code to follow this pattern.

### 2. Separate Touch Handling
Instead of having the gesture view extend the dynamic surface view, we kept them as separate components to allow users to mix-and-match functionality.

### 3. Event Args Immutability
Made event args classes immutable where possible, with only the `Handled` property being mutable.

### 4. Modern C# Features
Used modern C# features available in .NET 9:
- File-scoped namespaces
- Nullable reference types
- Record types (where appropriate)
- Expression-bodied members

---

## Files Created

1. `source/SkiaSharp.Extended.UI.Maui/Controls/Gestures/` folder:
   - `SKDynamicSurfaceView.shared.cs`
   - `SKDynamicSurfaceViewResources.shared.xaml`
   - `SKDynamicSurfaceViewResources.shared.xaml.cs`
   - `SKGestureSurfaceView.shared.cs`
   - `SKGestureSurfaceView.FlingTracker.shared.cs`
   - `SKGestureSurfaceView.TouchEvent.shared.cs`
   - `SKGestureSurfaceView.PinchValue.shared.cs`
   - `SKPaintDynamicSurfaceEventArgs.shared.cs`
   - `SKFlingDetectedEventArgs.shared.cs`
   - `SKGestureEventArgs.shared.cs`
   - `SKHoverDetectedEventArgs.shared.cs`
   - `SKTapDetectedEventArgs.shared.cs`
   - `SKTransformDetectedEventArgs.shared.cs`
   - `TouchMode.shared.cs`

---

## Testing Notes

- Linux CI can only build for Android, iOS/macOS/Windows are only available on their respective platforms
- Manual testing requires a device or emulator
- Gesture recognition requires actual touch input to validate

---

## References

- [PR #79](https://github.com/mono/SkiaSharp.Extended/pull/79)
- [SkiaSharp MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.views.maui)
- [MAUI Gesture Recognizers](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/)
