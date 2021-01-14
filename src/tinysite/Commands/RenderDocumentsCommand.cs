﻿using System;
using System.Collections.Generic;
using System.Linq;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderDocumentsCommand
    {
        public RenderDocumentsCommand(IDictionary<string, RenderingEngine> engines, Site site)
        {
            this.Engines = engines;
            this.Site = site;
        }

        public int RenderedData { get; private set; }

        public int RenderedDocuments { get; private set; }

        private IDictionary<string, RenderingEngine> Engines { get; }

        private Site Site { get; }

        public void Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                var documentRendering = new ContentRendering(tx);

                IEnumerable<DataFile> renderedData;
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderData))
                {
                    renderedData = this.Site.Data
                        .AsParallel()
                        .Select(documentRendering.RenderDataContent)
                        .ToList();
                }

                IEnumerable<DocumentFile> renderedDocuments;
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentContent))
                {
                    renderedDocuments = this.Site.Documents
                        .Where(d => !d.Draft && !d.Unmodified)
                        .AsParallel()
                        .Select(documentRendering.RenderDocumentContent)
                        .ToList();
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentLayouts))
                {
                    foreach (var document in renderedDocuments)
                    {
                        var content = document.Content;

                        foreach (var layout in document.Layouts)
                        {
                            content = documentRendering.RenderDocumentContentUsingLayout(document, content, layout);
                        }

                        document.RenderedContent = content;

                        document.Rendered = (document.RenderedContent != null);

#if DEBUG
                        Console.WriteLine("Rendered: {0} to {1}", document.SourceRelativePath, document.OutputRelativePath);

                        foreach (var contributor in document.AllContributingFiles())
                        {
                            Console.WriteLine("   Contributor: {0}", contributor.SourceRelativePath);
                        }

                        Console.WriteLine();
#endif
                    }
                }

                this.RenderedData = renderedData.Count();

                this.RenderedDocuments = renderedDocuments.Count();
            }
        }
    }
}
