using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    class ConnUsers
    {
        private List<string> users;

        public ConnUsers()
        {
            users = new List<string>();
        }

        private string GetUserAt(int index)
        {
            return users[index];
        }

        public void AddUser(string nickname)
        {
            users.Add(nickname);
            users.Sort();
        }

        public void RemoveUser(string nickname)
        {
            users.Remove(nickname);
            users.Sort();
        }

        public string this[int i]
        {
            get
            {
                return GetUserAt(i);
            }
        }

    }

}
