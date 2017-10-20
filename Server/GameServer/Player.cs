using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Player
    {
        private User user;
        private bool isAI = false;
        private List<int> questionIDs;
        private QuestionManager questionManager = QuestionManager.Instance;

        public Player(User user)
        {
            this.user = user;
            questionIDs = new List<int>();
        }

        public Player(string nickname)
        {
            user = new User("",-1, nickname, null);
            isAI = true;
        }

        public bool IsAI()
        {
            return isAI;
        }

        public string GetNickname()
        {
            return user.GetNickname();
        }

        public Socket GetSocket()
        {
            return user.GetSocket();
        }

        public List<int> GetQuestionIDs()
        {
            return questionIDs;
        }

        public void AddQuestionID(int questionID)
        {
            questionIDs.Add(questionID);
            if (questionIDs.Count() == questionManager.GetQuestionCount())
                questionIDs.Clear();
        }

        public bool QuestionSeen(int questionID)
        {
            if (questionIDs.Contains(questionID))
                return true;
            else
                return false;
        }


    }
}
