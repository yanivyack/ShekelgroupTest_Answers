using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ENV.IO
{
    public class DummyIO : IDisposable
    {

        public DummyIO()
        {
        }
        public DummyIO(params object[] args)
        {
        }

        public  void Dispose()
        {
            
        }
        public string Name { get; set; }
    }
}
