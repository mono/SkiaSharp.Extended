// SKTouchInterop.ts
// Handles pointer events on the SkiaSharp canvas and forwards them to .NET
// using the same event model as the MAUI SKTouchEventArgs API.
let currentElement = null;
let currentDotNetRef = null;
function findElement(touchId) {
    return document.querySelector(`[data-sk-touch-id="${touchId}"]`);
}
export function initializeTouchEvents(touchId, dotNetRef) {
    const element = findElement(touchId);
    if (!element)
        return;
    currentElement = element;
    currentDotNetRef = dotNetRef;
    element.style.touchAction = "none";
    element.style.userSelect = "none";
    element.addEventListener("pointerdown", onPointerDown);
    element.addEventListener("pointermove", onPointerMove);
    element.addEventListener("pointerup", onPointerUp);
    element.addEventListener("pointercancel", onPointerCancel);
    element.addEventListener("pointerenter", onPointerEnter);
    element.addEventListener("pointerleave", onPointerLeave);
    element.addEventListener("wheel", onWheel);
}
export function disposeTouchEvents(touchId) {
    const element = findElement(touchId);
    if (!element)
        return;
    element.style.touchAction = "";
    element.style.userSelect = "";
    element.removeEventListener("pointerdown", onPointerDown);
    element.removeEventListener("pointermove", onPointerMove);
    element.removeEventListener("pointerup", onPointerUp);
    element.removeEventListener("pointercancel", onPointerCancel);
    element.removeEventListener("pointerenter", onPointerEnter);
    element.removeEventListener("pointerleave", onPointerLeave);
    element.removeEventListener("wheel", onWheel);
    currentElement = null;
    currentDotNetRef = null;
}
function onPointerDown(e) {
    sendPointerEvent(e, 2 /* SKTouchAction.Pressed */);
    try {
        e.currentTarget.setPointerCapture(e.pointerId);
    }
    catch { /* ignore */ }
}
function onPointerMove(e) {
    sendPointerEvent(e, 3 /* SKTouchAction.Moved */);
}
function onPointerUp(e) {
    sendPointerEvent(e, 4 /* SKTouchAction.Released */);
}
function onPointerCancel(e) {
    sendPointerEvent(e, 0 /* SKTouchAction.Cancelled */);
}
function onPointerEnter(e) {
    sendPointerEvent(e, 1 /* SKTouchAction.Entered */);
}
function onPointerLeave(e) {
    sendPointerEvent(e, 5 /* SKTouchAction.Exited */);
}
function onWheel(e) {
    if (!currentDotNetRef)
        return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const delta = e.deltaMode === 0
        ? Math.round(-e.deltaY / 10)
        : (e.deltaY < 0 ? 1 : -1);
    currentDotNetRef.invokeMethodAsync("OnPointerEvent", {
        id: -1,
        action: 6 /* SKTouchAction.WheelChanged */,
        deviceType: 1 /* SKTouchDeviceType.Mouse */,
        x,
        y,
        pressure: 0,
        inContact: false,
        wheelDelta: delta,
    });
    e.preventDefault();
}
function getDeviceType(pointerType) {
    switch (pointerType) {
        case "mouse": return 1 /* SKTouchDeviceType.Mouse */;
        case "pen": return 2 /* SKTouchDeviceType.Stylus */;
        default: return 0 /* SKTouchDeviceType.Touch */;
    }
}
function sendPointerEvent(e, action) {
    if (!currentDotNetRef)
        return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const deviceType = getDeviceType(e.pointerType);
    const inContact = e.buttons !== 0 || action === 2 /* SKTouchAction.Pressed */;
    currentDotNetRef.invokeMethodAsync("OnPointerEvent", {
        id: e.pointerId,
        action,
        deviceType,
        x,
        y,
        pressure: e.pressure,
        inContact,
    });
}
