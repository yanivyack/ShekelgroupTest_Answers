using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;

namespace ENV.Security
{
    public class Role
    {
        string _name;
        string _key;
        protected Role _admin;

        public Role(string name, string key, bool isPublic = true)
        {
            _name = name;
            _key = key.ToUpperInvariant();

            Security.Entities.AvailableRoles.AddRole(_key, name, isPublic);
            AddRoleToDictionary(_key, _name);
        }
        static Dictionary<Text, Text> _rolesByName = new Dictionary<Text, Text>();
        private static void AddRoleToDictionary(string key, string name)
        {
            lock (_rolesByName)
            {
                Text n = name.ToUpperInvariant();
                if (!_rolesByName.ContainsKey(n))
                    _rolesByName.Add(n, key.ToUpperInvariant());
                else
                {
                    _rolesByName[n] = key.ToUpperInvariant();
                }
            }
        }

        public static Text GetByName(Text value)
        {
            Text result = null;
            if (_rolesByName.TryGetValue(value.ToUpper(), out result))
                return result;
            return result;
        }
        public bool Allowed
        {
            get { return UserManager.CurrentUser.DoYouHave(_key)||(_admin!=null&&_admin.Allowed); }
        }
        /// <summary>
        /// Checks if the user is autorized - otherwise, displayes an error
        /// </summary>
        /// <returns></returns>
        public bool IsAuthorized()
        {
            if (Allowed)
                return true;
            Context.Current.BeginInvoke(() => Message.ShowWarningInStatusBar(LocalizationInfo.Current.UserIsNotAuthorized));
            return false;
        }
        internal string Key { get { return _key; } }

        public string Name { get { return _name; } }

        public class Administrator : Role
        {
            public Administrator(string name, string key)
                : base(name, key, false)
            {

            }

            public void ApplyAdminTo(Type type)
            {
                foreach (var item in type.GetFields(
                    System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static))
                {
                    var role = item.GetValue(null) as Role;
                    if (role != null&&role!=this) {
                        role._admin = this;
                    }
                }
            }
        }
    }
    public class RolesCollection
    {
        Func<RolesStorage> _rolesByIndex;
        public RolesCollection(Type t)
        {
            _rolesByIndex = () => {
                var r = new RolesStorage();
                foreach (var item in t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                {
                    var y = item.GetValue(null) as Role;
                    if (y != null) {
                        r.Add( y);
                    }

                }
                _rolesByIndex = () => r;
                return r;
            };
        }
        class RolesStorage
        {
            Dictionary<int, Role> _roles = new Dictionary<int, Role>();
            Dictionary<string, int> _rolesByName = new Dictionary<string, int>();
            Dictionary<Role, int> _roleIds = new Dictionary<Role, int>();

            internal void Add(Role y)
            {
                var number = _roles.Count + 1;
                _roles.Add(number, y);
                var key = y.Name.ToUpper();
                if (!_rolesByName.ContainsKey(key))
                    _rolesByName.Add(key, number);
                if (!_roleIds.ContainsKey(y))
                    _roleIds.Add(y, number);
            }

            internal bool TryGetValue(Number roleNumber, out Role r)
            {
                return _roles.TryGetValue(roleNumber, out r);
            }

            internal Number GetByName(string value)
            {
                int r;
                if (_rolesByName.TryGetValue(value.ToUpper(), out r)) return r;
                return 0;
            }

            internal Number GetByRole(Role r)
            {
                return _roleIds[r];
            }
        }

        internal Bool IsRoleAllowed(Number roleNumber)
        {
            Role r;
            if (_rolesByIndex().TryGetValue(roleNumber, out r))
                return r.Allowed;
            return false;
        }

        internal Number GetByName(string value)
        {
            return _rolesByIndex().GetByName(value);
        }

        internal Number IndexOf(Role role)
        {
            return _rolesByIndex().GetByRole(role);
        }
    }
}
