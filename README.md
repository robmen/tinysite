# tinySite

tinySite is a lightweight static site generator.

tinySite loads a standard file structure that looks like the following:

    /documents/ - files processed before being written to output
    /files/ - files copied as is to output
    /layouts/ - files referenced by documents to format
    /site.json - json configuration file

By default, tinySite writes the processed output to the `/build/` folder.

These folders are configurable in the site.json as such:

    {
      "documents" : "documents/",
      "files" : "files/",
      "layouts" : "layouts/",
      "output" : "build/",
    }


### Future Considerations

Add a query language that might look something like:

    paginate: ?query documents every 10 where relativepath startswith "documents\posts\" descending date formaturl "posts/page/{0}"

    paginate: query documents every 10 where relativepath startswith "documents\posts\" descending date pagedurl "posts/page/{0}"

    paginate: query documents every 10 where relativepath startswith "documents\posts\" descending date formaturl "posts/page/{0}"

    paginate: [documents] [every 4] [where relativepath startswith "documents\posts\"] [descending date] [pageurl "posts/page/{0}"]
                           take                         endswith                        ascending
                                                        equals
                                                        greaterthan
                                                        lessthan
                                                        contains
