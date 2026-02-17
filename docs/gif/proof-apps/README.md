# GIF Proof Applications

This directory contains small, focused test applications that demonstrate specific behaviors of the reference GIF libraries (giflib, libnsgif, cgif).

## Purpose

When the GIF specification and reference implementations disagree on a specific behavior, we create proof applications to:

1. **Observe actual behavior**: See what each library actually does with a specific test case
2. **Collect evidence**: Generate data for the compatibility decision library
3. **Validate decisions**: Confirm our implementation matches the majority behavior
4. **Regression testing**: Ensure reference behaviors remain consistent

## Directory Structure

```
proof-apps/
├── README.md (this file)
├── [feature-name]/
│   ├── test-giflib.c        # Test using giflib
│   ├── test-libnsgif.c      # Test using libnsgif
│   ├── test-cgif.c          # Test using cgif
│   ├── Makefile             # Build all three tests
│   ├── test-input.gif       # Input file (if needed)
│   └── README.md            # What this test proves
```

## Creating a Proof App

1. **Identify disagreement**: Find a specific feature where spec and/or references differ
2. **Create directory**: `mkdir proof-apps/[feature-name]`
3. **Write test programs**: Create minimal C programs for each reference library
4. **Test case**: Focus on ONE specific behavior
5. **Observable output**: Print/save results that can be compared
6. **Document**: Create README explaining what's being tested

## Example Test Template

### test-giflib.c
```c
#include <gif_lib.h>
#include <stdio.h>

int main() {
    // Minimal test demonstrating specific behavior
    // Print observable results
    return 0;
}
```

### test-libnsgif.c
```c
#include <libnsgif.h>
#include <stdio.h>

int main() {
    // Same test using libnsgif API
    // Print comparable results
    return 0;
}
```

### test-cgif.c
```c
#include <cgif.h>
#include <stdio.h>

int main() {
    // Same test using cgif API
    // Print comparable results
    return 0;
}
```

## Makefile Template

```makefile
CC=gcc
CFLAGS=-Wall -Wextra -O2

all: test-giflib test-libnsgif test-cgif

test-giflib: test-giflib.c
	$(CC) $(CFLAGS) -o test-giflib test-giflib.c -lgif

test-libnsgif: test-libnsgif.c
	$(CC) $(CFLAGS) -o test-libnsgif test-libnsgif.c -lnsgif

test-cgif: test-cgif.c
	$(CC) $(CFLAGS) -o test-cgif test-cgif.c -lcgif

clean:
	rm -f test-giflib test-libnsgif test-cgif

run-all: all
	@echo "=== giflib ==="
	./test-giflib
	@echo "\n=== libnsgif ==="
	./test-libnsgif
	@echo "\n=== cgif ==="
	./test-cgif
```

## Running Tests

```bash
cd proof-apps/[feature-name]
make run-all
```

This will build and run all three tests, showing their outputs side-by-side for comparison.

## Test Naming Convention

- **Feature-based**: Name tests after the GIF feature being tested
  - `lzw-early-change/` - Tests LZW early change behavior
  - `disposal-method-4/` - Tests undefined disposal method handling
  - `transparency-with-local-palette/` - Tests transparency with local color tables

- **Specific**: Each test should verify ONE specific behavior
- **Reproducible**: Tests should produce consistent, deterministic output

## Recording Results

After running a proof app:

1. Record output in the decision library
2. Update the disagreement matrix
3. Reference the proof app in the documentation
4. Commit test files to repository

## Dependencies

To build these tests, you'll need:

```bash
# Ubuntu/Debian
sudo apt-get install libgif-dev libnsgif-dev libcgif-dev

# macOS
brew install giflib libnsgif cgif
```

## Example Proof Apps

*(Examples will be added as disagreements are discovered)*

- TBD: Will be populated during implementation

## Notes

- Keep tests minimal - focus on one behavior
- Print clear, comparable output
- Use the same test input across all three libraries
- Document expected vs. actual behavior
- Tests should compile and run independently
