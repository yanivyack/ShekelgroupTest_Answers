using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box;

namespace ENV
{
    public class CachedUIController<taskType>:Firefly.Box.Flow.CachedTask<taskType> 
        where taskType:class
    {
        public CachedUIController(Func<taskType> createInstance) : base(createInstance, x=>((AbstractUIController)(object)x)._uiController)
        {
        }
    }
    public class CachedController<taskType> : Firefly.Box.Flow.CachedTask<taskType>,ICachedController
        where taskType : class
    {
        Func<taskType> _createInstance;
        public CachedController(Func<taskType> createInstance)
            : base(createInstance, x =>
            {
                var cb = x as ControllerBase;
                if (cb != null)
                    return cb.GetITask();
                return null;

            })
        {
            _createInstance = createInstance;
        }
        public object GetInstance()
        {

            var result =  Instance;
            var bpb = result as BusinessProcessBase;
            if (bpb!=null &&bpb._inProcess)
                return _createInstance();
            return result;
        }
    }
    interface ICachedController
    {
        object GetInstance();
    }
    
}
