using System;

namespace TinySite.Models
{
    [Flags]
    public enum LoadDocumentFlags
    {
        None = 0x0,
        CleanUrls = 0x1,
        DateFromFileName = 0x2,
        InsertDateIntoPath = 0x4,
        OrderFromFileName = 0x8,
        SanitizePath = 0x10,
    }
}
