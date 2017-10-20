using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class UserManager
    {
        private static UserManager instance = new UserManager();
        private static object syncRoot = new Object();
        private List<User> userList;

        private UserManager()
        {
            userList = new List<User>();
        }

        public static UserManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new UserManager();
                    }
                }

                return instance;
            }
        }

        public void AddUser(User user)
        {
            bool userExists = userList.Contains(user);

            lock (userList)
            {
                if (!userExists)
                    userList.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            bool userExists = userList.Contains(user);

            lock (userList)
            {
                if (userExists)
                    userList.Remove(user);
            }
        }

        public User FindUser(Predicate<User> predicate)
        {
            User user = null;
            try
            {
                user = userList.Find(predicate);
            }
            catch (Exception e) { };


            return user;
        }

        public User GetUser(Socket socket)
        {
            return FindUser(user => user.GetSocket().Equals(socket));
        }

        public List<User> GetUserList()
        {
            return userList;
        }

        public bool UserLoggedIn(string username)
        {
            try
            {
                User user = userList.Find(x => x.GetUsername().Equals(username));
                if (user != null)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
