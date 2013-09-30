using System;

namespace TinySite.Models
{
    [Flags]
    public enum LoadDocumentFlags
    {
        None = 0x0,
        CleanUrls = 0x1,
        DateFromFileName = 0x2,
        DateInPath = 0x4,
    }
}
