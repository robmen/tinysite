using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public abstract class DynamicOutputFile : DynamicSourceFile
    {
        protected DynamicOutputFile(OutputFile file)
            : base(file)
        {
            this.File = file;
        }

        private OutputFile File { get; }

        protected override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            data.Add(nameof(this.File.OutputPath), this.File.OutputPath);
            data.Add(nameof(this.File.OutputRootPath), this.File.OutputRootPath);
            data.Add(nameof(this.File.OutputRelativePath), this.File.OutputRelativePath);
            data.Add(nameof(this.File.Url), this.File.Url);
            data.Add(nameof(this.File.RootUrl), this.File.RootUrl);
            data.Add(nameof(this.File.RelativeUrl), this.File.RelativeUrl);
            data.Add(nameof(this.File.TargetExtension), this.File.TargetExtension);

            return data;
        }
    }
}