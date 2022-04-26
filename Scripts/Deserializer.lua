local function b64_decode(data)
    local b='ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

    data = string.gsub(data, '[^'..b..'=]', '')
    return (data:gsub('.', function(x)
        if (x == '=') then return '' end
        local r,f='',(b:find(x)-1)
        for i=6,1,-1 do r=r..(f%2^i-f%2^(i-1)>0 and '1' or '0') end
        return r;
    end):gsub('%d%d%d?%d?%d?%d?%d?%d?', function(x)
        if (#x ~= 8) then return '' end
        local c=0
        for i=1,8 do c=c+(x:sub(i,i)=='1' and 2^(8-i) or 0) end
            return string.char(c)
    end))
end
local function num2bin(n, places)
    local binNum = ""
    if n ~= 0 then
        while n >= 1 do
            if n %2 == 0 then
                binNum = binNum .. "0"
                n = n / 2
            else
                binNum = binNum .. "1"
                n = (n-1)/2
            end
        end
    else
        binNum = "0"
    end

    for i = #binNum, places - 1 do
        binNum = binNum .. "0"
    end
  
    return string.reverse(binNum)
end

local bitlib = bit32 or bit or nil
local modm = math.pow(2,32)-1

local band = (bitlib and bitlib.band) or function(a,b)
    local r,c=0,1
    while a>0 and b>0 do
        if a%2==1 and b%2==1 then r=r+c end
        c=c*2
        a=math.floor(a/2)
        c=math.floor(c/2)
    end 
    return r
end
local bor = (bitlib and bitlib.bor) or function(a,b)
    return modm-band(modm-a,modm-b)
end
local bxor = (bitlib and bitlib.bxor) or function(a,b)
    local c,e=1,0
    while a>0 and b>0 do
        local f,g=a%2,b%2
        if f~=g then e=e+c end
        a,b,c=(a-f)/2,(b-g)/2,c*2
    end
    if a<b then a=b end
    while a>0 do
        local f=a%2
        if f>0 then e=e+c end
        a,c=(a-f)/2,c*2
    end
    return e
end
local lshift = (bitlib and bitlib.lshift) or function(a,b)
    return a*math.pow(2,b)
end
local rshift = (bitlib and bitlib.rshift) or function(a,b)
    return math.floor(a/math.pow(2,b))
end

local test2 = "AFADtgAZAMMACALjABEAqQASAIMBkQR6AOYA/gD00xMBQQBVCW4AugB8AHUAk1feAZ0BLEkVAHYAzQBUAGju0glvIHGH8f+l/1T/Cf9/8RcGlwAIQ+cAOQAcAKcA2wDBAgIAlQB+AKYEEgUIANsA2gAicP5yVGmwbup07gTfBbgAGgBTABdL32/oa/FvenM7"
local test3 = "AIADSQCrAEUAYAJWADcAagB6ALwAIgILADcAkgAABLkFLgC+ANkALHA7codpb26kdCoEeAW/AMcAugD4S2xv4GvgbxRzVAErBK8AEwBQALAJSgCwADUA0ABlACHPMwWnCVgBEwGbAKwAQwDIGpGACgdEAN8C5wDiAZgAb6QKC4sDlwCSAX0AuuWOHso="

local bytecode = b64_decode(test3)
local pos = 1

local function GetByte()
    local l = string.byte(string.sub(bytecode, pos, pos))
    pos = pos + 1
    return l
end
local function ReadByte()
    local b1 = num2bin(GetByte(), 8)
    local data = num2bin(GetByte(), 8)
    local xorBin = ""

    for i = 1, #data do
        local bb = (string.sub(b1, i, i) == "1" and true or false)
        local db = (string.sub(data, i, i) == "1" and true or false)

        if bb == true then
            xorBin = xorBin .. ((not db) == true and "1" or "0")
        else
            xorBin = xorBin .. (db == true and "1" or "0")
        end
    end

    return bxor(tonumber(data, 2), tonumber(xorBin, 2))
end
local function ReadBool()
    return ReadByte() == 1
end
local function ReadInt16()
    local X, Y = ReadByte(), ReadByte()

    return (Y * math.pow(2, 8)) + X
end
local function ReadInt32()
    local X, Y, Z, W = ReadByte(), ReadByte(), ReadByte(), ReadByte()

    return (W * math.pow(2,24)) + (Z * math.pow(2,16)) + (Y * math.pow(2,8)) + X
end
local function ReadInt64()
    local X,Y,Z,W = ReadByte(), ReadByte(), ReadByte(), ReadByte()
    local X1,Y1,Z1,W1 = ReadByte(), ReadByte(), ReadByte(), ReadByte()

    local p1, p2 = 0, 0
    p1 = (W1 * math.pow(2,56)) + (Z1 * math.pow(2,48)) + (Y1 * math.pow(2,40)) + (X1 * math.pow(2, 32))
    p2 = (W * math.pow(2, 24)) + (Z * math.pow(2,16)) + (Y * math.pow(2,8)) + X

    return p1 + p2
end
local function ReadDouble()
    local full = ReadInt64()
    local sign = band(full, 1) == 1
    local num = rshift(full, 1)

    if sign == true then num = -num end

    return tonumber(tostring(num) .. "." .. tostring(ReadInt64()))
end
local function ReadString()
    local function concat(tbl)
        local r = ""
        for i = 1,#tbl do
            r = r .. tostring(tbl[i])
        end
        return r
    end

    local len = ReadInt32()

    if len == 0 then
        return ""
    end

    local tbl = {}
    for i = 1, len do
        tbl[i] = string.char(ReadByte())
    end

    return concat(tbl)
end

local function Deserialize()
    local hasDebug = ReadBool()
    local steps = ReadInt32()

    local Chunk = {
        Instructions = {},
        Constants = {},
        Functions = {},
        Debug = {
            UpValues = {},
            Locals = {}
        }
    }

    for i = 1, steps do
        local step = ReadByte()
        if step == 0 then
            local count = ReadInt32()
            for k = 1, count do
                local ttype = ReadByte()
                if ttype == 0 then
                    if ReadString() ~= "NIL" then
                        error("This constant should be nil")
                    end
                    Chunk.Constants[k] = nil
                elseif ttype == 1 then
                    Chunk.Constants[k] = ReadBool()
                elseif ttype == 3 then
                    Chunk.Constants[k] = ReadDouble()
                elseif ttype == 4 then
                    Chunk.Constants[k] = ReadString()
                end
            end
        elseif step == 1 then
            local count = ReadInt32()
            for k = 1, count do
                local instr = {}
                local opmode = num2bin(ReadByte(), 8):reverse()

                if string.sub(opmode, 1, 1) == "1" then
                    instr[1] = ReadByte()
                end
                if string.sub(opmode, 2, 2) == "1" then
                    instr[2] = ReadInt16()
                end
                if string.sub(opmode, 4, 4) == "1" then
                    instr[2] = ReadInt32()
                end
                if string.sub(opmode, 5, 5) == "1" then
                    instr[2] = ReadInt32() - 131072
                end
                if string.sub(opmode, 3, 3) == "1" then
                    instr[3] = ReadInt16()
                end
                if string.sub(opmode, 6, 6) == "1" then
                    instr["raw"] = ReadInt32()
                end

                instr[4] = ReadInt16()

                Chunk.Instructions[k] = instr
            end
        elseif step == 2 then
            local count = ReadInt32()
            for k = 1, count do
                Chunk.Functions[k] = Deserialize()
            end
        elseif step == 3 and hasDebug == true then
        end
    end

    return Chunk
end