using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

public sealed class DBManager
{
    private static volatile DBManager instance;
    private static object syncRoot = new Object();
    private static string databaseStr = "create database if not exists licenta_quiz;";
    private static string databaseUseStr = "use licenta_quiz;";
    private const string connectionStr = "server=127.0.0.1;uid=root;" +
        "pwd=12345;database=licenta_quiz;";
    private static string playerTable = @"create table if not exists player (
					 id int auto_increment primary key,
                     name varchar(50),
                     nickname varchar(50),
                     score int,
                     password_hash varchar(64));";
    private static string questionTable = @"create table if not exists question (
						id int auto_increment primary key,
                        title varchar(150),
                        option1 varchar(150),
                        option2 varchar(150),
                        option3 varchar(150),
                        option4 varchar(150),
                        correct_option int);";
    private static MySqlConnection conn;
    private static MySqlCommand command;

    private DBManager()
    {
        try
        {
            command = new MySqlCommand();
            command.Connection = conn;
            Console.WriteLine("Connection to database established successfully!");
            SetupDB();
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

    private void SetupDB()
    {
        RunNonQuery(databaseStr);
        RunNonQuery(databaseUseStr);
        RunNonQuery(playerTable);
        RunNonQuery(questionTable);
    }

    private long RunNonQuery(String sqlCommand)
    {
        conn = new MySqlConnection(connectionStr);
        conn.Open();
        command.Connection = conn;
        command.CommandText = sqlCommand;
        command.ExecuteNonQuery();
        return command.LastInsertedId;
    }

    private MySqlDataReader RunQuery(String sqlCommand)
    {
        conn = new MySqlConnection(connectionStr);
        conn.Open();
        command.Connection = conn;
        command.CommandText = sqlCommand;
        return command.ExecuteReader();
    }

    private int GetQuestionCount()
    {
        string sqlCommand = "select count(*) from question;";
        MySqlDataReader reader = RunQuery(sqlCommand);
        reader.Read();
        return reader.GetInt32(0);
    }

    private int GetUserCount()
    {
        string sqlCommand = "select count(*) from player;";
        MySqlDataReader reader = RunQuery(sqlCommand);
        reader.Read();
        return reader.GetInt32(0);
    }

    public string CalculateHash(string username, string password)
    {
        SHA256Managed crypto = new SHA256Managed();
        string saltedPass = password + username;
        StringBuilder hash = new StringBuilder();
        byte[] cryptoBytes = crypto.ComputeHash(Encoding.UTF8.GetBytes(saltedPass), 0, Encoding.UTF8.GetByteCount(saltedPass));
        foreach (byte theByte in cryptoBytes)
        {
            hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }

    public void AddQuestion(string title, string ans1, string ans2, string ans3,
        string ans4, int answer)
    {
        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(title)
            && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(title))
        {
            string sqlCommand = "insert into question values(null, '" + title
                + "', '" + ans1 + "', '" + ans2 + "', '" + ans3 + "', '" + ans4 + "', " + answer + ");";
            RunNonQuery(sqlCommand);
        }
    }

    public void AddUser(string username, string nickname, int score, string passHash)
    {
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(nickname) &&
            !string.IsNullOrEmpty(passHash))
        {
            string sqlCommand = "insert into player values(null, '" + username + "', '" + nickname + "', "
                + score + ", '" + passHash + "');";
            RunNonQuery(sqlCommand);
        }
    }

    public void UpdateQuestion(int id, string title, string ans1, string ans2, string ans3,
        string ans4, int answer)
    {
        string sqlCommand = "update question set title = '" + title + "', option1 = '" + ans1 + "', "
            + "option2 = '" + ans2 + "', " + "option3 = '" + ans3 + "', " + "option4 = '" + ans4 + "', "
            + "correct_option = " + answer + " where id = " + id + ";";
        RunNonQuery(sqlCommand);
    }

    public void UpdateUser(int id, string username, string nickname, int score, string password)
    {
        string sqlCommand;
        if (password == null)
        {
            sqlCommand = "update player set name = '" + username + "', set nickname = '" + nickname + "', "
                + "set score = " + score + " where id = " + id + ";";
            RunNonQuery(sqlCommand);

        }
        else
        {
            string passHash = CalculateHash(username, password);
            sqlCommand = "update player set name = '" + username + "', nickname = '" + nickname + "', "
                + "score = " + score + ", password_hash = '" + passHash + "' where id = " + id + ";";
            RunNonQuery(sqlCommand);
        }
    }

    public void DeleteQuestion(int id)
    {
        string sqlCommand = "delete from question where id = " + id + ";";
        RunNonQuery(sqlCommand);
    }

    public void DeleteUser(int id)
    {
        string sqlCommand = "delete from player where id = " + id + ";";
        RunNonQuery(sqlCommand);
    }

    public List<AdminTool.Question> LoadAllQuestions()
    {
        List<AdminTool.Question> result = new List<AdminTool.Question>();
        try
        {
            //int questionNum = getQuestionCount();
            // 0     1        2         3         4         5            6
            // id | text | option1 | option2 | option3 | option4 | correct_option
            String sqlCommand = "select * from question;";
            MySqlDataReader reader = RunQuery(sqlCommand);
            while (reader.Read())
            {
                result.Add(new AdminTool.Question(reader.GetInt32(0), reader.GetString(1),
                    reader.GetString(2), reader.GetString(3), reader.GetString(4),
                    reader.GetString(5), reader.GetInt32(6)));
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Mysql error: {0}", ex.ToString());
            return result;
        }

        return result;
    }

    public List<AdminTool.User> LoadAllUsers()
    {
        List<AdminTool.User> result = new List<AdminTool.User>();
        try
        {
            string sqlCommand = "select id, name, nickname, score from player;";
            MySqlDataReader reader = RunQuery(sqlCommand);
            while (reader.Read())
            {
                result.Add(new AdminTool.User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3)));
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Mysql error: {0}", ex.ToString());
            return result;
        }

        return result;
    }

}
