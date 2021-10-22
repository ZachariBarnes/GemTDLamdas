using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Npgsql.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.

namespace GemTD
{

    public class DBUtils
    {
        private NpgsqlConnection CreateConnection()
        {

            string connString = Helpers.GetRDSConnectionString();
            // //Console.WriteLine("Conn:{0}", connString);
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            //Console.WriteLine("Creating Connection");
            return conn;
        }
        public async Task<User> FetchUser(int userId)
        {
            //Console.WriteLine($"Fetching data for user {userId}");
            if (userId == 0)
            {
                User error = new User(0, "Error");
                error.error = "Missing or invalid UserId";
                return new User(0, "error");
            }
            try
            {
                NpgsqlConnection db = CreateConnection();
                User myUser = new User(0, "test"); //place holder
                string sql = "SELECT id, username, name, email, password FROM public.users where id = :user";

                await db.OpenAsync();
                //Console.WriteLine("Connection Open");

                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("user", userId);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // //Console.WriteLine($"Db read:{dr}");
                    // Console.Write("{0}\t{1} \n", dr[0], dr[1]);
                    myUser = new User(int.Parse(dr[0].ToString()), dr[1].ToString(), dr[2].ToString(), dr[3].ToString());
                    myUser.Password = dr[4].ToString();
                    dr.Close();
                    if (myUser.userID > 0)
                    {
                        string saltSql = "SELECT salt FROM public.salt where userid = :uID";
                        NpgsqlCommand saltCmd = new NpgsqlCommand(saltSql, db);
                        saltCmd.Parameters.AddWithValue("uID", myUser.userID);
                        NpgsqlDataReader dr2 = saltCmd.ExecuteReader();
                        if (dr2.Read())
                            myUser.Salt = dr2[0].ToString();
                        dr2.Close();
                    }
                }
                else
                {
                    //Console.WriteLine("No Data Found for given User");
                    myUser.error = $"No Data Found for id: {userId}";
                }

                dr.Close();
                db.Close();
                //Console.WriteLine("Connection Closed");
                return myUser;

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }
        }
        public async Task<User> FindUserByUserName(User ExisitngUser)
        {
            //Console.WriteLine($"Find param: {JObject.FromObject(ExisitngUser)}");
            //Console.WriteLine($"Fetching data for userName: {ExisitngUser.userName}");
            if (string.IsNullOrEmpty(ExisitngUser.userName))
            {
                User error = new User(0, "Error");
                error.error = "Missing or invalid UserName";
                return new User(0, "error");
            }
            try
            {
                NpgsqlConnection db = CreateConnection();
                User myUser = new User(0, "test"); //place holder
                string sql = "SELECT * FROM public.users where username = :username";

                await db.OpenAsync();
                //Console.WriteLine("Connection Open");

                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("username", ExisitngUser.userName);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // //Console.WriteLine($"Db read:{dr}");
                    // Console.Write("{0}\t{1} \n", dr[0], dr[1]);
                    myUser = new User(int.Parse(dr[0].ToString()), dr[1].ToString(), dr[2].ToString(), dr[3].ToString());
                    myUser.Password = dr[4].ToString();
                    dr.Close();
                    if (myUser.userID > 0)
                    {
                        string saltSql = "SELECT salt FROM public.salt where userid = :uID";
                        NpgsqlCommand saltCmd = new NpgsqlCommand(saltSql, db);
                        saltCmd.Parameters.AddWithValue("uID", myUser.userID);
                        NpgsqlDataReader dr2 = saltCmd.ExecuteReader();
                        if (dr2.Read())
                            myUser.Salt = dr2[0].ToString();
                        dr2.Close();
                    }
                }
                else
                {
                    //Console.WriteLine("No Data Found for given User");
                    myUser.error = $"No Data Found for id: {ExisitngUser.userName}";
                }

                dr.Close();
                db.Close();
                //Console.WriteLine("Connection Closed");
                return myUser;

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }
        }
        public async Task<Object> UpdateUser(User UpdateUser)
        {

            try
            {
                int UserID = UpdateUser.userID;
                //Console.WriteLine($"UserID before Lookup: {UserID}, UpdatedUser: {UpdateUser}");
                User oldUser = await FetchUser(UserID);
                User newUser = new User(0, null);

                if (!string.IsNullOrEmpty(UpdateUser.newPassword)) {
                    UpdateUser.GeneratePassword(UpdateUser.newPassword);
                }
                string userName = UpdateUser.userName ?? oldUser.userName;
                string name = UpdateUser.name ?? oldUser.name;
                string email = UpdateUser.email ?? oldUser.email;
                string password = UpdateUser.Password ?? oldUser.Password;
                string salt = UpdateUser.Salt ?? oldUser.Salt;
                NpgsqlConnection db = CreateConnection();
                string sql = $"Update public.users SET username= :uname, name= :realname, email= :emailname, password= :pw where id = :uid returning *";

                await db.OpenAsync();
                //Console.WriteLine("Connection Open");
                //Console.WriteLine($"Parameters: {userName}, {name}, {email}, {UserID}");

                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("uname", userName);
                cmd.Parameters.AddWithValue("realname", name);
                cmd.Parameters.AddWithValue("emailname", email);
                cmd.Parameters.AddWithValue("pw", password);
                cmd.Parameters.AddWithValue("uid", UserID);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    newUser = new User(int.Parse(dr[0].ToString()), dr[1].ToString(), dr[2].ToString(), dr[3].ToString());
                    Console.Write("{0}", dr[0]);
                    UserID = int.Parse(dr[0].ToString());
                }
                else
                {
                    Console.WriteLine("Failed to Update User");
                }
                dr.Close();
                string saltSql = "UPDATE public.salt SET salt= :slt WHERE userid= :uid returning *";
                NpgsqlCommand saltCmd = new NpgsqlCommand(saltSql, db);
                saltCmd.Parameters.AddWithValue("slt", salt);
                saltCmd.Parameters.AddWithValue("uid", UserID);
                NpgsqlDataReader dr2 = saltCmd.ExecuteReader();
                dr2.Close();
                db.Close();
                Console.WriteLine("Connection Closed");
                return newUser;

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }
        }
        public async Task<int> CreateUser(User NewUser)
        {
            //Console.WriteLine($"Create User param: {JObject.FromObject(NewUser)}");

            try
            {
                User ExisitngUser = await FindUserByUserName(NewUser);
                int newUserID = ExisitngUser.userID;
                if (newUserID > 0)
                {
                    return 0;
                }
                NpgsqlConnection db = CreateConnection();
                string sql = "INSERT INTO public.users (username, name, email, password) VALUES(:uname, :realname, :emailname, :pw) returning id";

                await db.OpenAsync();
                //Console.WriteLine("Connection Open");

                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("uname", NewUser.userName);
                cmd.Parameters.AddWithValue("realname", NewUser.name);
                cmd.Parameters.AddWithValue("emailname", NewUser.email);
                cmd.Parameters.AddWithValue("pw", NewUser.Password);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    Console.Write("{0}", dr[0]);
                    newUserID = int.Parse(dr[0].ToString());
                    dr.Close();
                    if (newUserID > 0)
                    {
                        string saltSql = "INSERT INTO public.salt (userid, salt) VALUES(:uID, :satl) returning id";
                        NpgsqlCommand saltCmd = new NpgsqlCommand(saltSql, db);
                        saltCmd.Parameters.AddWithValue("uID", newUserID);
                        saltCmd.Parameters.AddWithValue("satl", NewUser.Salt);
                        NpgsqlDataReader dr2 = saltCmd.ExecuteReader();
                        dr2.Close();
                    }
                }
                else
                {
                    //Console.WriteLine("Failed to create User");
                }

                db.Close();
                //Console.WriteLine("Connection Closed");
                return newUserID;

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }
        }
        public async Task<Score> InsertHighScore(Score newScore)
        {
            Score dbScore = new Score(0, 0, 0);
            try
            {
                int userID = newScore.userID;
                NpgsqlConnection db = CreateConnection();
                string sql = "INSERT INTO public.highscores (user_id, game_mode, score, wave, difficulty) VALUES(:uid, :gamemode, :score, :wave, :difficulty) returning *";

                await db.OpenAsync();
                //Console.WriteLine("Connection Open");

                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("uid", newScore.userID);
                cmd.Parameters.AddWithValue("gamemode", newScore.gameMode);
                cmd.Parameters.AddWithValue("score", newScore.score);
                cmd.Parameters.AddWithValue("wave", newScore.wave);
                cmd.Parameters.AddWithValue("difficulty", newScore.difficulty);

                NpgsqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    dbScore = new Score(int.Parse(dr[1].ToString()), int.Parse(dr[3].ToString()), int.Parse(dr[4].ToString()), dr[2].ToString());
                    dbScore.scoreID = int.Parse(dr[0].ToString());
                    Console.Write("{0}", dr[0]);
                }
                else
                {
                    //Console.WriteLine("Failed to create User");
                }

                dr.Close();
                db.Close();
                //Console.WriteLine("Connection Closed");
                return dbScore;

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }
        }
        public async Task<List<Score>> FetchScores(int offset = 0, int limit = 10)
        {

            //Console.WriteLine($"Fetching top 10 Scores");
            try
            {
                List<Score> topScores = new List<Score>();
                NpgsqlConnection db = CreateConnection();
                string sql = "SELECT s.id, s.score, s.wave, s.game_mode, u.username, s.difficulty FROM public.highscores s left outer join public.users u on u.id=s.user_id order by score desc limit :limit offset :offset";
                await db.OpenAsync();
                //Console.WriteLine("Connection Open");
                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("limit", limit);
                cmd.Parameters.AddWithValue("offset", offset);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    // //Console.WriteLine($"Db read:{dr}");
                    Console.Write($"Score Found, Pts:{dr[1]}, Wave:{dr[2]}, Mode:{dr[3]}, User:{dr[4]}, Difficulty:{dr[5]}");
                    topScores.Add(new Score(int.Parse(dr[0].ToString()), int.Parse(dr[1].ToString()), int.Parse(dr[2].ToString()), dr[3].ToString(), dr[4].ToString(), int.Parse(dr[5].ToString())));
                }
                dr.Close();
                db.Close();
                //Console.WriteLine("Connection Closed");
                return topScores;

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }
        }

        public async Task<List<Score>> FetchScoresByUser(int offset = 0, int limit = 10, int userId = 0)
        {
            Console.WriteLine($"Fetching top 10 Scores");
            try
            {
                List<Score> topScores = new List<Score>();
                NpgsqlConnection db = CreateConnection();
                string sql = "SELECT s.id, s.score, s.wave, s.game_mode, u.username, s.difficulty " +
                    "FROM public.highscores s " +
                    "left outer join public.users u on u.id=s.user_id " +
                    "where u.id = :userId " +
                    "order by score desc limit :limit offset :offset";
                await db.OpenAsync();
                //Console.WriteLine("Connection Open");
                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("limit", limit);
                cmd.Parameters.AddWithValue("offset", offset);
                cmd.Parameters.AddWithValue("userId", userId);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    // //Console.WriteLine($"Db read:{dr}");
                    Console.Write($"Score Found, Pts:{dr[1]}, Wave:{dr[2]}, Mode:{dr[3]}, User:{dr[4]}, Difficulty:{dr[5]}");
                    topScores.Add(new Score(int.Parse(dr[0].ToString()), int.Parse(dr[1].ToString()), int.Parse(dr[2].ToString()), dr[3].ToString(), dr[4].ToString(), int.Parse(dr[5].ToString())));
                }
                dr.Close();
                db.Close();
                //Console.WriteLine("Connection Closed");
                return topScores;

            }
            catch (Exception msg)
            {
                //Console.WriteLine(msg.ToString());
                throw msg;
            }
        }
        public async Task<List<Score>> FetchScoresByUserDifficulty(int offset = 0, int limit = 10, int userId = 0, int diff = 1)
        {
            Console.WriteLine($"Fetching top 10 Scores");
            try
            {
                List<Score> topScores = new List<Score>();
                NpgsqlConnection db = CreateConnection();
                string sql = "SELECT s.id, s.score, s.wave, s.game_mode, u.username, s.difficulty " +
                    "FROM public.highscores s " +
                    "left outer join public.users u on u.id=s.user_id " +
                    "where u.id = :userId and u.difficulty = :difficulty" +
                    "order by score desc limit :limit offset :offset";
                await db.OpenAsync();
                //Console.WriteLine("Connection Open");
                NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
                cmd.Parameters.AddWithValue("limit", limit);
                cmd.Parameters.AddWithValue("offset", offset);
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("difficulty", diff);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    // //Console.WriteLine($"Db read:{dr}");
                    Console.Write($"Score Found, Pts:{dr[1]}, Wave:{dr[2]}, Mode:{dr[3]}, User:{dr[4]}, Difficulty:{dr[5]}");
                    topScores.Add(new Score(int.Parse(dr[0].ToString()), int.Parse(dr[1].ToString()), int.Parse(dr[2].ToString()), dr[3].ToString(), dr[4].ToString(), int.Parse(dr[5].ToString())));
                }
                dr.Close();
                db.Close();
                //Console.WriteLine("Connection Closed");
                return topScores;

            }
            catch (Exception msg)
            {
                //Console.WriteLine(msg.ToString());
                throw msg;
            }
        }

    }
}