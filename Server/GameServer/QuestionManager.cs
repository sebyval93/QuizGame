using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LitJson;

namespace GameServer
{
    class QuestionManager
    {
        private static readonly QuestionManager instance = new QuestionManager();
        private List<int> questionList;
        private DBManager dbManager = DBManager.Instance;

        private QuestionManager()
        {
            Load();
        }

        public static QuestionManager Instance
        {
            get
            {
                return instance;
            }
        }

        private void Load()
        {
            Console.WriteLine("--");
            Console.WriteLine("Loading questions from database...");
            questionList = dbManager.LoadAllQuestions();

            Console.WriteLine("Found " + questionList.Count + " questions in the database!");

            Console.WriteLine("--");
        }

        public int GetQuestionCount()
        {
            return questionList.Count;
        }



        public QuestionInfo GetRandomQuestion(ref List<int> askedQuestions)
        {
            Random r = new Random();
            List<int> availableQuestions = questionList.Except(askedQuestions).ToList();
            int randomIndex;
            QuestionInfo question = new QuestionInfo();
            if (availableQuestions.Count == 0)
            {
                randomIndex = r.Next(0, questionList.Count);

                question = dbManager.GetQuestionInfo(questionList[randomIndex]);

                // asked questions is full, reset it.
                askedQuestions.Clear();
            }
            else
            {
                randomIndex = r.Next(0, availableQuestions.Count);

                question = dbManager.GetQuestionInfo(questionList[randomIndex]);

                askedQuestions.Add(availableQuestions[randomIndex]);
            }
                                           
            return question;
        }

        public bool CheckAnswer(int id, int sentAnswerKey)
        {
            Question question = dbManager.GetQuestion(id);
            if (question.answerKey == sentAnswerKey)
            {
                Console.WriteLine("Correct!");
                return true;
            }
            else
                Console.WriteLine("Incorrect!");

            return false;
        }

    }
}
