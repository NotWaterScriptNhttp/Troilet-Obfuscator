using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCLI
{
    internal class ArgParser
    {
        enum ArgType
        {
            Bool,
            String,
            Int,
            Double
        }
        struct Arg
        {
            public ArgType Type;
            public object Value;

            public Arg(ArgType type, object value)
            {
                Type = type;
                Value = value;
            }
        }

        private Dictionary<string, Arg> Args = new Dictionary<string, Arg>();

        public void Parse(string[] args)
        {
            foreach (var arg in args)
            {
                int idx = arg.IndexOf(':');
                if (idx == -1)
                {
                    Arg sett;
                    if (!Args.TryGetValue(arg, out sett))
                        sett = new Arg(ArgType.Bool, true);
                    else sett.Value = true;

                    Args[arg] = sett;
                    continue;
                }

                string paramName = arg.Substring(0, idx);
                string value = arg.Substring(idx + 1);

                Arg param;
                if (!Args.TryGetValue(paramName, out param))
                {
                    Args[paramName] = new Arg(ArgType.String, value);
                    continue;
                }

                switch (param.Type)
                {
                    case ArgType.Bool:
                        if (value.Length == 1)
                            param.Value = value == "1";
                        else param.Value = value.ToLower() == "true";
                        break;
                    case ArgType.String:
                        param.Value = value;
                        break;
                    case ArgType.Int:
                        if (value.StartsWith("0x"))
                            param.Value = Convert.ToInt32(value, 16);
                        else if (value.StartsWith("0b"))
                            param.Value = Convert.ToInt32(value, 2);
                        else param.Value = Convert.ToInt32(value);
                        break;
                    case ArgType.Double:
                        param.Value = Convert.ToDouble(value);
                        break;
                }

                Args[paramName] = param;
            }
        }

        public void AddSetting(string name, bool def = false)
        {
            if (Args.ContainsKey(name))
                throw new ArgumentException("This arg already exists!", "name");

            Args[name] = new Arg(ArgType.Bool, def);
        }

        public void AddParam(string name, string val)
        {
            if (Args.ContainsKey(name))
                throw new ArgumentException("This arg already exists!", "name");

            Args[name] = new Arg(ArgType.String, val);
        }
        public void AddParam(string name, bool val)
        {
            if (Args.ContainsKey(name))
                throw new ArgumentException("This arg already exists!", "name");

            Args[name] = new Arg(ArgType.Bool, val);
        }
        public void AddParam(string name, int val)
        {
            if (Args.ContainsKey(name))
                throw new ArgumentException("This arg already exists!", "name");

            Args[name] = new Arg(ArgType.Int, val);
        }
        public void AddParam(string name, double val)
        {
            if (Args.ContainsKey(name))
                throw new ArgumentException("This arg already exists!", "name");

            Args[name] = new Arg(ArgType.Double, val);
        }

        public bool HasSetting(string name)
        {
            if (!Args.TryGetValue(name, out Arg arg))
                return false;

            if (arg.Type != ArgType.Bool)
                throw new ApplicationException($"Arg of type {arg.Type} cannot be converted to bool");

            return (bool)arg.Value;
        }
        public T? GetParam<T>(string name, T? def = default)
        {
            if (!Args.TryGetValue(name, out Arg arg))
                return def;

            if (typeof(T) != arg.Value.GetType())
                return def;

            return (T?)arg.Value;
        }
    }
}
