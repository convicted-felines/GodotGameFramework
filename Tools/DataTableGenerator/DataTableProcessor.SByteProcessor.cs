using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class SByteProcessor : GenericDataProcessor<sbyte>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "sbyte";

            public override string[] GetTypeStrings() => new[] { "sbyte", "system.sbyte" };

            public override sbyte Parse(string value) => sbyte.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}
