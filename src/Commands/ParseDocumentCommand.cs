using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TinySite.Extensions;
using TinySite.Models;

namespace TinySite.Commands
{
    public class ParseDocumentCommand
    {
        private static readonly Regex SmarterDateTime = new Regex(@"^\s*(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})([Tt@](?<hour>\d{1,2})[\:\.](?<minute>\d{1,2})([\:\.](?<second>\d{1,2}))?)?\s*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
        private static readonly Regex MetadataKeyValue = new Regex(@"^(?<key>\w+\??):\s?(?<value>.+)?$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public ParseDocumentCommand(string documentPath)
        {
            this.DocumentPath = documentPath;
        }

        public string Content { get; private set; }

        public DateTime? Date { get; private set; }

        public bool Draft { get; private set; }

        public MetadataCollection Metadata { get; private set; }

        public IDictionary<string, string> Queries { get; private set; }

        private string DocumentPath { get; }

        public void Execute()
        {
            this.Metadata = new MetadataCollection();
            this.Queries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var content = String.Empty;
            var retry = 0;

            do
            {
                try
                {
                    using (var reader = new StreamReader(this.DocumentPath))
                    {
                        content = reader.ReadToEnd();
                    }

                    retry = 0;
                }
                catch (IOException)
                {
                    if (retry < 3)
                    {
                        Thread.Sleep(500);
                        ++retry;
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (retry > 0);

            content = this.ParseMetadataHeaderFromContent(content);

            this.Content = content;
        }

        private string ParseMetadataHeaderFromContent(string content)
        {
            var startOfLine = 0;
            var endOfLine = -1;
            var preambleOpened = false;
            var preambleSkipped = false;

            while ((endOfLine = content.IndexOf("\n", startOfLine)) > 0)
            {
                var line = content.Substring(startOfLine, endOfLine - startOfLine + 1).TrimEnd();

                // Eat any blank lines or comments at the top of the document or in the header.
                //
                if (String.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("//"))
                {
                    startOfLine = endOfLine + 1;

                    continue;
                }

                // If start or end of header.
                if (line.StartsWith("---"))
                {
                    startOfLine = endOfLine + 1;

                    if (preambleOpened || preambleSkipped)
                    {
                        // Eat any blank lines after the preamble.
                        while ((endOfLine = content.IndexOf("\n", startOfLine)) > 0)
                        {
                            line = content.Substring(startOfLine, endOfLine - startOfLine + 1).TrimEnd();

                            if (!String.IsNullOrEmpty(line))
                            {
                                break;
                            }

                            startOfLine = endOfLine + 1;
                        }

                        break;
                    }

                    preambleOpened = true;
                }
                else // try to parse for metadata.
                {
                    var match = MetadataKeyValue.Match(line);
                    if (match.Success)
                    {
                        if (!preambleOpened)
                        {
                            preambleSkipped = true;
                        }

                        dynamic complexValue;

                        var key = match.Groups[1].Value.ToLowerInvariant();
                        var value = match.Groups[2].Value.Trim();
                        switch (key)
                        {
                            case "date":
                                this.Date = ParseDateTimeSmarter(value);
                                break;

                            case "draft":
                            case "ignore":
                            case "ignored":
                                this.Draft = ParseBoolen(value);
                                break;

                            case "tag":
                            case "tags":
                                var tags = ParseArray(value);
                                this.Metadata.Add("tags", tags);
                                break;

                            default:
                                if (TryParseComplexValue(value, content, startOfLine + match.Groups[2].Index, ref endOfLine, out complexValue))
                                {
                                    this.Metadata.Add(key, complexValue);
                                }
                                else
                                {
                                    this.HandleDefaultMetdata(key, value);
                                }
                                break;
                        }
                    }
                    else if (!preambleOpened) // no preamble and not metadata means we're done with the header.
                    {
                        break;
                    }

                    startOfLine = endOfLine + 1;
                }
            }

            return content.Substring(startOfLine).TrimEnd();
        }

        private static string[] ParseArray(string value)
        {
            return value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !String.IsNullOrEmpty(t))
                .ToArray();
        }

        private static bool ParseBoolen(string value)
        {
            return (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase));
        }

        private static DateTime? ParseDateTimeSmarter(string value)
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

            DateTime dateTime;
            return DateTime.TryParse(value, out dateTime) ? (DateTime?)dateTime : null;
        }

        private static bool TryParseComplexValue(string value, string content, int index, ref int endOfLine, out dynamic complexValue)
        {
            complexValue = null;

            if (value.StartsWith("[") || value.StartsWith("{"))
            {
                var openCharacter = value[0];
                var closeCharacter = (openCharacter == '[') ? ']' : '}';
                var countOpenCharacters = 1;

                var startJsonIndex = index;

                for (index = index + 1; countOpenCharacters > 0 && index < content.Length; ++index)
                {
                    var c = content[index];

                    // TODO: take into account string content and do not count brackets/braces
                    // inside strings.
                    if (c == openCharacter)
                    {
                        ++countOpenCharacters;
                    }
                    else if (c == closeCharacter)
                    {
                        --countOpenCharacters;
                    }
                }

                if (countOpenCharacters == 0)
                {
                    var json = content.Substring(startJsonIndex, index - startJsonIndex);

                    complexValue = CaseInsensitiveExpando.FromJson(json);

                    endOfLine = content.IndexOf('\n', index + 1);

                    if (endOfLine < 0)
                    {
                        endOfLine = content.Length;
                    }
                }
            }

            return complexValue != null;
        }

        private void HandleDefaultMetdata(string key, string value)
        {
            if (key.EndsWith("?"))
            {
                this.Queries.Add(key.TrimEnd('?'), value);
            }
            else if (key.EndsWith("date", StringComparison.OrdinalIgnoreCase))
            {
                var date = ParseDateTimeSmarter(value);
                if (date.HasValue)
                {
                    this.Metadata.Add(key,  date);
                }
                else
                {
                    this.Metadata.Add(key,  value);
                }
            }
            else
            {
                this.Metadata.Add(key, value);
            }
        }
    }
}
