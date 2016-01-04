using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public abstract class DynamicOutputFile : DynamicSourceFile
    {
        private OutputFile _outputFile;

        protected DynamicOutputFile(OutputFile file)
            : base(file)
        {
            _outputFile = file;
        }

        protected override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            data.Add(nameof(_outputFile.OutputPath), _outputFile.OutputPath);
            data.Add(nameof(_outputFile.OutputRootPath), _outputFile.OutputRootPath);
            data.Add(nameof(_outputFile.OutputRelativePath), _outputFile.OutputRelativePath);
            data.Add(nameof(_outputFile.Url), _outputFile.Url);
            data.Add(nameof(_outputFile.RootUrl), _outputFile.RootUrl);
            data.Add(nameof(_outputFile.RelativeUrl), _outputFile.RelativeUrl);
            data.Add(nameof(_outputFile.TargetExtension), _outputFile.TargetExtension);

            return data;
        }
    }
}