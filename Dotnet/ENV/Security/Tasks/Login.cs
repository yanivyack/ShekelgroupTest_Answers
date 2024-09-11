using System;
using System.Collections.Generic;
using System.Text;
using ENV.Data;
using Firefly.Box;

namespace ENV.Security.Tasks
{
    class Login : FlowUIControllerBase
    {
        public static bool InputDate = false;
        LoginUI _form;
        internal Types.UserName UserName = new ENV.Security.Types.UserName() { DefaultValue = UserManager.DefaultUser ?? "" };
        internal Types.Password Password = new ENV.Security.Types.Password();
        internal DateColumn Date = new DateColumn() { Value = Firefly.Box.Date.Now };
        internal Firefly.Box.Data.TextColumn Ok = new Firefly.Box.Data.TextColumn();
        internal Firefly.Box.Data.TextColumn Cancel = new Firefly.Box.Data.TextColumn();
        CustomCommand DoTheLogin = new CustomCommand() { Shortcut = System.Windows.Forms.Keys.Enter , Precondition = CustomCommandPrecondition.SaveControlDataToColumn};
        Role _loginRole;
        public Login(Role loginRole)
        {
            _loginRole = loginRole;
            Columns.Add(UserName, Password, Date, Ok, Cancel);
            Handlers.Add(DoTheLogin).Invokes += e => DoLogin();
            Handlers.Add(System.Windows.Forms.Keys.Enter).Invokes += e => {

                Raise(DoTheLogin);
                e.Handled = true;
            };
            View = () => _form = new LoginUI(this);
        }

        bool _result = false;
        public bool Run()
        {
            Execute();

            return _result;
        }

        public void DoLogin()
        {
            if (UserManager.Login(UserName, Password))
            {
                if (_loginRole != null && !_loginRole.Allowed)
                {
                    Common.ShowMessageBox("Error", System.Windows.Forms.MessageBoxIcon.Error, LocalizationInfo.Current.UserNotAuthorizedForThisApplication);
                }
                else
                {
                    Exit();
                    UserManager.DefaultUser = UserName;
                    UserManager.DefaultPassword = Password;
                    UserManager.DisplayLoginDialog = false;
                    _result = true;
                    if (InputDate)
                        UserMethods.UserDeterminedDate = Date;
                }
            }
            else
            {
                Common.ShowMessageBox("Error", System.Windows.Forms.MessageBoxIcon.Error, LocalizationInfo.Current.UserNotExistsOrPasswordWasWrong);
            }
        }
    }
}