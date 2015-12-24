using System.Reflection;
using Odin.Attributes;
using Odin.Parsing;

namespace Odin.Configuration
{
    public abstract class Conventions
    {
        public abstract string GetCommandName(Command command);
        public abstract string GetLongOptionName(ParameterInfo row);
        public abstract string GetActionName(MethodInfo methodInfo);
        public abstract string GetShortOptionName(string rawAlias);
        public abstract bool IsMatchingParameter(ParameterValue parameterMap, string arg);
        public abstract IParser CreateParser(ParameterValue parameter);
    }
}