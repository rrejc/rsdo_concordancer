using System.Collections.Generic;
using OpenSearch.Client;
using Rsdo.Concordancer.Core.Extensions;
using Rsdo.Concordancer.Core.Search.Queries.TermLists;
using Rsdo.Concordancer.Infrastructure.Extensions;

namespace Rsdo.Concordancer.Infrastructure.Search.QueryBuilders;

public class TermListQueryBuilder : IQueryBuilder<TermListQuery>
{
    public QueryContainer Build(TermListQuery query)
    {
        var queries = new List<QueryContainer>();

        // Search for words in all possible positions
        for (var i = 0; i <= 5 - query.Words.Count; i++)
        {
            var termQuery = new List<QueryContainer>();
            for (var j = 0; j < query.Words.Count; j++)
            {
                var word = query.Words[j];
                var tokenField = GetTokenField(i + j);
                termQuery.Add(GetWordEsQuery(word, tokenField));
            }

            queries.Add(termQuery.ToBooleanAndQuery());
        }

        return queries.ToBooleanOrQuery();
    }

    private static QueryContainer GetWordEsQuery(SearchedTermQuery wordQuery, string tokenField)
    {
        var queries = new List<QueryContainer>();

        if (!string.IsNullOrEmpty(wordQuery.Form))
        {
            queries.Add(wordQuery.Form.ToLower().ToQuery($"{tokenField}.formLower"));
        }

        if (!wordQuery.Lemmas.IsNullOrEmpty())
        {
            queries.Add(wordQuery.Lemmas.ToQuery($"{tokenField}.lemma"));
        }

        return queries.Count == 0 ? new MatchNoneQuery() : queries.ToBooleanAndQuery();
    }

    private static string GetTokenField(int position)
    {
        return position switch
        {
            > 0 => $"tokenRight{position}",
            _ => "token",
        };
    }
}