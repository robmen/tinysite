using System;
using System.Collections.Generic;
using System.Linq;
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

                if (!String.IsNullOrEmpty(format))
                {
                    for (int i = 1; i < pages; ++i)
                    {
                        var dupe = document.Clone();

                        dupe.UpdateOutputPaths(String.Format(format, i + 1), null);

                        dupe.Paginator = this.CreatePaginator(i + 1, query.PageEvery, pages, format, pagedPosts);

                        dupes.Add(dupe);
                    }
                }

                document.Paginator = this.CreatePaginator(1, query.PageEvery, pages, format, pagedPosts);
            }

            this.PagedDocuments = dupes;
        }

        private Paginator CreatePaginator(int page, int perPage, int pages, string format, IEnumerable<DocumentFile> documents)
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
                paginator.Pagination.NextPageUrl = page < pages ? this.UrlForPage(page + 1, format) : null;
                paginator.Pagination.PreviousPageUrl = page > 1 ? this.UrlForPage(page - 1, format) : null;

                var start = Math.Max(1, page - 3);
                var end = Math.Min(pages, start + 6);
                start = Math.Max(start, end - 6);

                paginator.Pagination.Pages = this.CreatePages(page, start, end, format).ToList();
            }

            return paginator;
        }

        private IEnumerable<Page> CreatePages(int current, int start, int end, string format)
        {
            for (int i = start; i <= end; ++i)
            {
                var page = new Page();
                page.Active = (i == current);
                page.Number = i;
                page.Url = this.UrlForPage(i, format);

                yield return page;
            }
        }

        private string UrlForPage(int page, string format)
        {
            return (page == 1) ? this.RootUrl : String.Concat(this.RootUrl, String.Format(format, page));
        }
    }
}
