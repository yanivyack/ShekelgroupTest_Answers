namespace Northwind
{
    /// <summary>FlowUIControllerBase</summary>
    public abstract class FlowUIControllerBase : ENV.FlowUIControllerBase 
    {
        /// <summary>Application that will be used by all inheriting classes</summary>
        public readonly Application Application = Northwind.Application.Instance;
        public FlowUIControllerBase()
        {
            setApplication(Application);
        }
    }
}
