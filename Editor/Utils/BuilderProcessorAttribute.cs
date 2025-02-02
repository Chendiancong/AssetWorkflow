
using System;
using System.Collections.Generic;
using System.Reflection;

namespace cdc.BundleWorkFlow.Editor
{
    public enum BuilderProcessorPhase
    {
        PreBuild,
        PostBuild,
        PreClean,
        PostClean,
    }

    public interface IBuilderProcessor
    {
        void Execute();
    }

    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct)]
    public class BuilderProcessorAttribute : Attribute
    {
        private BuilderProcessorPhase m_phase;
        public BuilderProcessorAttribute(BuilderProcessorPhase phase)
        {
            m_phase = phase;
        }

        private static Dictionary<BuilderProcessorPhase, List<IBuilderProcessor>> m_processors;
        public static void ExecutePhase(BuilderProcessorPhase phase)
        {
            List<IBuilderProcessor> list;
            if (m_processors == null)
            {
                m_processors = new Dictionary<BuilderProcessorPhase, List<IBuilderProcessor>>();
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        var attr = type.GetCustomAttribute<BuilderProcessorAttribute>();
                        if (attr != null)
                        {
                            if (!m_processors.TryGetValue(attr.m_phase, out list))
                            {
                                list = new List<IBuilderProcessor>();
                                m_processors[attr.m_phase] = list;
                            }
                            IBuilderProcessor processor = Activator.CreateInstance(type) as IBuilderProcessor;
                            if (processor != null)
                                list.Add(processor);
                        }
                    }
                }
            }

            if (m_processors.TryGetValue(phase, out list))
            {
                foreach (var processor in list)
                    processor.Execute();
            }
        }
    }
}