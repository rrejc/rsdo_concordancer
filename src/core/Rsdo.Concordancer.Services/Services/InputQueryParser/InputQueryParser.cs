using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Rsdo.Concordancer.Core.Extensions;
using Rsdo.Concordancer.ServiceModel.Requests.Concordances;
using Rsdo.Concordancer.ServiceModel.Types;
using Rsdo.Concordancer.Services.Services.TokenizerService;

namespace Rsdo.Concordancer.Services.Services.InputQueryParser;

public class InputQueryParser : IInputQueryParser
{
    private readonly IIndex<TokenizerType, ITokenizerService> tokenizers;

    public InputQueryParser(IIndex<TokenizerType, ITokenizerService> tokenizers)
    {
        this.tokenizers = tokenizers;
    }

    public async Task<IEnumerable<(TokenType type, string form)>> Tokenize(string query)
    {
        if (query.IsWildcardSearch())
        {
            // Classla tokenizer doesn't support wildcards, so use default tokenizer
            return await tokenizers[TokenizerType.Default].Tokenize(query);
        }

        try
        {
            return await tokenizers[TokenizerType.Classla].Tokenize(query);
        }
        catch (Exception)
        {
            return await tokenizers[TokenizerType.Default].Tokenize(query);
        }
    }

    public async Task<(SearchedMainWord mainWord, List<SearchedWordInContext> wordsInContext)> Parse(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentNullException(query);
        }

        var parsed = await ParseDefault(query);

        var mainWord = new SearchedMainWord()
        {
            Form = parsed[0].word,
            FormSearchType = GetFormSearchType(parsed[0].inPhrase),
        };

        var wordsInContext = parsed.Skip(1)
            .Select(
                (w, i) => new SearchedWordInContext()
                {
                    ConditionType = ConditionType.Is,
                    Form = w.word,
                    FormSearchType = GetFormSearchType(w.inPhrase),
                    DistanceType = DistanceType.Position,
                    LeftPosition = 0,
                    RightPosition = i + 1,
                })
            .ToList();

        return (mainWord, wordsInContext);
    }

    public async Task<List<(string word, bool inPhrase)>> ParseDefault(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentNullException(query);
        }

        var result = new List<(string word, bool inPhrase)>();

        var buffer = new StringBuilder();
        var inPhrase = false;

        for (int i = 0; i < query.Length; i++)
        {
            var c = query[i];

            if (c != '"')
            {
                buffer.Append(c);
            }

            if (c == '"' || i == query.Length - 1)
            {
                var tokens = await Tokenize(buffer.ToString());
                foreach (var token in tokens)
                {
                    result.Add((token.form, inPhrase));
                }

                buffer.Clear();
                inPhrase = !inPhrase;
            }
        }

        return result;
    }

    private static FormSearchType GetFormSearchType(bool inPhrase)
    {
        return inPhrase ? FormSearchType.ExactForm : FormSearchType.AllForms;
    }
}