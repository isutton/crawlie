using System;
using System.Collections.Generic;

namespace Crawlie.Client
{
    public interface ISiteMapFormatter
    {
        string Format(List<Uri> documentLinks);
    }
}