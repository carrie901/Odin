using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Odin.Attributes;
using Odin.Configuration;
using Odin.Exceptions;

namespace Odin
{
    public class ParameterValue
    {
        public static Dictionary<Type, Func<object, object>> Coercion { get;  }
        static ParameterValue()
        {
            Coercion = new Dictionary<Type, Func<object, object>>
            {
                [typeof(bool)] = o => bool.Parse(o.ToString()),
                [typeof(int)] = o => int.Parse(o.ToString()),
                [typeof(long)] = o => long.Parse(o.ToString()),
                [typeof(double)] = o => double.Parse(o.ToString()),
                [typeof(decimal)] = o => decimal.Parse(o.ToString()),
                [typeof(DateTime)] = o => DateTime.Parse(o.ToString()),

                [typeof(bool?)] = o => bool.Parse(o.ToString()),
                [typeof(int?)] = o => int.Parse(o.ToString()),
                [typeof(long?)] = o => long.Parse(o.ToString()),
                [typeof(double?)] = o => double.Parse(o.ToString()),
                [typeof(decimal?)] = o => decimal.Parse(o.ToString()),
                [typeof(DateTime?)] = o => DateTime.Parse(o.ToString()),

            };
        }


        private bool _isSet;

        private object _value;

        public ParameterValue(MethodInvocation methodInvocation, ParameterInfo parameterInfo)
        {
            MethodInvocation = methodInvocation;
            ParameterInfo = parameterInfo;

            if (ParameterType == typeof(bool))
                Value = false;

            if (IsNullableType())
                Value = null;

            if (ParameterInfo.IsOptional)
                Value = Type.Missing;
        }

        private bool IsNullableType()
        {
            return ParameterType.IsGenericType && ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }


        public MethodInvocation MethodInvocation { get; }

        public Conventions Conventions => MethodInvocation.Conventions;

        public ParameterInfo ParameterInfo { get;  }
        public Type ParameterType => ParameterInfo.ParameterType;

        public int Position => ParameterInfo.Position;

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _isSet = true;
            }
        }

        public string Name => ParameterInfo.Name;

        public string Switch => Conventions.GetArgumentName(this.ParameterInfo);

        public string[] Tokens => MethodInvocation.Tokens;

        public string GetDescription()
        {
            var attr = ParameterInfo.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
                return attr.Description;
            return "";
        }

        public bool IsValueSet()
        {
            return _isSet;
        }
        public bool IsBooleanSwitch()
        {
            return ParameterType == typeof(bool)
                || ParameterType == typeof(bool?)
                ;
        }

        public bool IsIdentifiedBy(string arg)
        {
            if (Conventions.IsIdentifiedBy(this, arg))
                return true;
            return HasAlias(arg);
        }

        private bool HasAlias(string arg)
        {
            var attr = this.ParameterInfo.GetCustomAttribute<AliasAttribute>();
            return Conventions.MatchesAlias(attr, arg);
        }

        public object Coerce(object value)
        {
            try
            {
                var key = this.ParameterType;
                if (Coercion.ContainsKey(key))
                    return Coercion[key].Invoke(value);

                if (key.IsEnum)
                    return Enum.Parse(key, value.ToString());

                if (IsNullableType())
                {
                    var genericType = key.GetGenericArguments()[0];
                    if (genericType.IsEnum)
                    {
                        return Enum.Parse(genericType, value.ToString());
                    }
                }

                return value;
            }
            catch (Exception e)
            {
                throw new ParameterConversionException(this, value, e);
            }
        }

        public bool HasNextValue(int indexOfCurrentArg)
        {
            return Tokens.Length > (indexOfCurrentArg + 1);
        }

        public bool NextArgIsIdentifier(int indexOfCurrentArg)
        {
            var j = indexOfCurrentArg + 1;
            if (j < Tokens.Length)
            {
                return Conventions.IsArgumentIdentifier(Tokens[j]);
            }
            return false;
        }

        public bool HasAliases()
        {
            var attr = this.ParameterInfo.GetCustomAttribute<AliasAttribute>();
            if (attr == null)
                return false;

            return attr.Aliases.Any();
        }

        public string[] GetAliases()
        {
            var attr = this.ParameterInfo.GetCustomAttribute<AliasAttribute>();
            if (attr == null)
                return new string[] { };

            return attr.Aliases.Select(a => Conventions.GetFormattedAlias(a)).ToArray();
        }


    }
}