local function gBit(Bit, Start, End)
	if End then
		local Res = (Bit / 2 ^ (Start - 1)) % 2 ^ ((End - 1) - (Start - 1) + 1);
		return Res - Res % 1;
	else
		local Plc = 2 ^ (Start - 1);
        return (Bit % (Plc + Plc) >= Plc) and 1 or 0;
	end;
end;

local Pos = 1
local bytes = "NbX9Čb@"

local function gBits32()
    local W, X, Y, Z = string.byte(bytes, Pos, Pos + 3);

    Pos	= Pos + 4;
    return (Z*16777216) + (Y*65536) + (X*256) + W;
end;

local function gFloat()
	local Left = gBits32();
	local Right = gBits32();
	local IsNormal = 1;
	local Mantissa = (gBit(Right, 1, 20) * (2 ^ 32))
					+ Left;
	local Exponent = gBit(Right, 21, 31);
	local Sign = ((-1) ^ gBit(Right, 32));
	if (Exponent == 0) then
		if (Mantissa == 0) then
			return Sign * 0; -- +-0
		else
			Exponent = 1;
			IsNormal = 0;
		end;
	elseif (Exponent == 2047) then
        return (Mantissa == 0) and (Sign * (1 / 0)) or (Sign * (0 / 0));
	end;
	return math.ldexp(Sign, Exponent - 1023) * (IsNormal + (Mantissa / (2 ^ 52)));
end;

print(gFloat())