export function capturePointer(element, pointerId) {
    if (!element || !element.setPointerCapture) {
        return;
    }

    try {
        element.setPointerCapture(pointerId);
    } catch {
        // The pointer may already have been released before interop completed.
    }
}
