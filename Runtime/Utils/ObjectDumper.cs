using System;
using System.Reflection;
using System.Text;

namespace cdc.AssetWorkflow
{
    public static class ObjectDumper
    {
        private static StringBuilder m_sb = new StringBuilder();

        public static string Dump(object target)
        {
            Type type = target.GetType();
            m_sb.Clear();
            m_sb.AppendLine($"{type.Name} = {{");
            foreach (var field in type.GetFields())
                m_sb.AppendLine($"{field.Name} = {type.InvokeMember(field.Name, BindingFlags.GetField, null, target, null)}");
            m_sb.AppendLine("}");
            return m_sb.ToString();
        }
    }
}