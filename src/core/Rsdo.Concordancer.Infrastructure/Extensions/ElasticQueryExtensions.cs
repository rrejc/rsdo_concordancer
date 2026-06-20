using System.Collections.Generic;
using System.Linq;
using OpenSearch.Client;
using Rsdo.Concordancer.Core.Extensions;

namespace Rsdo.Concordancer.Infrastructure.Extensions;

public static class ElasticQueryExtensions
{
    public static QueryContainer ToBooleanAndQuery(this List<QueryContainer> queries)
    {
        return queries.Count switch
        {
            0 => null,
            1 => queries[0],
            _ => new BoolQuery()
            {
                Must = queries,
            },
        };
    }

    public static QueryContainer ToBooleanOrQuery(this List<QueryContainer> queries)
    {
        return queries.Count switch
        {
            0 => null,
            1 => queries[0],
            _ => new BoolQuery()
            {
                Should = queries,
            },
        };
    }

    public static QueryContainer ToQuery(this string value, string field)
    {
        if (value.IsWildcardSearch())
        {
            return new WildcardQuery()
            {
                Field = field,
                Value = value,
            };
        }

        return new TermQuery()
        {
            Field = field,
            Value = value,
        };
    }

    public static QueryContainer ToQuery(this List<string> values, string field)
    {
        if (values.Any(v => v.IsWildcardSearch()))
        {
            return values.Select(value => value.ToQuery(field)).ToList().ToBooleanOrQuery();
        }

        return new TermsQuery()
        {
            Field = field,
            Terms = values,
        };
    }
}