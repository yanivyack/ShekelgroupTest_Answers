using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data;
using Firefly.Box.Data.Advanced;

namespace ENV.Utilities
{
    public class ControlDataBinder
    {
        readonly AbstractUIController _controller;
        readonly TextColumn _column;
        readonly Func<string> _getControlData;


        public ControlDataBinder(System.Windows.Forms.Control control, TextColumn column, AbstractUIController controller, Func<string> getControlData, Action<string> setControlData)
        {
            _controller = controller;
            _column = column;
            _getControlData = getControlData;
            _controller.EnterRow += () => setControlData(column.Value.TrimEnd());
            _controller._uiController.LeaveRow += UpdateColumnWithControlValue;
            column.ValueChanged += t => setControlData(column.Value.TrimEnd());
            control.Leave += (a, b) => UpdateColumnWithControlValue();

        }
        public void UpdateColumnWithControlValue()
        {
            _column.Value = _getControlData();
        }
    }
}
