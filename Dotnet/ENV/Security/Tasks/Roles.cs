using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.UI;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using ID = ENV.Security.Types.ID;

namespace ENV.Security.Tasks
{
    class Roles : UserManager.SecurityUIController
    {
        internal Entities.Roles _roles ;
        internal readonly Entities.AvailableRoles AvailableRoles = new Entities.AvailableRoles();
        RolesUI _form;
        HashSet<Text> _originalData = new HashSet<Text>();

        public Roles(ID parentID, AbstractUIController parentController,IEntityDataProvider db)
        {
            _roles = new Entities.Roles(db);
            UseWildcardForContainsInTextColumnFilter = true;
            MatchController(parentController);
            AllowDelete = parentController.AllowUpdate;
            if (!AllowUpdate)
                _roles.Role.ClearExpandEvent();
            _uiController.DatabaseErrorOccurred += new DatabaseErrorEventHandler(_uiController_DatabaseErrorOccurred);
            From = _roles;
            OrderBy.Segments.Add(_roles.Role);
            Relations.Add(AvailableRoles, AvailableRoles.Role.IsEqualTo(() => u.Upper(_roles.Role)));
            AddAllColumns();
            _roles.ParentID.BindToParentID(parentID, _uiController);

            var roles = new Entities.Roles(db);
            roles.ForEachRow(roles.ParentID.IsEqualTo(parentID), () => _originalData.Add(roles.Role));

            Handlers.Add(GridForm.CancelCommand).Invokes += e =>
            {
                roles.Delete(roles.ParentID.IsEqualTo(_roles.ParentID));
                foreach (var role in _originalData)
                {
                    roles.Insert(() =>
                    {
                        roles.ParentID.Value = _roles.ParentID;
                        roles.Role.Value = role;
                    });
                }

            };

            View = () => _form = new RolesUI(this);
        }



        void _uiController_DatabaseErrorOccurred(DatabaseErrorEventArgs e)
        {

            if (e.ErrorType == DatabaseErrorType.DuplicateIndex)
            {
                e.Handled = true;
                Common.ShowMessageBox("Error", System.Windows.Forms.MessageBoxIcon.Error, LocalizationInfo.Current.ThisRightAlreadyExists);
                e.HandlingStrategy = DatabaseErrorHandlingStrategy.RollbackAndRecover;
            }
        }
        public void Run()
        {
            Execute();
        }
    }
}
