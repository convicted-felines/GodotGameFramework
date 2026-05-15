using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class SingleProcessor : GenericDataProcessor<float>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "float";

            public override string[] GetTypeStrings() => new[] { "float", "single", "system.single" };

            public override float Parse(string value) => float.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}
