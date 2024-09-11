using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.UI;
using Firefly.Box;

namespace ENV.Security.Tasks
{
    class SelectRole : UserManager.SecurityUIController
    {
        SelectRoleUI _form;
        internal Entities.AvailableRoles _roles = new ENV.Security.Entities.AvailableRoles();
        public SelectRole()
        {
            UseWildcardForContainsInTextColumnFilter = true;

            View = ()=>_form=new SelectRoleUI(this);
            OrderBy.Segments.Add(_roles.Description);
            From = _roles;
            AddAllColumns();
            Where.Add(_roles.Public.IsEqualTo(true));

            Activity = Firefly.Box.Activities.Browse;
            AllowUpdate = false;
            AllowDelete = false;
            AllowInsert = false;
            AllowSelect = true;

            Handlers.Add(GridForm.OkCommand).Invokes += e =>
            {
                _role.Value = _roles.Role;
            };
        }

        Types.Role _role;
        public void Run(Types.Role role)
        {
            _role = role;
            Execute();
        }

        protected override void OnSavingRow()
        {
            _role.Value = _roles.Role;
        }
    }
}
