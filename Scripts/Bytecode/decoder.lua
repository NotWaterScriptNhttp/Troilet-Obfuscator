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
function byte2bin(n)
    local t = {}
    for i=7,0,-1 do
        t[#t+1] = math.floor(n / 2^i)
        n = n % 2^i
    end
    return table.concat(t)
end
function num2bin(n)
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

    for i = #binNum, 31 do
        binNum = binNum .. "0"
    end
  
    return string.reverse(binNum)
end
local band = (bit32 and bit32.band) or function(a, b)
    local result = 0
    local bitval = 1
    while a > 0 and b > 0 do
        if a % 2 == 1 and b % 2 == 1 then
            result = result + bitval
        end
        bitval = bitval * 2
        a = math.floor(a/2)
        b = math.floor(b/2)
    end
    return result
end
local xor = (bit32 and bit32.bxor) or function(a,b)
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
local bor = (bit32 and bit32.bor) or function(a, b)
    local modm = math.pow(2, 32) - 1

    return modm - band(modm - a, modm - b)
end
local lshift = (bit32 and bit32.lshift) or function(x, by)
    return x * 2 ^ by
end
local rshift = (bit32 and bit32.rshift) or function (x, by)
    return math.floor(x / 2 ^ by)
end

local s = b64_decode("LZQB3QByAP0A3wAoAJQAKgFaAbYATAABAHoAEwC3AIA=") -- LBsBbADjANIAQQDPAFsAawFLATYAaQA9AD0AJgAoAFQ=

local pos = 1

local function getbyte()
    local b = string.sub(s, pos, pos)
    pos = pos + 1
    return string.byte(b)
end

function decode_byte()
    local b1 = byte2bin(getbyte())
    local data = byte2bin(getbyte())
    local xorBin = ""

    for i = 1, #data do
        local bb1 = (string.sub(b1, i, i) == "1" and true or false)
        local db = (string.sub(data, i, i) == "1" and true or false)

        if bb1 == true then
            xorBin = xorBin .. ((not db) == true and "1" or "0")
        else
            xorBin = xorBin .. (db == true and "1" or "0")
        end
    end

    return xor(tonumber(data, 2), tonumber(xorBin, 2))
end

local function read_i32()
    local X, Y, Z, W = decode_byte(), decode_byte(), decode_byte(), decode_byte()

    return (W * math.pow(2, 24)) + (Z * math.pow(2, 16)) + (Y * math.pow(2, 8)) + X
end
local function read_i64()
    local X, Y, Z, W, X1, Y1, Z1, W1 = decode_byte(), decode_byte(), decode_byte(), decode_byte(), decode_byte(), decode_byte(), decode_byte(), decode_byte()

    local p1 = (W1 * math.pow(2, 56)) + (Z1 * math.pow(2, 48)) + (Y1 * math.pow(2, 40)) + (X1 * math.pow(2, 32))
    local p2 = (W * math.pow(2, 24)) + (Z * math.pow(2, 16)) + (Y * math.pow(2, 8)) + X

    return p1 + p2
end
local function read_double()
    local fullnum = read_i64()
    local sign = band(fullnum, 1) == 1

    local num = rshift(fullnum, 1)
    if sign == true then num = -num end

    return tonumber(tostring(num) .. "." .. tostring(read_i64()))
end

local function read_string()
    local len = read_i32()

    if len < 0 then
        error("Can't read a string from negative number")
    end

    if len == 0 then
        return ""
    end

    local buffer = {}
    for i = 1, len do
        buffer[i] = string.char(decode_byte())
    end

    return table.concat(buffer)
end

print(read_double() + 0.257)