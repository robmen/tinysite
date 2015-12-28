using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Models;
using TinySite.Models.Dynamic;
using TinySite.Models.Query;

namespace TinySite.Services
{
    public class QueryProcessor
    {
        private static readonly Regex Tokenize = new Regex(@"("".+?"")|[^\s]+", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public IEnumerable<string> Tokens { get; private set; }

        public static QueryResult Parse(Site site, string query)
        {
            var dynamicSite = new DynamicSite(null, site);

            var result = new QueryResult();

            // query documents every 10 where relativepath startswith "documents\posts\" descending date formaturl "posts/page/{0}"
            var tokens = ParseTokens(query).ToArray();

            for (int i = 0; i < tokens.Length; ++i)
            {
                var token = tokens[i].ToLowerInvariant();

                switch (token)
                {
                    case "query":
                        result.Source = ParseSource(dynamicSite, tokens, ref i);
                        break;

                    case "ascending":
                    case "descending":
                        result.Order = ParseOrder(tokens, ref i);
                        break;

                    case "every":
                        result.PageEvery = ParseEvery(tokens, ref i);
                        break;

                    case "formaturl":
                        result.FormatUrl = ParseFormatUrl(tokens, ref i);
                        break;

                    case "take":
                        result.Take = ParseTake(tokens, ref i);
                        break;

                    case "where":
                        result.Where = ParseWhere(tokens, ref i);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unknown query token: {0}", token));
                }
            }

            var q = result.Source.AsQueryable();

            if (result.Where != null)
            {
                q = q.Cast<IDictionary<string, object>>().Where(e => WhereQuery(result.Where, e));
            }

            if (result.Order != null)
            {
                if (result.Order.Operator == OrderOperator.Ascending)
                {
                    q = q.Cast<IDictionary<string, object>>().OrderBy(e => OrderProperty(result.Order, e));
                }
                else
                {
                    q = q.Cast<IDictionary<string, object>>().OrderByDescending(e => OrderProperty(result.Order, e));
                }
            }

            if (result.Take > 0)
            {
                q = q.Take(result.Take);
            }

            result.Results = q;

            return result;
        }

        private static object OrderProperty(OrderClause order, IDictionary<string, object> e)
        {
            var t = e[order.Property];

            return t;
        }

        private static bool WhereQuery(WhereClause where, IDictionary<string, object> e)
        {
            var result = false;

            var obj = e[where.Property];

            var value = ParseType(obj);

            if (where.Type == typeof(int))
            {
                var n = (int)where.Value;

                var number = value.Type == typeof(int) ? (int)value.Value : Convert.ToInt32(value.Value);

                switch (where.Operator)
                {
                    case WhereOperator.Equals:
                        result = (number == n);
                        break;

                    case WhereOperator.GreaterThan:
                        result = (number > n);
                        break;
                    case WhereOperator.LessThan:
                        result = (number < n);
                        break;

                    case WhereOperator.Contains:
                    case WhereOperator.EndsWith:
                    case WhereOperator.StartsWith:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                var s = (string)where.Value;

                var str = Convert.ToString(value.Value);

                switch (where.Operator)
                {
                    case WhereOperator.Contains:
                        result = -1 != str.IndexOf(s);
                        break;

                    case WhereOperator.Equals:
                        result = (str == s);
                        break;

                    case WhereOperator.EndsWith:
                        result = str.EndsWith(s);
                        break;

                    case WhereOperator.StartsWith:
                        result = str.StartsWith(s);
                        break;

                    case WhereOperator.GreaterThan:
                        result = (0 < str.CompareTo(s));
                        break;

                    case WhereOperator.LessThan:
                        result = (0 > str.CompareTo(s));
                        break;
                }
            }

            return result;
        }

        private static IEnumerable<string> ParseTokens(string query)
        {
            var index = 0;

            for (var m = Tokenize.Match(query); m.Success; m = Tokenize.Match(query, index))
            {
                yield return m.Value;

                index = m.Index + m.Length;
            }
        }

        private static IEnumerable<dynamic> ParseSource(DynamicSite site, string[] tokens, ref int i)
        {
            ++i;

            var source = tokens[i].ToLowerInvariant();

            switch (source)
            {
                case "data":
                    return site[source] as IEnumerable<dynamic>;

                case "documents":
                    return site[source] as IEnumerable<dynamic>;

                //case "files":
                //    return site.Files;

                //case "layouts":
                //    return site.Layouts;

                default:
                    throw new InvalidOperationException(String.Format("Unknown query source: {0}", source));
            }
        }

        private static int ParseEvery(string[] tokens, ref int i)
        {
            var every = 0;

            ++i;

            if (Int32.TryParse(tokens[i], out every))
            {
                return every;
            }

            return -1;
        }

        private static string ParseFormatUrl(string[] tokens, ref int i)
        {
            ++i;

            return tokens[i].Trim('"');
        }

        private static OrderClause ParseOrder(string[] tokens, ref int i)
        {
            var order = new OrderClause();

            var direction = tokens[i].ToLowerInvariant();

            switch (direction)
            {
                case "ascending":
                    order.Operator = OrderOperator.Ascending;
                    break;

                case "descending":
                    order.Operator = OrderOperator.Descending;
                    break;
            }

            ++i;

            order.Property = tokens[i];

            return order;
        }

        private static int ParseTake(string[] tokens, ref int i)
        {
            var take = 0;

            ++i;

            if (Int32.TryParse(tokens[i], out take))
            {
                return take;
            }

            return -1;
        }

        private static WhereClause ParseWhere(string[] tokens, ref int i)
        {
            var where = new WhereClause();

            ++i;

            where.Property = tokens[i];

            ++i;

            var op = tokens[i].ToLowerInvariant();

            switch (op)
            {
                case "contains":
                    where.Operator = WhereOperator.Contains;
                    break;

                case "eq":
                case "equals":
                    where.Operator = WhereOperator.Equals;
                    break;

                case "gt":
                case "greaterthan":
                    where.Operator = WhereOperator.GreaterThan;
                    break;

                case "lt":
                case "lessthan":
                    where.Operator = WhereOperator.LessThan;
                    break;

                case "endswith":
                    where.Operator = WhereOperator.EndsWith;
                    break;

                case "startswith":
                    where.Operator = WhereOperator.StartsWith;
                    break;
            }

            ++i;

            var type = ParseType(tokens[i]);

            where.Type = type.Type;

            where.Value = type.Value;

            return where;
        }

        private static ParsedType ParseType(object value)
        {
            var str = value as string;

            int number;

            if (value is int)
            {
                return new ParsedType()
                {
                    Type = typeof(int),
                    Value = (int)value,
                };
            }
            else if (Int32.TryParse(str, out number))
            {
                return new ParsedType()
                {
                    Type = typeof(int),
                    Value = number,
                };
            }
            else
            {
                return new ParsedType()
                {
                    Type = typeof(string),
                    Value = str.Trim('"'),
                };
            }
        }

        private struct ParsedType
        {
            public object Value { get; set; }

            public Type Type { get; set; }
        }
    }
}
