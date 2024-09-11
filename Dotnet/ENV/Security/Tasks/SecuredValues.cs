using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using ENV.Data;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Tasks
{
    class SecuredValues : Security.UserManager.SecurityUIController
    {

        internal readonly Entities.SecuredValues securedValues;
        internal readonly TextColumn _cancel = new TextColumn();

        SecuredValuesUI _form;
        public SecuredValues(IEntityDataProvider db)
        {securedValues = new Entities.SecuredValues(db);
            UseWildcardForContainsInTextColumnFilter = true;
            From = securedValues;
            OrderBy.Segments.Add(securedValues.Name);
            _uiController.DatabaseErrorOccurred += new Firefly.Box.Data.DataProvider.DatabaseErrorEventHandler(_uiController_DatabaseErrorOccurred);

            View = () => _form = new SecuredValuesUI(this);

            AddAllColumns();
            Columns.Add(_cancel);
            AllowInsertInUpdateActivity = true;
        }

        void _uiController_DatabaseErrorOccurred(Firefly.Box.Data.DataProvider.DatabaseErrorEventArgs e)
        {
            if (e.ErrorType == DatabaseErrorType.DuplicateIndex)
            {
                e.Handled = true;
                Message.ShowError(LocalizationInfo.Current.NameAlreadyExist);
                e.HandlingStrategy = Firefly.Box.Data.DataProvider.DatabaseErrorHandlingStrategy.RollbackAndRecover;
            }
        }
       

    }
}
