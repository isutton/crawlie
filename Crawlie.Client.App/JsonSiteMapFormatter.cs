using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crawlie.Client
{
    public class JsonSiteMapFormatter : ISiteMapFormatter
    {
        public string Format(List<Uri> documentLinks)
        {
            var siteMap = new Dictionary<string, object>();
            
            foreach (var documentLink in documentLinks)
            {
                var segments = documentLink.Segments;

                var currentSiteMapNode = siteMap;

                foreach (var segment in segments)
                {
                    var cleanSegment = segment.TrimEnd('/');

                    if (string.IsNullOrWhiteSpace(cleanSegment)) continue;

                    if (currentSiteMapNode.TryGetValue(cleanSegment, out var existingSiteMapNode))
                    {
                        currentSiteMapNode = existingSiteMapNode as Dictionary<string, object>;
                    }
                    else
                    {
                        var newSiteMapNode = new Dictionary<string, object>();
                        currentSiteMapNode.Add(cleanSegment, newSiteMapNode);
                        currentSiteMapNode = newSiteMapNode;
                    }
                }
            }

            return JsonConvert.SerializeObject(siteMap, Formatting.Indented);
        }
    }
}