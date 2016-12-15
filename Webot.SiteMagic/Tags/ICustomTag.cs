using System;
using System.Collections.Generic;
using System.Text;

namespace Webot.SiteMagic
{
    public interface ICustomTag
    {
        string GetDefined(string tagDef);

        bool ContainTags();
    }
}
