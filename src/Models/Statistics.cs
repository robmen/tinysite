using System;
using System.Diagnostics;

namespace TinySite.Models
{
    public enum StatisticTiming
    {
        Overall,
        LoadedConfiguration,
        LoadedSite,
        Ordering,
        Pagination,
        RenderPartialsContent,
        RenderPartialsLayouts,
        RenderPartials,
        RenderDocumentContent,
        RenderDocumentLayouts,
        RenderDocuments,
        WriteDocuments,
        CopyStaticFiles,
        Rendered,
        Max,
    };

    public class Statistics
    {
        public static Statistics Current { get; set; }

        public Statistics()
        {
            this.Stopwatch = Stopwatch.StartNew();
            this.Timings = new CapturedTiming[(int)StatisticTiming.Max];
        }

        public int SiteFiles { get; set; }

        public int OrderedFiles { get; set; }

        public int PagedFiles { get; set; }

        public int RenderedPartials { get; set; }

        public int RenderedDocuments { get; set; }

        public int WroteDocuments { get; set; }

        public int CopiedFiles { get; set; }

        public Stopwatch Stopwatch { get; private set; }

        public CapturedTiming[] Timings { get; private set; }

        public CapturedTiming Start(StatisticTiming timing)
        {
            return this.Timings[(int)timing] = new CapturedTiming() { Timing = timing, Started = this.Stopwatch.ElapsedMilliseconds };
        }

        public void Stop(StatisticTiming timing)
        {
            this.Timings[(int)timing].Stopped = this.Stopwatch.ElapsedMilliseconds;
        }

        public void Report()
        {
            this.ReportTiming("   Loaded configuration in {0} s", StatisticTiming.LoadedConfiguration);
            this.ReportTiming("   Loaded {1} site files in {0} s", StatisticTiming.LoadedSite, this.SiteFiles);
            this.ReportTiming("   Ordered {1} the documents s", StatisticTiming.Pagination, this.OrderedFiles);
            this.ReportTiming("   Paginated the site into {1} documents {0} s", StatisticTiming.Pagination, this.PagedFiles);
            this.ReportTiming("      Rendered {1} partials in {0} s", StatisticTiming.RenderPartials, this.RenderedPartials);
            this.ReportTiming("          Partial content rendered in {0} s", StatisticTiming.RenderPartialsContent);
            this.ReportTiming("          Partial layouts rendered in {0} s", StatisticTiming.RenderPartialsLayouts);
            this.ReportTiming("      Rendered {1} documents in {0} s", StatisticTiming.RenderDocuments, this.RenderedDocuments);
            this.ReportTiming("          Document content rendered in {0} s", StatisticTiming.RenderDocumentContent);
            this.ReportTiming("          Document layouts rendered in {0} s", StatisticTiming.RenderDocumentLayouts);
            this.ReportTiming("      Wrote {1} documents to disk in {0} s", StatisticTiming.WriteDocuments, this.WroteDocuments);
            this.ReportTiming("      Copied {1} static files in {0} s", StatisticTiming.CopyStaticFiles, this.CopiedFiles);
            this.ReportTiming("   Rendered the site in {0} s", StatisticTiming.Rendered);
            this.ReportTiming("Processing complete in {0} s", StatisticTiming.Overall, this.SiteFiles);
        }

        private void ReportTiming(string format, StatisticTiming timing, params object[] other)
        {
            var capture = this.Timings[(int)timing];

            var data = new object[1 + other.Length];
            data[0] = (capture == null) ? 0 : (capture.Stopped - capture.Started) / (float)1000; ;
            other.CopyTo(data, 1);

            Console.WriteLine(format, data);
        }

        public class CapturedTiming : IDisposable
        {
            public StatisticTiming Timing { get; set; }

            public long Started { get; set; }

            public long Stopped { get; set; }

            public void Dispose()
            {
                this.Stopped = Statistics.Current.Stopwatch.ElapsedMilliseconds;
            }
        }
    }
}
