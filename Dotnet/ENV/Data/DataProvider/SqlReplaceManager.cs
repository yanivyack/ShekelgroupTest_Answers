using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using ENV.Data;
using ENV;

namespace ENV.Data.DataProvider
{
    public class SqlReplaceManager : UIControllerBase
    {
        public SQLReplace s = new SQLReplace();
        public TextColumn Tab = new TextColumn("Tab"){Value="Template"};
        public BoolColumn Error = new BoolColumn("Error"), Changed = new BoolColumn("Changed");

        public SqlReplaceManager()
        {
            From = s;
            OrderBy.Add(s.Id, SortDirection.Descending);
            Activity = Activities.Browse;
     
            Columns.Add(s.OriginalSQLTemplate);
            Columns.Add(s.OriginalSQLTemplateKey).BindValue(()=>s.OriginalSQLTemplate.Value.Remove(SQLReplaceBuilder.keySize));
            foreach (var v in new[]{Error,Changed,s.Refresh} )
            {
                v.InputRange = "Y,";
            }
            AddAllColumns();
            Error.BindValue(() => s.LastError != "");
            Changed.BindValue(() => s.TargetSQLTemplate != "");
        }

        public void Run()
        {
            Execute();
        }

        protected override void OnLoad()
        {
            View = () => new Views.SqlReplaceManagerView(this);
        }

        protected override void OnEnterRow()
        {
            if (Activity == Activities.Insert)
                s.Id.SilentSet(s.Max(s.Id) + 1);
        }

        public void ShowMerged()
        {
            SaveRowAndDo(o=>  s.ShowMergedSQL());
        }
        public void RunSQL()
        {
            SaveRowAndDo(o=>s.RunSQL());
        }
    }
}