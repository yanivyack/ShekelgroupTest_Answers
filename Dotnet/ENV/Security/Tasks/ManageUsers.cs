using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ENV.Data;
using ENV.Security.Entities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Tasks
{
    class ManageUsers : UserManager.SecurityUIController
    {
        internal Entities.Users _users;

        internal Firefly.Box.Data.TextColumn _cancel = new Firefly.Box.Data.TextColumn(),
            _roles = new Firefly.Box.Data.TextColumn(),
            _groups = new Firefly.Box.Data.TextColumn(),
            _import = new Firefly.Box.Data.TextColumn();
        internal readonly NumberColumn RoleCount = new NumberColumn(){OnChangeMarkRowAsChanged = false};
        internal readonly NumberColumn GroupCount = new NumberColumn(){OnChangeMarkRowAsChanged = false};
        ManageUsersUI _form;
        internal IEntityDataProvider _db;
        UserManager.UserStorage _storage;
        public ManageUsers(IEntityDataProvider db,UserManager.UserStorage storage)
        {
            _storage = storage;
            _db = db;
            _users = new Users(db);
            
            UseWildcardForContainsInTextColumnFilter = true;
            Activity = Activities.Browse;
            SwitchToInsertWhenNoRows = true;
            From = _users;
            OrderBy.Segments.Add(_users.UserName);
            AddAllColumns();
            Columns.Add(RoleCount, GroupCount);
            RoleCount.BindValue(() => _users.CountRoles(db));
            GroupCount.BindValue(() => _users.CountGroups(db));

            Columns.Add(_cancel, _roles, _groups, _import);
            _users.ID.AddNewRowBehaviourTo(_uiController);

            
            


            View = () => _form = new ManageUsersUI(this);
            


            BindAllowDelete(() => UserManager.CanDelete(_users.UserName));
            _uiController.ReloadDataAfterUpdatingOrderByColumns = false;
        }
        protected override void OnSavingRow()
        {
            var e = new Entities.Users(_db);
            var bp = new BusinessProcess { From = e };
            bp.Where.Add(e.UserName.IsEqualTo(_users.UserName));
            bp.Where.Add(() => e.ID != _users.ID);
            bp.Run();
            if (bp.Counter > 0)
                Message.ShowError(LocalizationInfo.Current.UserAlreadyExists);
            
        }

      
      

        public void DoImport()
        {
            _uiController.SaveRowAndDo(x =>
            {
                var path = u.FileDlg("User Dump File", "*.xml");
                if (path != string.Empty)
                {
                    try
                    {
                        Security.UserManager.LoadFromMagicDump(path,_storage);
                    }
                    catch (Exception e)
                    {
                        Common.ShowExceptionDialog(e, true, "Import of user dump file");

                    }
                }
                x.ReloadData();
            });
        }

        internal void CahangePassword()
        {
            if (_users.UserName != Text.Empty)
                this.SaveRowAndDo(options =>
                {
                    new ChangePassword(_db, _users.UserName).RunTheTask();
                    options.ReloadData();
                });
        }

        public void ResetStatistics()
        {
            RoleCount.Value = _users.CountRoles(_db);
            GroupCount.Value = _users.CountGroups(_db);
        }

        public void ShowUserRoles()
        {
            SaveRowAndDo(_ =>
            {
                new Roles(_users.ID, this, _db).Run();
                ResetStatistics();
            });
        }

        public void ShowUserGroups()
        {
            SaveRowAndDo(_ =>
            {
                new UserGroups(_users.ID, this, _db).Run();
                ResetStatistics();
            });
        }
    }
}
