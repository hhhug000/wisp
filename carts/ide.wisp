wisp cartridge
version 1
__code__
local activetab = "code"
local samplecode = "-- Welcome to wisp!"
local status = "ready."

function _init()
end

function _update()
    
end

function _draw()
    cls(1)
    
    rectfill(0, 0, 128, 7, 8)
    print("code", 2, 1, activetab == "code" and 7 or 2)
    print("spr", 30, 1, activetab == "spr" and 7 or 2)
    print("editor", 96, 1, 6)
    
    if activetab == "code" then
        print(samplecode, 2, 12, 6)
    elseif activetab == "spr" then
        print("sprite stuff goes here", 2, 12, 6)
    end
    
    rectfill(0, 121, 128, 7, 8)
    print(status, 2, 122, 7)
    
    local mx = mousex()
    local my = mousey()
    if mx and my then
        spr(0, mx, my)
    end
end
__sprites__
00..............................................................................................................................
060.............................................................................................................................
0660............................................................................................................................
0000............................................................................................................................
..0.............................................................................................................................
__sfx__
__music__