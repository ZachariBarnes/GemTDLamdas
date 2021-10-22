using System;
using System.Collections.Generic;
using System.Linq;
using BCrypt;

namespace GemTD
{
    public class User
    {
        public User(int Id, string user_Name, string Name = null, string eMail = null, string pass = "")
        {
            userID = Id;
            userName = user_Name;
            email = eMail;
            name = Name;
            if (!string.IsNullOrEmpty(pass))
                GeneratePassword(pass);
        }
        public static string CalculatePassword(string pass, string salt)
        {
            string pw = "";
            pw = BCrypt.Net.BCrypt.HashPassword(pass, salt);
            return pw;
        }
        public int userID;
        public string userName;
        public string email;
        public string name;
        public string error;
        public string newPassword;
        private string password;
        private string salt;

        public string Password { get => password; set => password = value; }
        public string Salt { get => salt; set => salt = value; }

        private (string, string, string) ReturnUserData()
        {
            return (userName, name, email);
        }

        internal void GeneratePassword(string inputPassword)
        {
            salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            password = BCrypt.Net.BCrypt.HashPassword(inputPassword, salt);

        }
    }
}
