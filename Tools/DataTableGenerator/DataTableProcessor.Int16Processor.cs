using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class Int16Processor : GenericDataProcessor<short>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "short";

            public override string[] GetTypeStrings() => new[] { "short", "int16", "system.int16" };

            public override short Parse(string value) => short.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}
