using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GemTD
{
    public class Score
    {
        public Score(int Id, int Score, int Wave, string Game_Mode = "Classic", string uname = "", int diff = 1)
        {
            userID = Id;
            score = Score;
            wave = Wave;
            gameMode = Game_Mode;
            userName = uname;
            difficulty = diff;
        }
        public int scoreID;
        public int userID;
        public int score;
        public int wave;
        public int difficulty;
        public string gameMode;
        public string userName;
        public string error;

        public async Task<string> LookupUser()
        {
            if (userID != 0 && userName.Equals(""))
            {
                DBUtils database = new DBUtils();
                User user = await database.FetchUser(userID);
                userName = user.userName;
                return userName;
            }
            return "Error:NoUserId";
        }
        private (string, int, int, string, int) ReturnUserData()
        {
            return (userName, score, wave, gameMode, difficulty);
        }
    }
}
