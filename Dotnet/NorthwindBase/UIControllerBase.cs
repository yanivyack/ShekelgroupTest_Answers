namespace Northwind
{
    /// <summary>UIControllerBase</summary>
    public abstract class UIControllerBase : ENV.UIControllerBase 
    {
        /// <summary>Application that will be used by all inheriting classes</summary>
        public readonly Application Application = Northwind.Application.Instance;
        public UIControllerBase()
        {
            setApplication(Application);
        }
    }
}
