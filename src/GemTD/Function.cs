
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Npgsql.Logging;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GemTD
{
    public class API
    {
        private const string update = "update";
        private const string create = "create";
        private const string read = "read";
        private const string login = "login";
        private const string scoresByUser = "scoresByUser";
        private const string submitBug = "submitBug";

        /// <summary>
        /// Handles the Creation, updates and reading of Users and Highscores
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<Object> UserRequestHandler(object input, ILambdaContext context)
        {
            // await Task.Run(() => Console.WriteLine("User Request Recieved: {0}", input));
            // await Task.Run(() => Console.WriteLine("Context Recieved: {0}", context));
            DBUtils DbUtils = new DBUtils();
            Object result = new Object();
            dynamic requestJson = JObject.Parse(input.ToString());
            dynamic request = requestJson.data;
            string action = request.action;
            string tempUserID = request.user.userId ?? "";
            int userId = (string.IsNullOrEmpty(tempUserID)) ? 0 : int.Parse(tempUserID);
            string userName = request.user.username;
            string name = request.user.name;
            string email = request.user.email;
            string password = request.user.password ?? "";
            // Console.WriteLine($"Username Variable!: {userName}");
            User requestUser = new User(userId, userName, name, email, password);
            Response response = new Response();
            // Console.WriteLine("The Action is: {0}", action);
            response.status = "Success";
            response.message = $"Called User API with action {action} successfully";
            response.body = "Invalid request Parameters";

            switch (action)
            {
                case read:
                    {
                        if (userId != 0)
                        {
                            User results = await DbUtils.FetchUser(userId);
                            Console.WriteLine("Results: {0}", results);
                            results.Password = null;
                            results.Salt = null;
                            JsonSerializerSettings ignoreNull = new JsonSerializerSettings();
                            ignoreNull.NullValueHandling = NullValueHandling.Ignore;
                            response.body = JsonConvert.SerializeObject(results, ignoreNull);
                        }
                        else
                        {
                            response.error = "Missing or Invalid user.userId";
                        }
                        break;
                    }
                case create:
                    {

                        Console.WriteLine($"User ID for creating user: {userId}");

                        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password))
                        {
                            response.status = "Failure";
                            response.message = "Failure to create user. Username or Password is missing or invalid.";
                            response.error = "Username or Password is missing or invalid.";
                            return JObject.FromObject(response);
                        }
                        requestUser.userID = new int();
                        int results = await DbUtils.CreateUser(requestUser);

                        if (results == 0)
                        {
                            response.status = "Failure";
                            response.message = "Failure to create user. Username already taken";
                            response.error = "Username is Already taken, please Select another name";
                        }
                        else
                        {
                            User createdUser = await DbUtils.FetchUser(results);
                            createdUser.Password = null;
                            createdUser.Salt = null;
                            JsonSerializerSettings ignoreNull = new JsonSerializerSettings();
                            ignoreNull.NullValueHandling = NullValueHandling.Ignore;
                            response.body = JsonConvert.SerializeObject(createdUser, ignoreNull);
                        }
                        break;
                    }
                case update:
                    {
                        string newPassword = request.user.newPassword ?? "";
                        requestUser.newPassword = newPassword;
                        User reqUser = new User(0, userName);
                        User loginUser = await DbUtils.FindUserByUserName(reqUser);
                        string salt = loginUser.Salt;
                        string pw = User.CalculatePassword(password, salt);
                        if (pw == loginUser.Password)
                        {
                            dynamic results = await DbUtils.UpdateUser(requestUser);
                            response.body = JsonConvert.SerializeObject(results);
                            Console.WriteLine("Results: {0}", results);
                        }
                        else
                        {
                            response.status = "Failure";
                            response.message = $"Login Failue for user {loginUser.userName}";
                            response.body = "Username or password provided is invalid";
                        }
                        break;
                    }
                case login:
                    {
                        // Console.WriteLine($"Login user:{requestUser.userID}");
                        User reqUser = new User(0, userName);
                        User loginUser = await DbUtils.FindUserByUserName(reqUser);
                        //Console.WriteLine(JObject.FromObject(loginUser));
                        string salt = loginUser.Salt;
                        string pw = User.CalculatePassword(password, salt);
                        // Console.WriteLine($"given pass {pw}");
                        // Console.WriteLine($"saved pass {loginUser.Password}");

                        if (pw == loginUser.Password)
                        {
                            response.status = "Success";
                            response.message = $"Successfully logged in as user: {loginUser.userName}";
                            loginUser.Password = null;
                            loginUser.Salt = null;
                            JsonSerializerSettings ignoreNull = new JsonSerializerSettings();
                            ignoreNull.NullValueHandling = NullValueHandling.Ignore;
                            response.body = response.body = JsonConvert.SerializeObject(loginUser, ignoreNull);
                        }
                        else
                        {
                            response.status = "Failure";
                            response.message = $"Login Failue for user {loginUser.userName}";
                            response.body = "Username or password provided is invalid";
                        }
                        break;
                    }
                default:
                    {
                        response.status = "Failure";
                        response.message = $"Called API with unsupported action {action}";
                        response.body = "Invalid request Parameters";
                        break;
                    }
            }

            return JObject.FromObject(response);
        }

        public virtual async Task<Object> ScoreRequestHandler(object input, ILambdaContext context)
        {
            // Console.WriteLine("Score Request Recieved: {0}", input);
            // Console.WriteLine("Context Recieved: {0}", context);
            DBUtils DbUtils = new DBUtils();
            Object result = new Object();
            dynamic requestJson = JObject.Parse(input.ToString());
            // return requestJson;
            //return JObject.FromObject(requestJson);
            dynamic request = requestJson.data;

            string action = request.action;
            // Console.Write($"request.user: {request.user}");
            // Console.Write($"request.score: {request.score}");
            int userId = int.Parse(request.user.userId.ToString()) ?? 0;
            string userName = request.user.userName;
            User requestUser = await DbUtils.FetchUser(userId);
            Response response = new Response();
            // Console.WriteLine("The Action is: {0}", action);
            response.status = "Success";
            response.message = $"Called Score API with action {action} successfully";
            response.body = "Invalid request Parameters";

            switch (action)
            {
                case read:
                    {
                        if (request.score != null && request.score.offset != null && request.score.limit != null)
                        {
                            int offset = request.score.offset;
                            int limit = request.score.limit;
                            // Console.WriteLine($"Score: {request.score}, offset: {offset}, Limit: {limit}");
                            List<Score> results = await DbUtils.FetchScores(offset, limit);
                            Console.WriteLine("Score Read Results: {0}", JsonConvert.SerializeObject(results));

                            response.body = JsonConvert.SerializeObject(results);
                        }
                        else
                        {
                            response.status = "Failure";
                            response.message = "Missing or Invalid Parameters";
                            response.body = "Score.offset and/or Score.Limit invalid or missing";
                        }
                        break;
                    }
                case scoresByUser:
                    {
                        bool userProvided = userId > 0;
                        bool scoreProvided = request.score != null;
                        if (scoreProvided && userProvided )
                        {
                            int offset = request.score.offset ?? 0;
                            int limit = request.score.limit ?? 10;
                            Console.WriteLine($"Score: {request.score}, offset: {offset}, Limit: {limit}");
                            List<Score> results = await DbUtils.FetchScoresByUser(offset, limit, userId);
                            Console.WriteLine("Score Read Results: {0}", JsonConvert.SerializeObject(results));

                            response.body = JsonConvert.SerializeObject(results);
                        }
                        else
                        {
                            response.status = "Failure";
                            response.message = "Missing or Invalid Parameters";
                            response.body = "Score.offset and/or Score.Limit invalid or missing";
                        }
                        break;
                    }
                case create:
                    {
                        if (userId > 0)
                        {
                            // Console.WriteLine($"Score?: {request.score}");
                            // Console.WriteLine($"Score is null?: {request.score == null}");
                            if (!(request.score == null))
                            {
                                int newScore = int.Parse(request.score.score.ToString());
                                int newWave = int.Parse(request.score.wave.ToString());
                                int diff = int.Parse(request.score.difficulty.ToString());
                                Score thisScore = new Score(userId, newScore, newWave, request.score.gameMode.ToString(), "", diff);
                                string username = await thisScore.LookupUser();
                                bool validUser = username != null && !username.Contains("Error:NoUserId");
                                bool validScore = thisScore.score != 0 && thisScore.wave != 0;
                                Console.WriteLine($"Score: {JsonConvert.SerializeObject(thisScore)}, userName:{username}, ValidScore: {validScore}, ValidUser:{validUser}");
                                if (validUser && validScore)
                                {
                                    result = await DbUtils.InsertHighScore(thisScore);
                                    requestUser.userID = userId;
                                    response.body = JsonConvert.SerializeObject(result);
                                    Console.WriteLine("Results: {0}", result);
                                }
                                else
                                {
                                    response.status = "Failure";
                                    response.message = "Missing or Invalid Parameters";
                                    response.body = "User or Score Objects are missing or invalid";
                                }
                            }
                        }
                        break;
                    }
                case update:
                    {
                        response.status = "Failure";
                        response.message = $"Called Score API with unsupported action {action}";
                        response.body = "Update Score is not supported";
                        break;
                    }
                default:
                    {
                        response.status = "Failure";
                        response.message = $"Called Score API with unsupported action {action}";
                        response.body = "Invalid request Parameters";
                        break;
                    }
            }

            return JObject.FromObject(response);
        }

        public virtual async Task<Object> BugReportRequestHandler(object input, ILambdaContext context)
        {
            dynamic requestJson = JObject.Parse(input.ToString());
            dynamic request = requestJson.data;
            string action = request.action;
            Console.Write($"BugReport submitter: {request.user.userId}");
            string tempUserID = request.user.userId ?? "";
            int userId = (string.IsNullOrEmpty(tempUserID)) ? 0 : int.Parse(tempUserID);
            string userName = request.user.username;
            string name = request.user.name;
            string email = request.user.email;
            Response response = new Response();
            response.status = "Success";
            response.message = $"Called bugReport API with action {action} successfully";
            response.body = "Invalid request Parameters";

            switch (action)
            {
                case submitBug:
                    {
                        string logString = request.log;
                        Console.WriteLine(JObject.FromObject(request));
                        Console.WriteLine($"Log: {logString}");
                        byte[] log = Convert.FromBase64String(logString);
                        string logfile = Encoding.UTF8.GetString(log);
                        Console.WriteLine(logfile.Substring(0, 100));
                        string description = request.description;
                        string title = request.title;
                        bool SendMessageResponse = await Helpers.SendFeedbackSTMP(title, description, logfile, userName, email, name, userId.ToString());
                        response.body = $"Successfully Sent Message: {SendMessageResponse}";
                        break;
                    }
                default: break;
            }

            return JObject.FromObject(response);
        }
    }
}
