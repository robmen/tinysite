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

### Query language

A metadata's value may be a query using the following syntax:

    metaname?: query documents where relativepath startswith "documents\posts\" descending date

The `pagination` metadata is a special query that does not need the `?` token to indicate it is a query:

    paginate: [documents] [every 4] [where relativepath startswith "documents\posts\"] [descending date] [formaturl "posts/page/{0}"]
                           take                         endswith                        ascending
                                                        equals
                                                        greaterthan
                                                        lessthan
                                                        contains
