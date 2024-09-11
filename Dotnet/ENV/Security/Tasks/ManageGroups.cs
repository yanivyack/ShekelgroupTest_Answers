using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Tasks
{
    class ManageGroups : UserManager.SecurityUIController
    {
        internal Entities.Groups _groups;
        internal Firefly.Box.Data.TextColumn _cancel = new Firefly.Box.Data.TextColumn(), _roles = new Firefly.Box.Data.TextColumn();
        internal readonly ENV.Data. NumberColumn RoleCount = new ENV.Data. NumberColumn() { OnChangeMarkRowAsChanged = false };
        ManageGroupsUI _form;
        internal IEntityDataProvider _db;
        public ManageGroups(IEntityDataProvider db)
        {
            _db = db;
            _groups = new ENV.Security.Entities.Groups(db);
            UseWildcardForContainsInTextColumnFilter = true;
            From = _groups;
            OrderBy.Segments.Add(_groups.Description); 
            AddAllColumns();
            Columns.Add(_cancel, _roles);
            _groups.ID.AddNewRowBehaviourTo(_uiController);
            RoleCount.BindValue(() => _groups.CountRoles(db));


            View = () => _form = new ManageGroupsUI(this); 
        }

        protected override void OnSavingRow()
        {
            var e = new Entities.Groups(_db);
            var bp = new BusinessProcess { From = e };
            bp.Where.Add(e.Description.IsEqualTo(_groups.Description));
            bp.Where.Add(() => e.ID != _groups.ID);
            bp.Run();
            if (bp.Counter > 0)
                Message.ShowError(LocalizationInfo.Current.GroupAlreadyExists);
        }

        public void ResetStatistics()
        {
            RoleCount.Value = _groups.CountRoles(_db);

        }
    }
}
