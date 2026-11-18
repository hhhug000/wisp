# wisp
A fantasy console written in C#

## Downloading Wisp

Wisp is availible at the [releases page](https://github.com/hhhug000/wisp/releases/) on github.
Download the zip file for your operating system, then extract it.

On windows you can simply open a terminal and run the commands with `.\wisp`, however on linux you will first have to run `chmod +x wisp`, and then run commands with `./wisp`.

## Using Wisp

Wisp is intended to be run from the command line, to open the default example cartridge just run the base command (In all examples I will use the linux command, on windows just make sure to replace `./wisp` with `.\wisp`).

To run a custom cartridge use the command `./wisp path/to/cartridge.wisp`. If the file exists it will be opened, if it does not exist it will load the default example cartridge.

## Making games with Wisp

Wisp does not have a built in editor, so all games must be made in an external text editor.
Wisp cartridges are pretty simple, to make one create a new file called `myfirstcart.wisp` and open it in any text editor.

Start by putting this boilerplate in the file:

```
wisp cartridge
version 1
__code__
function _init()
end

function _update()
end

function _draw()
end
__sprites__

__map__

__sfx__

__music__

```

In each section you can add your code/data.

### \_\_code\_\_

The \_\_code\_\_ section is lua code that is run.

The _init() function will run once on start of the cartridge, _update() will run every frame, and _draw() will run every frame after _update()

For in depth detail on the api, check the [API.md](API.md) file

### \_\_sprites\_\_

The \_\_sprites\_\_ section is a spritesheet, this is rows of pixels separated by newlines.

For transparent, use `.`, for colors, use the colour code in hexadecimal.

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

Each line must be exactly 128 characters/pixels long, however you do not need to fill out all rows.

Each tile in the spritesheet is 8x8, and can be created with the spr function (check api reference)

### \_\_map\_\_

The \_\_map\_\_ section defines the tilemap for the cartridge. It is a grid 128x32 tiles wide and tall.

Each tile on the map is a 2 character hexadecimal id corresponding to a tile from the spritesheet (from 00 to FF).

Because each tile is 2 hex chars, every row must be exactly 256 chars long.

You can read and draw tiles using the map api functions (in the api reference)

### \_\_sfx\_\_

The \_\_sfx\_\_ section holds your sound effects.

Each sound effect has an id from 0 to 63, configuration values, and a sequence of steps separated by spaces or commas.

Each sound effect line takes this format:
`[id] [flag1] [flag2] [speed] [step_1] [step_2] ... [step_32]`

| Parameter | Content |
| --------- | ------- |
| id | Sound effect slot ID from 0 to 63 |
| flag1 / flag2 | Configuration values |
| speed | How many game frames each step lasts |

Each step is written using colon-separated decimal numbers:
`[pitch]:[waveform]:[volume]:[effect]`

* **Pitch:** The numerical note or frequency value.
* **Waveform:** The instrument or wave type.
* **Volume:** The audio volume level for the step.
* **Effect:** Special audio modifiers (such as slides or vibrato).

SFX can be played using api functions

### \_\_music\_\_

The \_\_music\_\_ section arranges your sound effects into background tracks and patterns.

Each music line defines a pattern row using spaces or commas to separate the values:
`[pattern_id] [flags] [ch0_sfx] [ch1_sfx] [ch2_sfx] [ch3_sfx]`

| Parameter | Content |
| --------- | ------- |
| pattern_id | The index of the music pattern row (0 upwards) |
| flags | Playback control flags (eg loop or stop markers) |
| ch0_sfx to ch3_sfx | Sound effect IDs playing concurrently across audio channels 0 through 3 |

Music tracks can be started and stopped using api functions