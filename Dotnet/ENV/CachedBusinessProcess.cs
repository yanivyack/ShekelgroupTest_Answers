using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV
{
    public class CachedBusinessProcess<taskType> : Firefly.Box.Flow.CachedTask<taskType>
        where taskType : BusinessProcessBase
    {
        public CachedBusinessProcess(Func<taskType> createInstance)
            : base(createInstance, x => x._businessProcess)
        {
        }
    }
}
