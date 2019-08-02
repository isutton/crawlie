using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;

namespace Crawlie.Server.IntegrationTests
{
    public interface ICrawlerEngine
    {
        Task<List<Uri>> ProcessDocument(string documentString, string baseUrl);
    }

    public class CrawlerEngine : ICrawlerEngine
    {
        private readonly IBrowsingContext _context;

        public CrawlerEngine()
        {
            _context = BrowsingContext.New(Configuration.Default);
        }

        public async Task<List<Uri>> ProcessDocument(string documentString, string baseUrl)
        {
            var document = await _context.OpenAsync(req => req.Content(documentString));

            var documentLinks = document
                .QuerySelectorAll("a")
                .SelectMany(e => e is IHtmlAnchorElement anchorElement
                    ? new List<Uri> {new Uri(anchorElement.Href)}
                    : new List<Uri>());

            return documentLinks.Where(u => u.Host == baseUrl).ToList();
        }
    }
}