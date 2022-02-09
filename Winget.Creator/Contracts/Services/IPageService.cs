using System;

namespace Winget.Creator.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
