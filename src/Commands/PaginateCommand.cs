using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite.Extensions;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class PaginateCommand
    {
        public string RootUrl { private get; set; }

        public Site Site { private get; set; }

        public IEnumerable<DocumentFile> PagedDocuments { get; private set; }

        public void Execute()
        {
            var dupes = new List<DocumentFile>();

            foreach (var document in this.Site.Documents.Where(d => !String.IsNullOrEmpty(d.PaginateQuery)))
            {
                var query = QueryProcessor.Parse(this.Site, document.PaginateQuery);

                var pagedPosts = query.Results.OfType<DynamicDocumentFile>().Select(d => d.GetDocument()).ToList();

                var count = pagedPosts.Count();

                var pages = (count + query.PageEvery - 1) / query.PageEvery;

                var urlFormat = query.FormatUrl;

                var lastSlash = document.RelativeUrl.LastIndexOf('/');

                var documentRelativeUrl = document.RelativeUrl.Substring(0, lastSlash + 1);

                if (!String.IsNullOrEmpty(urlFormat))
                {
                    urlFormat = urlFormat.TrimStart('/');

                    var prependPathFormat = Path.Combine(Path.GetDirectoryName(document.OutputRelativePath), urlFormat.Replace('/', '\\'));

                    urlFormat = String.Concat(documentRelativeUrl, urlFormat.Replace('\\', '/').EnsureEndsWith("/"));

                    for (int i = 1; i < pages; ++i)
                    {
                        var paginator = this.CreatePaginator(i + 1, query.PageEvery, pages, documentRelativeUrl, urlFormat, pagedPosts);

                        var dupe = document.CloneForPage(urlFormat, prependPathFormat, paginator);

                        dupe.AddContributingFiles(pagedPosts);

                        dupes.Add(dupe);
                    }
                }

                document.Paginator = this.CreatePaginator(1, query.PageEvery, pages, documentRelativeUrl, urlFormat, pagedPosts);

                document.AddContributingFiles(pagedPosts);
            }

            this.PagedDocuments = dupes;
        }

        private Paginator CreatePaginator(int page, int perPage, int pages, string baseUrl, string urlFormat, IEnumerable<DocumentFile> documents)
        {
            // It is important that this query is not executed here (aka: do not add ToList() or ToArray()). This
            // query should be executed by the rendering engine so the returned documents are rendered first.
            var pagedDocuments = documents.Skip((page - 1) * perPage).Take(perPage);

            var pagination = new Pagination();

            if (pages > 1 && !String.IsNullOrEmpty(urlFormat))
            {
                pagination.Page = page;
                pagination.PerPage = perPage;
                pagination.TotalPage = pages;
                pagination.NextPageUrl = page < pages ? this.UrlForPage(page + 1, baseUrl, urlFormat) : null;
                pagination.PreviousPageUrl = page > 1 ? this.UrlForPage(page - 1, baseUrl, urlFormat) : null;

                var start = Math.Max(1, page - 3);
                var end = Math.Min(pages, start + 6);
                start = Math.Max(start, end - 6);

                pagination.Pages = this.CreatePages(page, start, end, baseUrl, urlFormat).ToList();
            }

            return new Paginator(pagedDocuments, pagination);
        }

        private DocumentFile DupeDocumentForPage(DocumentFile document, string urlFormat, string prependPathFormat, Paginator paginator)
        {
            var prependPath = String.Format(prependPathFormat, paginator.Pagination.Page);

            var prependUrl = String.Format(urlFormat, paginator.Pagination.Page);

            var dupe = document.CloneForPage(urlFormat, prependPathFormat, paginator);

            var updateFileName = Path.GetFileName(dupe.OutputRelativePath);

            dupe.OutputRelativePath = Path.Combine(prependPath, updateFileName);

            dupe.OutputPath = Path.Combine(dupe.OutputRootPath, prependPath, updateFileName);

            dupe.RelativeUrl = String.Concat(prependUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);

            dupe.Paginator = paginator;
            return dupe;
        }

        private IEnumerable<Page> CreatePages(int current, int start, int end, string baseUrl, string urlFormat)
        {
            for (int i = start; i <= end; ++i)
            {
                var page = new Page();
                page.Active = (i == current);
                page.Number = i;
                page.Url = this.UrlForPage(i, baseUrl, urlFormat);

                yield return page;
            }
        }

        private string UrlForPage(int page, string baseUrl, string format)
        {
            return (page == 1) ? baseUrl : String.Format(format, page);
        }
    }
}
