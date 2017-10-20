using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LitJson;

namespace GameServer
{
    public class Question
    {
        public int id;
        public string questionText;
        public string answer1;
        public string answer2;
        public string answer3;
        public string answer4;
        public int answerKey;

        public Question(int id, string questionText, string answer1, string answer2, string answer3, string answer4, int answerKey)
        {
            this.id = id;
            this.questionText = questionText;
            this.answer1 = answer1;
            this.answer2 = answer2;
            this.answer3 = answer3;
            this.answer4 = answer4;
            this.answerKey = answerKey;
        }

        public Question()
        { }
    }

    class Questions
    {
        public List<Question> questions;

        public Questions()
        {
            questions = new List<Question>();
        }
    }

}
