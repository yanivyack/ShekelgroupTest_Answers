using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Tasks
{
    class ChangePassword : Security.UserManager.SecurityUIController
    {
        private string _user;
        internal Types.UserName CurrentPassword = new ENV.Security.Types.UserName();
        internal Types.Password NewPassword = new ENV.Security.Types.Password();
        internal Types.Password ConfirmNewPassword = new ENV.Security.Types.Password();
        internal Firefly.Box.Data.TextColumn Ok = new Firefly.Box.Data.TextColumn();
        internal Firefly.Box.Data.TextColumn Cancel = new Firefly.Box.Data.TextColumn();
        IEntityDataProvider _db;
        public ChangePassword(IEntityDataProvider db, string user = null)
        {
            _db = db;
            _user = user;
            Columns.Add(CurrentPassword, NewPassword, ConfirmNewPassword, Ok, Cancel);


            View = () =>
            {
                if (IsAdmin())
                    return new ChangePasswordAdminUI(this);
                else
                    return new ChangePasswordUI(this);
            };
        }

        private bool IsAdmin()
        {
            return !string.IsNullOrEmpty(_user);
        }

        internal void TryChangePassword()
        {
            if (IsAdmin())
                ChangeUserPassword();
            else
                ChangeCurrentUserPassword();
        }

        private void ChangeUserPassword()
        {
            if(ConfirmNewPassword != NewPassword)
            {
                Message.ShowError(ENV.LocalizationInfo.Current.ConfirmNewPasswordNotSameAsNewPassword);
            }

            var e = new Entities.Users(_db);
            var bp = new BusinessProcess { From = e };
            bp.Where.Add(e.UserName.IsEqualTo(_user));
            bp.ForFirstRow(() => e.Password.Value = NewPassword);
            Exit();
        }



        private void ChangeCurrentUserPassword()
        {
            bool ok = false, showedError = false;
            
            var e = new Entities.Users(_db);
            var bp = new BusinessProcess { From = e };
            bp.Where.Add(e.UserName.IsEqualTo(UserManager.CurrentUser.Name));
            Action<string> error = x =>
            {
                showedError = true;
                Message.ShowError(x);

            };
            bp.ForFirstRow(() =>
            {
                if (CurrentPassword.ToUpper() != e.Password.ToUpper())
                {
                    error(ENV.LocalizationInfo.Current.WrongCurrentPassword);
                }
                else if (CurrentPassword.ToUpper() == NewPassword.ToUpper())
                {
                    error(ENV.LocalizationInfo.Current.NewPasswordSameAsOldPassword);
                }
                else if (ConfirmNewPassword != NewPassword)
                {
                    error(ENV.LocalizationInfo.Current.ConfirmNewPasswordNotSameAsNewPassword);
                }
                else
                {
                    e.Password.Value = NewPassword;
                    ok = true;
                }
            });
            if (!ok && !showedError)
            {
                Message.ShowError(ENV.LocalizationInfo.Current.FailedToChangePassword);
            }
            if (ok)
            {
                Message.ShowWarning(ENV.LocalizationInfo.Current.PasswordWasChangedSuccessfully);
                Exit();
            }
        }

        
    }
}