using System;
using System.Collections.Generic;
using System.Linq;

namespace GemTD
{
    public class Score
    {
        public Score(int Id, int Score, int Wave, string Game_Mode = "Classic", string uname = "")
        {
            userID = Id;
            score = Score;
            wave = Wave;
            gameMode = Game_Mode;
            userName = uname;
            if (userID != 0 && uname.Equals(""))
                LookupUser();
        }
        public int scoreID;
        public int userID;
        public int score;
        public int wave;
        public string gameMode;
        public string userName;
        public string error;

        public async void LookupUser()
        {
            DBUtils database = new DBUtils();
            User user = await database.FetchUser(userID);
            userName = user.userName;
        }
        private (string, int, int, string) ReturnUserData()
        {
            return (userName, score, wave, gameMode);
        }
    }
}
