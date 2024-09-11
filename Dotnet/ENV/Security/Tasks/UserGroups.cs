
using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.Entities;
using ENV.Security.UI;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using ID = ENV.Security.Types.ID;

namespace ENV.Security.Tasks
{
    class UserGroups : UserManager.SecurityUIController
    {
        internal Entities.UserGroups _userGroups;
        internal readonly Entities.Groups _groups;
        internal readonly Entities.Groups _groupsForList;
        UserGroupsUI _form;
        HashSet<Text> _originalData = new HashSet<Text>();

        public UserGroups(ID parentID, AbstractUIController parentController,IEntityDataProvider db)
        {
            _userGroups = new ENV.Security.Entities.UserGroups(db);
            _groups = new Groups(db);
            _groupsForList = new Groups(db);
            UseWildcardForContainsInTextColumnFilter = true;
            MatchController(parentController);
            AllowDelete = parentController.AllowUpdate;
            _uiController.DatabaseErrorOccurred += new DatabaseErrorEventHandler(_uiController_DatabaseErrorOccurred);
            From = _userGroups;
            Relations.Add(_groups, _groups.ID.IsEqualTo(_userGroups.GroupID));
            OrderBy.Segments.Add(_groups.Description);

            AddAllColumns();
            _userGroups.ParentID.BindToParentID(parentID, _uiController);

            var userGroups = new Entities.UserGroups(db);
            userGroups.ForEachRow(userGroups.ParentID.IsEqualTo(parentID), () => _originalData.Add(userGroups.GroupID));

            Handlers.Add(GridForm.CancelCommand).Invokes += e =>
            {
                userGroups.Delete(userGroups.ParentID.IsEqualTo(_userGroups.ParentID));
                foreach (var groupId in _originalData)
                {
                    userGroups.Insert(() =>
                    {
                        userGroups.ParentID.Value = _userGroups.ParentID;
                        userGroups.GroupID.Value = groupId;
                    });
                }
            };

            View = () => _form = new UserGroupsUI(this);
        }

        void _uiController_DatabaseErrorOccurred(DatabaseErrorEventArgs e)
        {
            if (e.ErrorType == DatabaseErrorType.DuplicateIndex)
            {
                Common.ShowMessageBox("Error", System.Windows.Forms.MessageBoxIcon.Error, LocalizationInfo.Current.ThisGroupAlreayExists);

                e.HandlingStrategy = DatabaseErrorHandlingStrategy.RollbackAndRecover;
            }
        }
        public void Run()
        {
            Execute();
        }
    }
}
