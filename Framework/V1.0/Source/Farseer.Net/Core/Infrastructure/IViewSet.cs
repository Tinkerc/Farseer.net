using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FS.Core.Data;

namespace FS.Core.Infrastructure
{
    public interface IViewSet<TReturn>
    {
        Queue Queue { get; }
    }
}
