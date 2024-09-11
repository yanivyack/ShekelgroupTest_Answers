using ENV.Printing;
namespace Northwind.Shared.Printing
{
    public class Printers
    {
        static Printers()
        {
        }
        /// <summary>Printer1</summary>
        public static readonly Printer Printer1 = new Printer("Printer1");
    }
}
