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

                var pagedPosts = query.Results.Cast<DocumentFile>().ToList();

                var count = pagedPosts.Count();

                var pages = (count + query.PageEvery - 1) / query.PageEvery;

                var format = query.FormatUrl;

                var lastSlash = document.RelativeUrl.LastIndexOf('/');

                var documentRelativeUrl = document.RelativeUrl.Substring(0, lastSlash + 1);

                if (!String.IsNullOrEmpty(format))
                {
                    format = format.TrimStart('/');

                    var appendPathFormat = Path.Combine(Path.GetDirectoryName(document.OutputRelativePath), format.Replace('/', '\\'));

                    format = String.Concat(documentRelativeUrl, format.Replace('\\', '/').EnsureEndsWith("/"));

                    for (int i = 1; i < pages; ++i)
                    {
                        var dupe = document.Clone();

                        var appendPath = String.Format(appendPathFormat, i + 1);
                        var appendUrl = String.Format(format, i + 1);

                        this.UpdateOutputPaths(dupe, appendPath, appendUrl);

                        dupe["PageNumber"] = i + 1;
                        dupe.Paginator = this.CreatePaginator(i + 1, query.PageEvery, pages, documentRelativeUrl, format, pagedPosts);

                        dupes.Add(dupe);
                    }
                }

                document.Paginator = this.CreatePaginator(1, query.PageEvery, pages, documentRelativeUrl, format, pagedPosts);
            }

            this.PagedDocuments = dupes;
        }

        private void UpdateOutputPaths(DocumentFile document, string appendPath, string appendUrl)
        {
            var updateFileName = Path.GetFileName(document.OutputRelativePath);

            document.OutputRelativePath = Path.Combine(appendPath, updateFileName);

            document.OutputPath = Path.Combine(document.OutputRootPath, appendPath, updateFileName);

            document.RelativeUrl = String.Concat(appendUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);
        }

        private Paginator CreatePaginator(int page, int perPage, int pages, string baseUrl, string format, IEnumerable<DocumentFile> documents)
        {
            var paginator = new Paginator();

            // It is important that this query is not executed here (aka: do not add ToList() or ToArray()). This
            // query should be executed by the rendering engine so the returned documents are rendered first.
            paginator.Documents = documents.Skip((page - 1) * perPage).Take(perPage);

            if (pages > 1 && !String.IsNullOrEmpty(format))
            {
                paginator.Pagination = new Pagination();
                paginator.Pagination.Page = page;
                paginator.Pagination.PerPage = perPage;
                paginator.Pagination.TotalPage = pages;
                paginator.Pagination.NextPageUrl = page < pages ? this.UrlForPage(page + 1, baseUrl, format) : null;
                paginator.Pagination.PreviousPageUrl = page > 1 ? this.UrlForPage(page - 1, baseUrl, format) : null;

                var start = Math.Max(1, page - 3);
                var end = Math.Min(pages, start + 6);
                start = Math.Max(start, end - 6);

                paginator.Pagination.Pages = this.CreatePages(page, start, end, baseUrl, format).ToList();
            }

            return paginator;
        }

        private IEnumerable<Page> CreatePages(int current, int start, int end, string baseUrl, string format)
        {
            for (int i = start; i <= end; ++i)
            {
                var page = new Page();
                page.Active = (i == current);
                page.Number = i;
                page.Url = this.UrlForPage(i, baseUrl, format);

                yield return page;
            }
        }

        private string UrlForPage(int page, string baseUrl, string format)
        {
            return (page == 1) ? baseUrl : String.Format(format, page);
        }
    }
}
