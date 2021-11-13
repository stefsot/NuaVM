using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NuaVM.VM;

// ReSharper disable IdentifierTypo

namespace NuaVM.Types
{
    public class NuaTable : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.table;

        public override object Value { get; protected set; } = new Dictionary<object, NuaObject>();

        // NuaTable implementation
        // -------------------------------------

        public delegate NuaObject IndexDelegate(NuaExecutionContext context, NuaTable table, NuaObject key);
        public event IndexDelegate OnIndex;

        public delegate void NewIndexDelegate(NuaExecutionContext context, NuaTable table, NuaObject key,
            NuaObject value);
        public event NewIndexDelegate OnNewIndex;

        // ReSharper disable once InconsistentNaming
        public Dictionary<object, NuaObject> Dictionary
        {
            get => (Dictionary<object, NuaObject>) Value;
            protected set => Value = value;
        }

        // TODO: doesn't work as lua specification says
        public override int Length => Dictionary.Count;

        public NuaTable Metatable { get; set; }

        public NuaObject this[NuaExecutionContext context, NuaObject key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                OnIndex?.Invoke(context, this, key);
                return Get(context, key);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                OnNewIndex?.Invoke(context, this, key, value);
                Set(context, key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NuaObject IndexMetaMethodHandler(NuaExecutionContext context, NuaObject key)
        {
            var indexer = Metatable[context, "__index"] as NuaFunction;

            if (indexer != null && !indexer.IsNull)
            {
                indexer.Metadata.DebugName = "__index metamethod";

                var result = indexer.Invoke(context, this, key);
                return result?.Length > 0 ? result[0] : new NuaNull();
            }

            return Metatable[context, key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NuaObject Get(NuaExecutionContext context, NuaObject key)
        {
            switch (key.Type)
            {
                case NuaObjectType.boolean:
                case NuaObjectType.number:
                case NuaObjectType.@string:
                case NuaObjectType.function:
                {
                    if (Dictionary.TryGetValue(key.Value, out var value))
                        return value;

                    break;
                }

                case NuaObjectType.table:
                case NuaObjectType.userdata:
                {
                    if (Dictionary.TryGetValue(key, out var value))
                        return value;

                    break;
                }
            }

            if (Metatable != null && context != null)
            {
                return IndexMetaMethodHandler(context, key);
            }

            return new NuaNull();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool NewIndexMetaMethodHandler(NuaExecutionContext context, NuaObject key, NuaObject value)
        {
            var newindex = Metatable[context, "__newindex"] as NuaFunction;

            if (newindex != null && !newindex.IsNull)
            {
                newindex.Invoke(context, this, key, value);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(NuaExecutionContext context, NuaObject key, NuaObject value)
        {
            if (key.IsNull)
                return;

            switch (key.Type)
            {
                case NuaObjectType.boolean:
                case NuaObjectType.number:
                case NuaObjectType.@string:
                case NuaObjectType.function:
                    if (!Dictionary.ContainsKey(key.Value) && Metatable != null)
                    {
                        if(!NewIndexMetaMethodHandler(context, key, value))
                            Dictionary[key.Value] = value;
                    }
                    else
                    {
                        Dictionary[key.Value] = value;
                    }

                    break;

                default:
                    if (!Dictionary.ContainsKey(key) && Metatable != null)
                    {
                        if (!NewIndexMetaMethodHandler(context, key, value))
                        {
                            Dictionary[key] = value;
                        }
                    }
                    else
                    {
                        Dictionary[key] = value;
                    }
                    
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string key, NuaObject value)
        {
            Dictionary[key] = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("table {");

            foreach (var k in Dictionary.Keys)
            {
                sb.Append($"[{k}] = {Dictionary[k]}, ");
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
