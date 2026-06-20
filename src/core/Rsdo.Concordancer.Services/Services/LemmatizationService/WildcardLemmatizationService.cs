using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rsdo.Concordancer.Core.Extensions;
using Rsdo.Concordancer.Services.Framework.DbContext;

namespace Rsdo.Concordancer.Services.Services.LemmatizationService;

public class WildcardLemmatizationService : SimpleLemmatizationService
{
    private readonly MasterDbContext dbContext;

    public WildcardLemmatizationService(MasterDbContext dbContext)
        : base(dbContext)
    {
        this.dbContext = dbContext;
    }

    public override async Task<List<string>> GetLemmas(string form)
    {
        if (!form.IsWildcardSearch())
        {
            return await base.GetLemmas(form);
        }

        var lemmas = await dbContext.LemmaFormPair.Where(f => EF.Functions.Like(f.Form.ToLower(), form.Replace("*", "%").Replace("?", "_").ToLower()))
            .Select(f => f.Lemma)
            .Distinct()
            .ToListAsync();
        if (lemmas.IsNullOrEmpty())
        {
            lemmas.Add(form);
        }

        return lemmas;
    }
}