using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace GameServer
{
    public sealed class DBManager
    {
        private static volatile DBManager instance;
        private static object syncRoot = new Object();
        private string connectionStr;// = "server=127.0.0.1;uid=root;" +
           // "pwd=12345;database=licenta_quiz;";
        private static MySqlConnection conn;
        private static MySqlCommand command;

        
        public struct UserInfo
        {
            public string userName;
            public string nickName;
            public int score;
        }

        private DBManager()
        {
            try
            {
                connectionStr = JSONReader.GetDBString();
                command = new MySqlCommand();
                command.Connection = conn;
                Console.WriteLine("Connection to database established successfully!");

            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public static DBManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DBManager();
                    }
                }

                return instance;
            }
        }

        private void RunNonQuery(String sqlCommand)
        {
            conn = new MySqlConnection(connectionStr);
            conn.Open();
            command.Connection = conn;
            command.CommandText = sqlCommand;
            command.ExecuteNonQuery();
        }

        private MySqlDataReader RunQuery(String sqlCommand)
        {
            conn = new MySqlConnection(connectionStr);
            conn.Open();
            command.Connection = conn;
            command.CommandText = sqlCommand;
            return command.ExecuteReader();
        }

        public bool UserExists(String userName)
        {
            String sqlCommand = "select exists(select * from player where name ='"
                + userName + "' limit 1);";
            MySqlDataReader reader = RunQuery(sqlCommand);
            reader.Read();
            if (reader.GetInt32(0) == 0)
                return false;
            else
                return true;
        }

        private int GetUserID(String userName)
        {
            String sqlCommand = "select id from player where name='" + userName + "';";
            MySqlDataReader reader = RunQuery(sqlCommand);
            reader.Read();
            return reader.GetInt32(0);
        }

        private int GetQuestionCount()
        {
            String sqlCommand = "select count(*) from question;";
            MySqlDataReader reader = RunQuery(sqlCommand);
            reader.Read();
            return reader.GetInt32(0);
        }

        private String GetUserHash(String userName)
        {
            String sqlCommand = "select password_hash from player where name='"
                + userName + "';";
            MySqlDataReader reader = RunQuery(sqlCommand);
            reader.Read();
            return reader.GetString(0);
        }

        public void AddUser(String userName, String hash)
        {
            try
            {
                string sqlCommand = "select max(id) from player;";
                MySqlDataReader reader = RunQuery(sqlCommand);
                reader.Read();
                int lastID = reader.GetInt32(0);
                Random rng = new Random();
                string nickname = "newUser" + lastID.ToString() + rng.Next(9999).ToString();

                sqlCommand = "insert into player values(null, '" + userName
                    + "', '" + nickname + "', 0, '" + hash + "');";
                RunNonQuery(sqlCommand);

                
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
            }
        }

        public void RemoveUser(String userName)
        {
            try
            {
                string sqlCommand = "delete from player where name='"
                    + userName + "';";
                RunNonQuery(sqlCommand);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
            }
        }

        public bool ValidateUser(string userName, string suppliedHash)
        {
            try
            {
                string hash = GetUserHash(userName);
                if (hash == suppliedHash)
                    return true;
                else
                    return false;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
                return false;
            }
        }

        public void ModifyScore(string username, int delta)
        {
            try
            {
                int score;
                string sqlCommand = "select score from player where name ='" + username + "';";
                MySqlDataReader reader = RunQuery(sqlCommand);
                reader.Read();
                score = reader.GetInt32(0);

                score += delta;

                sqlCommand = "update player set score = " + score + " where name ='" + username + "';";
                RunNonQuery(sqlCommand);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
            }
        }


        public UserInfo GetUserInfo(string userName)
        {
            UserInfo info;
            info.userName = userName;

            try
            {
                string sqlCommand = "select nickname, score from player where name = '" + userName + "';";
                MySqlDataReader reader = RunQuery(sqlCommand);
                reader.Read();
                info.nickName = reader.GetString(0);
                info.score = reader.GetInt32(1);

                return info;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
                info.nickName = null;
                info.score = -1;
                return info;
            }
        }

        public bool ChangeUserNickname(string username, string newNickname)
        {
            if (newNickname.IndexOfAny(new char[] { '\'', '"', '\\', '/', ',' }) != -1)
                return false;

            try
            {
                String sqlCommand = "select count(nickname) from player where nickname='"
                    + newNickname + "';";
                MySqlDataReader reader = RunQuery(sqlCommand);
                reader.Read();

                if (reader.GetInt32(0) > 0)
                    return false;

                sqlCommand = "update player set nickname='"
                    + newNickname + "' where name='" + username + "';";
                RunNonQuery(sqlCommand);
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
            }

            return false;
        }

        public Question GetQuestion(int id)
        {
            Question result;
            string sqlCommand = "select * from question where id = " + id + ";";
            MySqlDataReader reader = RunQuery(sqlCommand);
            reader.Read();
            result = new Question(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6));
            return result;
        }

        public QuestionInfo GetQuestionInfo(int id)
        {
            Question question = GetQuestion(id);
            QuestionInfo questionInfo = new QuestionInfo();
            questionInfo.id = question.id;
            questionInfo.questionText = question.questionText;
            questionInfo.answer1 = question.answer1;
            questionInfo.answer2 = question.answer2;
            questionInfo.answer3 = question.answer3;
            questionInfo.answer4 = question.answer4;

            return questionInfo;
        }

        public List<int> LoadAllQuestions()
        {
            List<int> result = new List<int>();
            try
            {
                //int questionNum = getQuestionCount();
                // 0     1        2         3         4         5            6
                // id | text | option1 | option2 | option3 | option4 | correct_option
                String sqlCommand = "select id from question;";
                MySqlDataReader reader = RunQuery(sqlCommand);
                while (reader.Read())
                {
                    result.Add(reader.GetInt32(0));
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
                return result;
            }

            return result;
        }

        public QuestionInfo GetRandomQuestion(List<QuestionInfo> askedList)
        {
            try
            {

            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Mysql error: {0}", ex.ToString());
            }

            return new QuestionInfo();
        }


    }
}
