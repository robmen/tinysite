using System;

namespace TinySite.Models.Query
{
    public class WhereClause
    {
        public string Property { get; set; }

        public WhereOperator Operator { get; set; }

        public object Value { get; set; }

        public Type Type { get; set; }
    }
}
