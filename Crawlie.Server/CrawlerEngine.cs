using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace Crawlie.Server
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

        private static bool TryExtractHref(IElement element, out string href)
        {
            if (element is IHtmlAnchorElement anchorElement)
            {
                var cleanHref = anchorElement.Href.Replace(" ", "");
                var hasCorrectProtocol = anchorElement.Protocol == "http:" || anchorElement.Protocol == "https:";

                if (!string.IsNullOrWhiteSpace(cleanHref) && hasCorrectProtocol)
                {
                    href = cleanHref;
                    return true;
                }
            }

            href = "";
            return false;
        }
        
        public async Task<List<Uri>> ProcessDocument(string documentString, string baseUrl)
        {
            var document = await _context.OpenAsync(req => req.Content(documentString));

            var documentLinks = document
                .QuerySelectorAll("a")
                .SelectMany(e => 
                    TryExtractHref(e, out var href) 
                        ? new List<Uri> {new Uri(href)} 
                        : new List<Uri>())
                .Where(u => u.Host == baseUrl)
                .ToList();

            return documentLinks;
        }
    }
}