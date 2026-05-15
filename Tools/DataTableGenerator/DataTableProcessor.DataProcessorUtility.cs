using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private static class DataProcessorUtility
        {
            private static readonly SortedDictionary<string, DataProcessor> s_DataProcessors =
                new SortedDictionary<string, DataProcessor>(StringComparer.OrdinalIgnoreCase);

            static DataProcessorUtility()
            {
                // Auto-discover all DataProcessor implementations via reflection
                Type baseType = typeof(DataProcessor);
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.IsAbstract || type.IsGenericType)
                    {
                        continue;
                    }

                    if (!baseType.IsAssignableFrom(type))
                    {
                        continue;
                    }

                    var processor = (DataProcessor)Activator.CreateInstance(type)!;
                    foreach (string typeString in processor.GetTypeStrings())
                    {
                        s_DataProcessors[typeString] = processor;
                    }
                }
            }

            public static DataProcessor GetDataProcessor(string type)
            {
                if (s_DataProcessors.TryGetValue(type ?? string.Empty, out DataProcessor? processor))
                {
                    return processor;
                }

                throw new Exception($"Data processor for type '{type}' is not found.");
            }
        }
    }
}
