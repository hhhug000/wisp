# API Reference
This file shows the API reference for wisp code

## Colours

| Colour Code (Base 10) | Colour Code (Hex) | RGBA | Colour Name |
|---|---|--------------|-------|
| 0 | 0 | 0, 0, 0, 255 | Black |
| 1 | 1 | 29, 43, 83, 255 | Dark Blue |
2 | 2 | 126, 37, 83, 255 | Purple |
3 | 3 | 0, 135, 81, 255 | Dark Green |
4 | 4 | 171, 82, 54, 255 | Brown |
5 | 5 | 95, 87, 79, 255 | Grey |
6 | 6 | 194, 195, 199, 255 | Light Grey |
7 | 7 | 255, 241, 232, 255 | White |
8 | 8 | 255, 0, 77, 255 | Red |
9 | 9 | 255, 163, 0, 255 | Orange |
10 | A | 255, 236, 39, 255 | Yellow |
11 | B | 0, 228, 54, 255 | Green |
12 | C | 41, 173, 255, 255 | Blue |

## Lifecycle functions

These functions are defined in your code and called by the wisp engine at set times.

* `_init()` - Called once when the cart starts
* `_update()` - Called once every frame, designed to handle logic, input and other updates
* `_draw()` - Called once every frame immediately after `_update()`, designed to handle rendering

## Graphics

* `cls(colour)` - Clears the screen with the specified decimal colour code (0-15)
* `pset(x, y, color)` Draws a single pixel at coordinates `(x, y)` with the given decimal colour code
* `line(x0 y0, x1, y1, colour)` - Draws a stright line from `(x0, y0)` to `(x1, y1)`
* `rect(x, y, w, h )` - Draws an unfilled rectangle outline at `(x, y)` with width `w` and height `h`
* `rectfill(x, y, w, h, color)` - Draws a filled rectangle at `(x, y)` with width `w` and height `h`
* `circ(x, y, r, color)` - Draws a circle outline centered at `(x, y)` with radius `r`
* `circfill(x, y, r, color)` - Draws a filled circle centered at `(x, y)` with radius `r`
* `print(text, x, y, color)` - Draws a text string at `(x, y)` using the specified color
* `spr(n, x, y, [w], [h], [flipX], [flipY])` - Draws sprite index `n` at screen coordinates `(x, y)`. Optional parameters allow drawing multi-tile sprites (`w` and `h` in tiles, default `1`) and flipping horizontally or vertically (`flipX`, `flipY`)
* `map(tileX, tileY, [sx], [sy], [tileW], [tileH])` - Renders a region of the map starting at map coordinates `(tileX, tileY)` onto screen coordinates `(sx, sy)` with dimensions `(tileW, tileH)` in tiles

## Map

* `mget(x, y)` - Returns the tile ID located at map coordinates `(x, y)`. Returns `0` if out of bounds

## Audio

* `sfx(n)` - Triggers and plays sound effect slot `n` (0–63)
* `music(n)` - Starts background music playback from pattern row `n`. Passing `-1` stops all active music channels

## Input

* `btn(i)` - Returns `true` if button index `i` is currently held down
* `btnp(i)` - Returns `true` only on the frame button index `i` is initially pressed (button press trigger)
    
| Button ID | Button |
| --------- | ------ |
| `0` | Left (Arrow Left / A) |
| `1` | Right (Arrow Right / D) |
| `2` | Up (Arrow Up / W) |
| `3` | Down (Arrow Down / S) |
| `4` | Button Z (Z) |
| `5` | Button X (X) |
| `6` | Left Mouse Click |
| `7` | Right Mouse Click |

* `mousex()` - Returns the integer X coordinate of the mouse cursor on the virtual screen, or `nil` if the cursor is off-screen.
* `mousey()` - Returns the integer Y coordinate of the mouse cursor on the virtual screen, or `nil` if the cursor is off-screen.

## Math and Utilities

* `min(a, b)` — Returns the smaller of two numbers
* `max(a, b)` — Returns the larger of two numbers
* `mid(x, y, z)` — Returns the median value among three numbers
* `cos(angle)` — Returns the cosine of an angle (where `1.0` represents a full circle of $2\pi$ radians)
* `sin(angle)` — Returns the sine of an angle (where `1.0` represents a full circle of $2\pi$ radians)
* `atan2(x, y)` — Returns the angle given `x` and `y` coordinates
* `abs(x)` — Returns the absolute value of `x`
* `sqrt(x)` — Returns the square root of `x`
* `rnd([x])` — Generates a random number. If called with no arguments, returns a float between `0.0` and `1.0`. If passed a number `x`, returns a float from `0` up to `x`. If passed a Lua table, returns a random element from that table
* `srand(seed)` — Seeds the random number generator with the specified integer