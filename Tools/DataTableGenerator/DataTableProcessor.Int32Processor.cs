using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class Int32Processor : GenericDataProcessor<int>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "int";

            public override string[] GetTypeStrings() => new[] { "int", "int32", "system.int32" };

            public override int Parse(string value) => int.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write7BitEncodedInt(Parse(value));
            }
        }
    }
}
