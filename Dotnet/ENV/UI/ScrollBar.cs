using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.UI
{
    public class ScrollBar:Firefly.Box.UI.ScrollBar
    {
        public bool AutoExpand { get; set; }

        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        public AfterExpandGoToNextControlOptions AfterExpandGoToNextControl { get; set; }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public void ClearExpandEvent()
        {
            
        }
    }
}
