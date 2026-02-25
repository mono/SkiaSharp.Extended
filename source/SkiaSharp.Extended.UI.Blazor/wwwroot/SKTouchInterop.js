// SKTouchInterop.js
// Handles pointer events on the SkiaSharp canvas and forwards them to .NET
// using the same event model as the MAUI SKTouchEventArgs API.

export function initializeTouchEvents(element, dotNetRef) {
    if (!element) return;

    // Use pointer events for cross-device support (mouse, touch, stylus)
    element.addEventListener('pointerdown', (e) => onPointerEvent(e, dotNetRef, 2 /* Pressed */));
    element.addEventListener('pointermove', (e) => onPointerEvent(e, dotNetRef, 3 /* Moved */));
    element.addEventListener('pointerup', (e) => onPointerEvent(e, dotNetRef, 4 /* Released */));
    element.addEventListener('pointercancel', (e) => onPointerEvent(e, dotNetRef, 0 /* Cancelled */));
    element.addEventListener('pointerenter', (e) => onPointerEvent(e, dotNetRef, 1 /* Entered */));
    element.addEventListener('pointerleave', (e) => onPointerEvent(e, dotNetRef, 5 /* Exited */));
    element.addEventListener('wheel', (e) => onWheelEvent(e, dotNetRef));

    // Prevent default to avoid page scroll/zoom on touch
    element.style.touchAction = 'none';
    element.style.userSelect = 'none';
}

export function disposeTouchEvents(element) {
    if (!element) return;
    // Cloning removes all listeners – simpler than tracking references
    element.style.touchAction = '';
    element.style.userSelect = '';
}

function getDeviceType(pointerType) {
    switch (pointerType) {
        case 'mouse': return 1;   // Mouse
        case 'pen': return 2;     // Stylus
        default: return 0;        // Touch
    }
}

function getElementRect(element) {
    return element.getBoundingClientRect();
}

function onPointerEvent(e, dotNetRef, action) {
    const rect = getElementRect(e.currentTarget);
    // Convert to element-local coordinates (CSS pixels)
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const deviceType = getDeviceType(e.pointerType);
    const inContact = e.buttons !== 0 || action === 2; // Pressed always in contact

    dotNetRef.invokeMethodAsync('OnPointerEvent', {
        id: e.pointerId,
        action: action,
        deviceType: deviceType,
        x: x,
        y: y,
        pressure: e.pressure,
        inContact: inContact
    });
}

function onWheelEvent(e, dotNetRef) {
    const rect = getElementRect(e.currentTarget);
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    // Normalize deltaY to a small integer similar to MAUI WheelDelta
    // Positive = scroll down, negative = scroll up
    const delta = e.deltaMode === 0
        ? Math.round(-e.deltaY / 10)   // pixels
        : (e.deltaY < 0 ? 1 : -1);    // lines/pages

    dotNetRef.invokeMethodAsync('OnPointerEvent', {
        id: -1,
        action: 6, // WheelChanged
        deviceType: 1, // Mouse
        x: x,
        y: y,
        pressure: 0,
        inContact: false,
        wheelDelta: delta
    });

    e.preventDefault();
}
