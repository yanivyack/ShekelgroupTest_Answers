using ENV.IO;
using ENV.Printing;
using ENV;
namespace Northwind.Shared
{
    public static class DebugHelper
    {
        #region Debug Setup
        public static void Init()
        {
            if (_inited)
                return;
            _inited = true;
            
            #if !DEBUG
            return;
            #endif
            ;
            ENV.Advanced.HandlerCollectionWrapper.BeforeHandler += HandlerInvokes;
            ControllerBase.OnProcessingCommand += ProcessingCommand;
            ControllerBase.OnRaise += Raise;
            ControllerBase.BeforeExecute += ControllerExecute;
            ReportSection.BeforeWrite += SectionWrite;
            TextSection.BeforeWrite += SectionWrite;
            TextTemplate.BeforeWrite += SectionWrite;
            TextSection.BeforeRead += SectionRead;
        }
        #endregion
        static void HandlerInvokes(object handler)
        {
            // Add break point to break before handler execution. Press F11 to go to handler code
        }
        static void ProcessingCommand(object controller, Firefly.Box.Command command)
        {
            // Add break point to break when a controller processes a command
        }
        static void Raise(object command)
        {
            // Add break point here to break whenever anything is raised
        }
        static void ControllerExecute(ControllerBase controller)
        {
            // Add break point to break on all controller execution
        }
        static void SectionWrite()
        {
            // Add break point to break on all section write to printer or file
        }
        static void SectionRead()
        {
            // Add break point to break on all section read from file
        }
        static bool _inited;
    }
}
