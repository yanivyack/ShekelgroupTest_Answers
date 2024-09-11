namespace Northwind
{
    /// <summary>BusinessProcessBase</summary>
    public abstract class BusinessProcessBase : ENV.BusinessProcessBase 
    {
        /// <summary>Application that will be used by all inheriting classes</summary>
        public readonly Application Application = Northwind.Application.Instance;
        public BusinessProcessBase()
        {
            setApplication(Application);
        }
    }
}
