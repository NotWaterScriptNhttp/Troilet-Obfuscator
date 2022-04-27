using System.Linq;
using System.Text;

using TroiletCore.RandomStuff;

using LuaLib.Emit;

namespace TroiletCore.VM
{
    internal class CreateVM
    {
        private static string CheckNumber(string str)
        {
            char[] numbers = new char[]
            {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9'
            };

            if (numbers.Contains(str[0]))
                return CheckNumber((char)(str[0] + 60) + str.Substring(1, str.Length - 1));

            return str;
        }

        public static string GetVM(byte[] serialized, ObfuscatorSettings settings)
        {
            #region Creating Values

            Obfuscator.CurrentObfuscator.UpdateStatus(ObfuscatorStatus.CreatingValues);

            string VarargName = CheckNumber("586LOL_FAKENAME"); // Helpers.RandomString(7, 15)

            settings.A_Idx = (ushort)Helpers.random.Next(0, ushort.MaxValue);
            settings.B_Idx = (ushort)Helpers.random.Next(0, ushort.MaxValue);
            settings.C_Idx = (ushort)Helpers.random.Next(0, ushort.MaxValue);
            settings.OP_Idx = (ushort)Helpers.random.Next(0, ushort.MaxValue);

            #endregion

            string script = $@"local function b64_decode(data)
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
    local binNum = ''
    if n ~= 0 then
        while n >= 1 do
            if n %2 == 0 then
                binNum = binNum .. '0'
                n = n / 2
            else
                binNum = binNum..'1'
                n = (n - 1) / 2
            end
        end
    else
        binNum = '0'
    end

    for i = #binNum, places - 1 do
        binNum = binNum..'0'
    end

    return string.reverse(binNum)
end
local function bpcall(func, ...)
    local data = {"{pcall(func, ...)}"}
    return data[1], {"{unpack(data, 2, #data)}"}
end

local bitlib = bit32 or bit or nil
local modm = math.pow(2, 32) - 1

local band = (bitlib and bitlib.band) or function(a, b)
    local r, c = 0,1
    while a > 0 and b> 0 do
        if a % 2 == 1 and b% 2 == 1 then r = r + c end
        c = c * 2
        a = math.floor(a / 2)
        c = math.floor(c / 2)
    end
    return r
end
local bxor = (bitlib and bitlib.bxor) or function(a, b)
    local c, e = 1,0
    while a > 0 and b> 0 do
        local f, g = a % 2, b% 2
        if f~= g then e = e + c end
        a, b, c = (a - f) / 2,(b - g)/ 2,c * 2
    end
    if a < b then a = b end
    while a > 0 do
        local f = a % 2
        if f > 0 then e = e + c end
        a, c = (a - f) / 2,c * 2
    end
    return e
end
local rshift = (bitlib and bitlib.rshift) or function(a, b)
    return math.floor(a / math.pow(2, b))
end

local bytecode = b64_decode('{Encoding.UTF8.GetString(serialized)}')
local pos = 1

local function GetByte()
    local l = string.byte(string.sub(bytecode, pos, pos))
    pos = pos + 1
    return l
end
local function ReadByte()
    local b1 = num2bin(GetByte(), 8)
    local data = num2bin(GetByte(), 8)
    local xorBin = ''

    for i = 1, #data do
        local bb = (string.sub(b1, i, i) == '1' and true or false)
        local db = (string.sub(data, i, i) == '1' and true or false)

        if bb == true then
            xorBin = xorBin..((not db) == true and '1' or '0')
        else
            xorBin = xorBin..(db == true and '1' or '0')
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

    return (W * math.pow(2, 24)) + (Z * math.pow(2, 16)) + (Y * math.pow(2, 8)) + X
end
local function ReadInt64()
    local X, Y, Z, W = ReadByte(), ReadByte(), ReadByte(), ReadByte()
    local X1, Y1, Z1, W1 = ReadByte(), ReadByte(), ReadByte(), ReadByte()

    local p1, p2 = 0, 0
    p1 = (W1 * math.pow(2, 56)) + (Z1 * math.pow(2, 48)) + (Y1 * math.pow(2, 40)) + (X1 * math.pow(2, 32))
    p2 = (W * math.pow(2, 24)) + (Z * math.pow(2, 16)) + (Y * math.pow(2, 8)) + X

    return p1 + p2
end
local function ReadDouble()
    local full = ReadInt64()
    local sign = band(full, 1) == 1
    local num = rshift(full, 1)

    if sign == true then num = -num end

    return tonumber(tostring(num)..'.'..tostring(ReadInt64()))
end
local function ReadString()
    local function concat(tbl)
        local r = ''
        for i = 1,#tbl do
            r = r..tostring(tbl[i])
        end
        return r
    end

    local len = ReadInt32()

    if len == 0 then
        return ''
    end

    local tbl = {"{}"}
    for i = 1, len do
        tbl[i] = string.char(ReadByte())
    end

    return concat(tbl)
end

local function Deserialize()
    local hasDebug = ReadBool()
    local steps = ReadInt32()

    local Chunk = {"{ {}, {}, {}, { {}, {} }, "} {VarargName} = 0 {"}"}
    
    Chunk['{VarargName}'] = ReadByte()

    for i = 1, steps do
        local step = ReadByte()
        if step == 0 then
            local count = ReadInt32()
            for k = 1, count do
                local ttype = ReadByte()
                if ttype == 0 then
                    assert(ReadString() == 'NIL', 'must be nil')
                    Chunk[2][k] = nil
                elseif ttype == 1 then
                    Chunk[2][k] = ReadBool()
                elseif ttype == 3 then
                    Chunk[2][k] = ReadDouble()
                elseif ttype == 4 then
                    Chunk[2][k] = ReadString()
                end
            end
        elseif step == 1 then
            local count = ReadInt32()
            for k = 1, count do
                local instr = {"{}"}
                local opmode = num2bin(ReadByte(), 8):reverse()

                if string.sub(opmode, 1, 1) == '1' then
                    instr[{settings.A_Idx}] = ReadByte()
                end
                if string.sub(opmode, 2, 2) == '1' then
                    instr[{settings.B_Idx}] = ReadInt16()
                end
                if string.sub(opmode, 4, 4) == '1' then
                    instr[{settings.B_Idx}] = ReadInt32()
                end
                if string.sub(opmode, 5, 5) == '1' then
                    instr[{settings.B_Idx}] = ReadInt32() - 131072
                end
                if string.sub(opmode, 3, 3) == '1' then
                    instr[{settings.C_Idx}] = ReadInt16()
                end
                if string.sub(opmode, 6, 6) == '1' then
                    instr['raw'] = ReadInt32()
                end

                instr[{settings.OP_Idx}] = ReadInt16()

                Chunk[1][k] = instr
            end
        elseif step == 2 then
            local count = ReadInt32()
            for k = 1, count do
                Chunk[3][k] = Deserialize()
            end
        elseif step == 3 and hasDebug == true then
        end
    end

    return Chunk
end

local function Wrap(Func, Globals, UpValues)
    UpValues = UpValues or {"{}"}

    return function(...)
        local Instructions = Func[1]
        local Constants = Func[2]
        local Protos = Func[3]
        
        local PC = 1
        local Top = -1

        local Vararg = {"{}"}
        local Argv, Argc = {"{...}"}, #{"{...}"}

        local UpValues = {"{}"}
        local Stack = {"{}"}

        local varargp = Func['{VarargName}']
        for i = 0, Argc do
            if (i >= varargp) then
                Vararg[i - varargp] = Argv[i + 1]
            else
                Stack[i] = Argv[i + 1]
            end
        end

        local VarargCount = Argc - varargp + 1

        local succ, data = bpcall(function()
            while true do
                local Instruction = Instructions[PC]

";

            if (settings.SerializeDebug)
            {
                string warning = @"--[[
---------------------------------------------
-__ ---------__--------------------------- _ 
-\ \------- / /-------------(-)-----------| |
--\ \--/\--/ /_ _ _ __ _ __  _ _ __   __ _| |
---\ \/  \/ / _` | '__| '_ \| | '_ \ / _` | |
----\  /\  / (_| | |  | | | | | | | | (_| |_|
-----\/  \/ \__,_|_|  |_| |_|_|_| |_|\__, (_)
--------------------------------------__/ |--
-------------------------------------|___/ --
-----------Debug info is serialized----------
]]";

                script = warning + script;
            }

            #region PrintTable function
        
            /* made this a region cause VS is being dumb
#if DEBUG
            script = @"local function PrintTbl(tbl, label, deepPrint)
	assert(type(tbl) == 'table', 'First argument must be a table')
    assert(label == nil or type(label) == 'string', 'Second argument must be a string or nil')

    label = (label or 'TABLE')

	local strTbl = { }
    local indent = ' - '

    local function Insert(s, l)
        strTbl[#strTbl + 1] = (indent:rep(l) .. s .. '\n')
	end
    local function AlphaKeySort(a, b)
        return (tostring(a.k) < tostring(b.k))
    end
    local function PrintTable(t, lvl, lbl)
        Insert(lbl..':', lvl - 1)
        local nonTbls = { }
        local tbls = { }
        local keySpaces = 0

        for k, v in pairs(t) do
            if (type(v) == 'table') then
                 table.insert(tbls, { k = k, v = v})
            else
                table.insert(nonTbls, { k = k, v = '['..type(v)..'] '..tostring(v)})
			end
            local spaces = #tostring(k) + 1
			if (spaces > keySpaces) then
                keySpaces = spaces
            end
        end
        for _, v in pairs(nonTbls) do
            Insert(tostring(v.k)..':'..(' '):rep(keySpaces - #tostring(v.k)) .. v.v, lvl)
		end
        if (deepPrint) then
            for _, v in pairs(tbls) do
                PrintTable(v.v, lvl + 1, tostring(v.k)..(' '):rep(keySpaces - #tostring(v.k)) .. ' [Table]')
			end
        else
            for _, v in pairs(tbls) do
                Insert(tostring(v.k)..':'..(' '):rep(keySpaces - #tostring(v.k)) .. '[Table]', lvl)
			end
        end
    end
      
    PrintTable(tbl, 1, label)

    return table.concat(strTbl, '')
end
" + "\n" + script;     
#endif
            */

            #endregion

            #region Add VOpcodes to the vm

            Obfuscator.CurrentObfuscator.UpdateStatus(ObfuscatorStatus.CreatingVOpcodes);

            foreach (OpCodes op in VOpcodeGenerator.UsedOpcodes)
            {
                VOpcode vop = VOpcodeGenerator.vOpcodes[op];

                script += $"if Instruction[{settings.OP_Idx}] == {vop.opcode} then\n";
                script += "    "+vop.GetOpCode(settings)+"\n";
                script += "end\n";
            }

            #endregion

            Obfuscator.CurrentObfuscator.UpdateStatus(ObfuscatorStatus.GeneratingVM);

            return script + $@"
                PC = PC + 1
            end
        end)

        if not succ then
            local Instr = Instructions[PC]
            local A, B, C, Op = Instr[{settings.A_Idx}] or 'nil', Instr[{settings.B_Idx}] or 'nil', Instr[{settings.C_Idx}] or 'nil', Instr[{settings.OP_Idx}] or 'nil'
            error('Got an error on instruction: ' .. PC .. '[\nA: ' .. A .. '\nB: ' .. B .. '\nC: ' .. C .. '\nOP: ' .. Op .. ' (to find the original opcode use filename_VM.log in Temp)\n]', unpack(data))
        end

        return unpack(data)
    end
end
Wrap(Deserialize(), (getfenv and getfenv()) or _ENV)()";
        }
    }
}
