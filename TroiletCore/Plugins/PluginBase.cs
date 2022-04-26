using System;
using System.Collections.Generic;

using LuaLib;
using LuaLib.Emit;

namespace TroiletCore.Plugins
{
    //TODO: make the OnType functions return Tuple<Type, string(function to call in lua)>
    public abstract class PluginBase
    {
        //Properties
        public Obfuscator _obfuscator { get; internal set; }

        public PluginLogger Logger { get; internal set; }
        public bool IsEnabled { get; internal set; }

        abstract public string Name { get; protected set; }
        abstract public string Author { get; protected set; }
        abstract public string Version { get; protected set; }
        abstract public string Description { get; protected set; }

        //Public methods
        public abstract bool OnEnable();
        public abstract void OnDisable();

        virtual public void OnCreate() { }

        virtual public object[] OnObfStateUpdate(ObfState state, object[] args) { return args; }

        //Internals (i don't count these as private, i count them as protected in a way)
        internal bool Enable()
        {
            IsEnabled = true;
            return OnEnable();
        }
        internal void Disable()
        {
            IsEnabled = false;
            OnDisable();
        }
    }
}
