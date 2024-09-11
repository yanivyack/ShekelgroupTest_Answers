using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data
{
     class TimeSpanColumn : CustomColumnBase<TimeSpan>
    {


        public TimeSpanColumn(string name)
            : base(name, new TimeSpan())
        {
        }

        protected override int MaxLength
        {
            get { return 15; }
        }

        protected override TimeSpan DefaultValueWhenNullIsNotAllowed
        {
            get { return new TimeSpan(); }
        }

        protected override void SaveValueToDatabase(TimeSpan value, IValueSaver saver)
        {
            saver.SaveTimeSpan(value);
        }

        protected override TimeSpan LoadFromDatabase(IValueLoader loader)
        {
            return loader.GetTimeSpan();
        }

        protected override string ToString(TimeSpan value)
        {
            return value.ToString();
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        protected override TimeSpan Parse(string value)
        {
            return TimeSpan.Parse(value);
        }
    }
}
