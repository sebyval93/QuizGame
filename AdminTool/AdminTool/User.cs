using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTool
{
    public class User
    {
        public int id;
        public string username;
        public string nickname;
        public int score;

        public User(int id, string username, string nickname, int score)
        {
            this.id = id;
            this.username = username;
            this.nickname = nickname;
            this.score = score;
        }
    }
}
