using System;
using System.Collections.Generic;
using System.Globalization;
using ENV.Security.Entities;
using ENV.Security.Types;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security
{
    public class User
    {
        string _id;
        string _name;
        string _password;
        string _description;
        string _additionalInfo;


        public User(string id, string name, string password, string description, string additionalInfo)
        {
            _id = id;
            _name = name;
            _password = password;
            _description = description;
            _additionalInfo = additionalInfo;
        }

        public Text Name
        {
            get { return _name.Trim(); }
        }

        public Text Description
        {
            get { return _description; }
        }

        public Text AdditionalInfo
        {
            get { return _additionalInfo; }
        }

        HashSet<string> _avialableRoles = new HashSet<string>();
        public bool DoYouHave(string key)
        {
            if (_isAdmin)
                return true;
            key = key.ToUpperInvariant();
            if (_avialableRoles.Contains(key))
                return true;
            if (UserManager.Administrator!=null && key != UserManager.Administrator.Key)
                return _avialableRoles.Contains((UserManager.Administrator.Key));
            return false;
        }

        public void ReloadRoles(UserManager.UserStorage db)
        {
            _avialableRoles.Clear();
            db.ProvideUserRoles(_id, y => _avialableRoles.Add(y.ToUpperInvariant()));



        }
        bool _isAdmin;
        internal void SetAsAdmin()
        {
            _isAdmin = true;
        }
        public void AddRole(string key, UserManager.UserStorage db)
        {
            db.AddRole(_id, key);

        }

        public void AddGroup(string groupName, UserManager.UserStorage db)
        {
            db.AddGroupToUser(_id, groupName);

        }





        public static User Find(string username, string password)
        {
            User result = null;
            UserManager.ChangeUserFile(db =>
                                           {
                                               result = db.FindUser(username, password);
                                               if (result != null)
                                                   result.ReloadRoles(db);
                                               return false;
                                           });
            return result;
        }
    }
}