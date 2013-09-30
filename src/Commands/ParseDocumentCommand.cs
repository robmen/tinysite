using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Models;

namespace TinySite.Commands
{
    public class ParseDocumentCommand
    {
        private static readonly Regex SmarterDateTime = new Regex(@"^\s*(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})([Tt@](?<hour>\d{1,2})[\:\.](?<minute>\d{1,2})([\:\.](?<second>\d{1,2}))?)?\s*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
        private static readonly Regex MetadataKeyValue = new Regex(@"^(?<key>\w+):\s?(?<value>.+)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public string DocumentPath { private get; set; }

        public string SummaryMarker { private get; set; }

        public string Content { get; private set; }

        public DateTime? Date { get; private set; }

        public bool Draft { get; private set; }

        public MetadataCollection Metadata { get; private set; }

        public void Execute()
        {
            this.Metadata = new MetadataCollection();

            string content;
            using (var reader = new StreamReader(this.DocumentPath))
            {
                var line = ParseMetadataHeader(reader);

                content = String.Concat(line, "\r\n", reader.ReadToEnd()).Trim();
            }

            //if (!String.IsNullOrEmpty(content) && !String.IsNullOrEmpty(this.SummaryMarker))
            //{
            //    content = this.ParseOutSummary(content);
            //}

            this.Content = content;
        }

        private string ParseMetadataHeader(StreamReader reader)
        {
            var preambleOpened = false;
            var preambleSkipped = false;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                // Eat any blank lines or comments at the top of the document or in the header.
                //
                if (String.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("//"))
                {
                    continue;
                }

                if (line.Equals("---"))
                {
                    if (preambleOpened || preambleSkipped)
                    {
                        line = String.Empty;
                        break;
                    }

                    preambleOpened = true;
                }
                else
                {
                    var match = MetadataKeyValue.Match(line);
                    if (match.Success)
                    {
                        if (!preambleOpened)
                        {
                            preambleSkipped = true;
                        }

                        string key = match.Groups[1].Value.ToLowerInvariant();
                        string value = match.Groups[2].Value.Trim();
                        switch (key)
                        {
                            case "date":
                                this.Date = this.ParseDateTimeSmarter(value);
                                break;

                            case "draft":
                            case "ignore":
                            case "ignored":
                                this.Draft = value.ToLowerInvariant().Equals("true");
                                break;

                            case "tag":
                            case "tags":
                                var tags = line.Substring(5).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(t => t.Trim()).Where(t => !String.IsNullOrEmpty(t)).ToArray();

                                this.Metadata.Add("tags", tags);
                                break;

                            default:
                                this.Metadata.Add(key, value);
                                break;
                        }
                    }
                    else if (!preambleOpened) // no preamble, non-matches mean we're done with the header.
                    {
                        break;
                    }
                }
            }

            return line;
        }

        private DateTime? ParseDateTimeSmarter(string value)
        {
            var match = SmarterDateTime.Match(value);

            if (match.Success)
            {
                var year = Convert.ToInt32(match.Groups[1].Value, 10);
                var month = Convert.ToInt32(match.Groups[2].Value, 10);
                var day = Convert.ToInt32(match.Groups[3].Value, 10);
                var hour = match.Groups[4].Success ? Convert.ToInt32(match.Groups[4].Value, 10) : 0;
                var minute = match.Groups[5].Success ? Convert.ToInt32(match.Groups[5].Value, 10) : 0;
                var second = match.Groups[6].Success ? Convert.ToInt32(match.Groups[6].Value, 10) : 0;

                return new DateTime(year, month, day, hour, minute, second);
            }

            return null;
        }
    }
}
