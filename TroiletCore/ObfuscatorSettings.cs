using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaLib.Emit;

namespace TroiletCore
{
    public struct TypeData
    {
        public int XorKey;
        public int TypeID;
    }
    public enum BytecodeAppearance
    {
        Random,
        AllahHabibi
    }

    public class ObfuscatorSettings
    {
        // idk if this will even be used
        public Dictionary<ConstantType, TypeData> TypeDatas;

        // Non-User settings
        public int MainXorKey;
        public ushort A_Idx, B_Idx, C_Idx, OP_Idx; // b is used for B, Bx and sBx

        // User settings
        public BytecodeAppearance bAppearance;
        public bool SerializeDebug = false;
    }
}
