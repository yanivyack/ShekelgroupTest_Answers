using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.Sockets;

namespace ENV
{
    class LdapClient : IDisposable
    {
        public static string DefaultServerAndPort;

        System.DirectoryServices.Protocols.LdapConnection c;
        public static Exception LastException;

        static LdapClient _defaultClient;
        public static LdapClient DefaultClient
        {
            get
            {
                if (_defaultClient == null || _defaultClient._isDisposed)
                    _defaultClient = new LdapClient(Security.Entities.SecuredValues.Decode(DefaultServerAndPort));
                return _defaultClient;
            }
        }

        public LdapClient(string serverAndPort)
        {
            var s = serverAndPort.Split(':');
            var he = System.Net.Dns.GetHostEntry(s[0]);
            var ips = new List<string>();
            foreach (var ip in he.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ips.Add(ip.ToString());
            }

            if (ips.Count == 1)
                serverAndPort = serverAndPort.Replace(s[0], ips[0].ToString());

            c = new LdapConnection(serverAndPort);
            c.AutoBind = false;
            c.SessionOptions.ProtocolVersion = 3;
            c.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
            c.AuthType = AuthType.Basic;
        }

        void WrapAndStoreException(Action action)
        {
            LastException = null;
            try
            {
                action();
            }
            catch (Exception e)
            {
                var le = e as LdapException;
                if (le == null || le.ErrorCode != 49)
                    ErrorLog.WriteToLogFile(e, "LDAP");
                LastException = e;
                throw;
            }
        }

        public void Connect(string connectionString, string password)
        {
            Connect(() =>
            {
                if (string.IsNullOrEmpty(password))
                    throw new LdapException(49, "The supplied credential is invalid.");
                c.Bind(new NetworkCredential(connectionString.TrimEnd(), password));
            });
        }

        void Connect(Action bind)
        {
            WrapAndStoreException(
                () =>
                {
                    try
                    {
                        bind();
                    }
                    catch (Exception e)
                    {
                        Dispose();
                        throw;
                    }
                });
        }

        bool _isDisposed = false;
        public void Dispose()
        {
            _isDisposed = true;
            c.Dispose();
        }

        public string[] Search(string searchBase, System.DirectoryServices.Protocols.SearchScope scope, string filter, string attribute)
        {
            var results = new List<string>();
            WrapAndStoreException(
                () =>
                {
                    var r = new SearchRequest();
                    r.DistinguishedName = searchBase;
                    r.Filter = filter;
                    r.Attributes.Add(attribute);
                    r.Scope = scope;
                    foreach (SearchResultEntry e in ((SearchResponse)c.SendRequest(r)).Entries)
                        if (e.Attributes.Count > 0)
                            for (int i = 0; i < e.Attributes[attribute].Count; i++)
                                results.Add(e.Attributes[attribute][i].ToString());
                });
            return results.ToArray();
        }

    }
}
