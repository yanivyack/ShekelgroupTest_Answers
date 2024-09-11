using System;
using System.Windows.Forms;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.UI
{
    public class FormBuilder
    {
        FormView _form;

        public FormBuilder(string name)
        {
            _form = new FormView
            {
                Text = name,

            };

        }


        public void AddColumn(BoolColumn column)
        {
            _form.AddColumn(column);
        }
        public void AddColumn(ColumnBase column)
        {
            _form.AddColumn(column);
        }

        public void AddColumn(string name, Control displayControl)
        {

            _form.AddColumn(name, displayControl);
        }
        public void AddPassword(TextColumn column)
        {
            _form.AddPassword(column);
        }




        public bool Modal { get { return _form.Modal; } set { _form.Modal = value; } }

        public void AddAction(string name, Action what)
        {
            _form.AddAction(name, what);
        }

        public Firefly.Box.UI.Form Build()
        {
            return _form;
        }

        public void Run()
        {
            var uic = new UIController { View = Build() };
            uic.Run();
        }

        public void Close()
        {
            _form.Close();
        }
        public void AddCombo(TextColumn textColumn, Entity sourceEntity = null, TextColumn columnInEntity = null, FilterBase filter = null, string values = null)
        {
            _form.AddCombo(textColumn, sourceEntity, columnInEntity,null,null ,filter, values);
        }
    }
}