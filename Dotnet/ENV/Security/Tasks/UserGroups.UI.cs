using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;

namespace ENV.Security.Tasks
{
    partial class UserGroupsUI : UI.GridForm
    {
        UserGroups _task;
        public UserGroupsUI(UserGroups task)
        {
            _task = task;
            InitializeComponent();
            groupsCombo.BindListSource +=
                (sender, e) =>
                {
                    groupsCombo.ListSource = _task._groupsForList;
                    groupsCombo.ValueColumn = _task._groupsForList.ID;
                    groupsCombo.DisplayColumn = _task._groupsForList.Description;
                    groupsCombo.ListOrderBy = new Sort(_task._groupsForList.Description);
                };
            gridColumn1.Text = LocalizationInfo.Current.Groups;
            Text = LocalizationInfo.Current.UserGroups;
        }
    }
}