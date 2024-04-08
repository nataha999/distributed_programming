using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Valuator.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IRedisStorage _storage;

    public SummaryModel(ILogger<SummaryModel> logger, IRedisStorage storage)
    {
        _logger = logger;
        _storage = storage;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        //TODO: проинициализировать свойства Rank и Similarity значениями из БД
        Rank = Convert.ToDouble(_storage.Get("RANK-" + id));
        Similarity = Convert.ToDouble(_storage.Get("SIMILARITY-" + id));
    }
}