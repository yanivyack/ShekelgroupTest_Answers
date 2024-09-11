using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.Advanced;

using ENV.Data;
using ENV;
using ENV.Utilities;

namespace ENV.UI
{
    class V8CompatibleSort : FlowUIControllerBase
    {
        DataSetDataProvider db = new DataSetDataProvider();
        public SelectedColumns _sc;
        public OptionalColumn _oc ;
        

        public V8CompatibleSort()
        {
            _sc = new SelectedColumns(db);
            _oc = new OptionalColumn(db);
            _sc.Insert(() =>
            {
                _sc.Order.Value = 0;
                _sc.Letter.Value = "";
            });
            _oc.Insert(() =>
            {
                _oc.Letter.Value = "";
                _oc.Order.Value = 0;
                _oc.Description.Value = "";
            });

            _oc.Insert(() =>
            {
                _oc.Letter.Value = "A";
                _oc.Order.Value = 1;
                _oc.Description.Value = "Noam";
            });
            _oc.Insert(() =>
            {
                _oc.Letter.Value = "B";
                _oc.Order.Value = 2;
                _oc.Description.Value = "Yael";
            });

            Handlers.Add(Commands.TemplateOk).Invokes += e => Ok();
            Handlers.Add(System.Windows.Forms.Keys.F10).Invokes += e => Raise(Commands.TemplateOk);

            From = _sc;
            _sc.Order.BindValue(() => _sc.CountRows());
            Relations.Add(_oc, _oc.Letter.IsEqualTo(_sc.Letter));

            Columns.Add(_sc.Order);
            Flow.Add<V8CompatibleSortSelectColumn>(c => c.Run(_sc.Letter), Firefly.Box.Flow.FlowMode.ExpandBefore);
            Columns.Add(_sc.Letter);
            Columns.Add(_oc.Description);


        }
        string _title;
        protected override void OnStart()
        {
            Commands.TemplateOk.Enabled = true;
            Commands.TemplateExit.Enabled = true;
        }

        public void Run(SelectOrderByDialog.SelectOrderByParent uic, Action<bool> reloadData)
        {
            
            db.DataSet.Clear();
            _title = LocalizationInfo.Current.Sort + ": " + uic.Title;
            var sc = new SelectedColumns(db);
            var oc = new OptionalColumn(db);
            var dic = new Dictionary<string, ColumnBase>();
            sc.Insert(() =>
            {
                sc.Order.Value = 0;
                sc.Letter.Value = "";
            });
            oc.Insert(() =>
            {
                oc.Letter.Value = "";
                oc.Order.Value = 0;
                oc.Description.Value = "";
            });

            int i = ENV.UserMethods.Instance.IndexOf(uic.Columns[0]);
            foreach (var item in uic.Columns)
            {
                oc.Insert(() =>
                {
                    oc.Letter.Value = EvaluateExpressions.ConvertNumberToLetters(i ).ToString();
                    dic.Add(oc.Letter.Value.Trim(), item);
                    oc.Order.Value = i;
                    oc.Description.Value = item.Caption;
                    if (item.Entity != null)
                        oc.FromFile.Value = item.Entity.Caption;
                });
                i++;
            }
            Execute();
            if (_ok)
            {
                var s = new Sort();
                var bp = new BusinessProcess { From = sc };
                bp.OrderBy.Add(sc.Order);
                bp.ForEachRow(() =>
                {
                    ColumnBase c;
                    if (dic.TryGetValue(sc.Letter.Trim().ToString(), out c))
                        s.Add(c);
                });
                if (s.Segments.Count > 0)
                {
                    uic.OrderBy = s;
                    reloadData(true);
                }
            }
            Commands.TemplateOk.Enabled = false;
            Commands.TemplateExit.Enabled = false;
        }
        bool _ok = false;
        internal void Ok()
        {
            _ok = true;
            Exit();
        }

        protected override void OnEnterRow()
        {
            Cached<V8CompatibleSortSelectColumn>().Run();
        }

        protected override void OnLoad()
        {
            View = () => new V8CompatibleSortView(this) { Text = _title };
            //BindAllowDelete(() => sc.Order > 0);
            AllowInsertInUpdateActivity = true;

        }
        protected override void OnSavingRow()
        {

        }
        internal class V8CompatibleSortSelectColumn : UIControllerBase
        {

            public OptionalColumn s;

            public V8CompatibleSortSelectColumn(V8CompatibleSort parent)
            {
                s = new OptionalColumn(parent.db);
                From = s;
            }


            LetterType _select;
            public void Run(LetterType select)
            {
                _select = select;
                Exit(ExitTiming.BeforeRow, () => false);
                Execute();
            }
            public void Run()
            {

                Exit(ExitTiming.BeforeRow, () => true);
                Execute();
            }
            protected override void OnSavingRow()
            {
                _select.Value = s.Letter.Value;
            }


            protected override void OnLoad()
            {
                View = () => new V8CompatibleSortSelectColumns(this);
                AllowUpdate = false;
                AllowInsert = false;
                AllowDelete = false;
                AllowSelect = true;
                Activity = Activities.Browse;
                SwitchToInsertWhenNoRows = true;
                KeepViewVisibleAfterExit = true;
            }
        }
        internal class LetterType : TextColumn
        {
            public LetterType() : base("שדה", "3U")
            { }
        }
        internal class OptionalColumn : ENV.Data.Entity


        {
             
            [PrimaryKey]
            public LetterType Letter = new LetterType();
            public NumberColumn Order = new NumberColumn("Order", "5");
            public TextColumn Description = new TextColumn("Description", "30H");
            public TextColumn FromFile = new TextColumn("File", "30H");

          
            public OptionalColumn(IEntityDataProvider dp) : base("OptionalColumns", dp)
            {
            }
        }
        internal class SelectedColumns : Entity
        {

            public LetterType Letter = new LetterType();
            [PrimaryKey]
            public NumberColumn Order = new NumberColumn("Order", "5Z", "#");
            
            public SelectedColumns(IEntityDataProvider dp) : base("selectedColumns", dp)
            {
            }
        }
    }

}