// SKTouchInterop.ts
// Handles pointer events on the SkiaSharp canvas and forwards them to .NET
// using the same event model as the MAUI SKTouchEventArgs API.

interface DotNetObjectReference {
    invokeMethodAsync(methodName: string, args: PointerEventPayload): void;
}

interface PointerEventPayload {
    id: number;
    action: number;
    deviceType: number;
    x: number;
    y: number;
    pressure: number;
    inContact: boolean;
    wheelDelta?: number;
}

const enum SKTouchAction {
    Cancelled = 0,
    Entered = 1,
    Pressed = 2,
    Moved = 3,
    Released = 4,
    Exited = 5,
    WheelChanged = 6,
}

const enum SKTouchDeviceType {
    Touch = 0,
    Mouse = 1,
    Stylus = 2,
}

let currentElement: HTMLElement | null = null;
let currentDotNetRef: DotNetObjectReference | null = null;

export function initializeTouchEvents(element: HTMLElement, dotNetRef: DotNetObjectReference): void {
    if (!element) return;

    currentElement = element;
    currentDotNetRef = dotNetRef;

    element.addEventListener("pointerdown", onPointerDown);
    element.addEventListener("pointermove", onPointerMove);
    element.addEventListener("pointerup", onPointerUp);
    element.addEventListener("pointercancel", onPointerCancel);
    element.addEventListener("pointerenter", onPointerEnter);
    element.addEventListener("pointerleave", onPointerLeave);
    element.addEventListener("wheel", onWheel);
}

export function disposeTouchEvents(element: HTMLElement): void {
    if (!element) return;

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

function onPointerDown(e: PointerEvent): void {
    sendPointerEvent(e, SKTouchAction.Pressed);
    try { (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId); } catch { /* ignore */ }
}

function onPointerMove(e: PointerEvent): void {
    sendPointerEvent(e, SKTouchAction.Moved);
}

function onPointerUp(e: PointerEvent): void {
    sendPointerEvent(e, SKTouchAction.Released);
}

function onPointerCancel(e: PointerEvent): void {
    sendPointerEvent(e, SKTouchAction.Cancelled);
}

function onPointerEnter(e: PointerEvent): void {
    sendPointerEvent(e, SKTouchAction.Entered);
}

function onPointerLeave(e: PointerEvent): void {
    sendPointerEvent(e, SKTouchAction.Exited);
}

function onWheel(e: WheelEvent): void {
    if (!currentDotNetRef) return;

    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    const delta = e.deltaMode === 0
        ? Math.round(-e.deltaY / 10)
        : (e.deltaY < 0 ? 1 : -1);

    currentDotNetRef.invokeMethodAsync("OnPointerEvent", {
        id: -1,
        action: SKTouchAction.WheelChanged,
        deviceType: SKTouchDeviceType.Mouse,
        x,
        y,
        pressure: 0,
        inContact: false,
        wheelDelta: delta,
    });

    e.preventDefault();
}

function getDeviceType(pointerType: string): SKTouchDeviceType {
    switch (pointerType) {
        case "mouse": return SKTouchDeviceType.Mouse;
        case "pen": return SKTouchDeviceType.Stylus;
        default: return SKTouchDeviceType.Touch;
    }
}

function sendPointerEvent(e: PointerEvent, action: SKTouchAction): void {
    if (!currentDotNetRef) return;

    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const deviceType = getDeviceType(e.pointerType);
    const inContact = e.buttons !== 0 || action === SKTouchAction.Pressed;

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
