using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using FS.Core.Infrastructure;
using FS.Extends;
using FS.Mapping.Context;

namespace FS.Core.Data.View
{
    public class ViewQueueManger : BaseQueueManger
    {
        public ViewQueueManger(DbExecutor database, ContextMap contextMap)
            : base(database, contextMap) { }
    }
}
