using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using GemTD;
using Moq;

namespace GemTD.Tests
{
    public class FunctionTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FunctionTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }


        [Fact]
        public async void TestReadFromDb()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new GemTD.API();
            var context = new TestLambdaContext();
            dynamic upperCase = await function.UserRequestHandler("{\"action\":\"read\",\"userId\":2}", context);
            // _testOutputHelper.WriteLine(upperCase);
            Response response = new Response();
            response.status = "Success";
            response.body = JsonConvert.SerializeObject(new User(2, "Thrasonic", "Zachari Barnes", "Sabin6120@yahoo.com"));
            // response.body = "{\"userID\":0,\"userName\":\"error\",\"email\":\"error\",\"name\":\"Zachari Barnes\",\"error\":null}";

            response.message = $"Called API with action read successfully";
            // var response = await DoAsyncWork(input);
            dynamic result = JObject.FromObject(response);
            var obj1Str = JsonConvert.SerializeObject(result);
            var obj2Str = JsonConvert.SerializeObject(upperCase);
            _testOutputHelper.WriteLine(obj2Str);
            Assert.Equal(obj1Str, obj2Str);
        }


        [Fact]
        public async void TestUpdateUserFromDb()
        {
            // Invoke the lambda function and confirm the string was upper cased.
            var function = new GemTD.API();
            var context = new TestLambdaContext();
            Object resultingUser = await function.UserRequestHandler(
                "{\"action\":\"update\",\"user\":{\"userId\":2, \"userName\":\"Thrasonic\", \"name\":\"Zachari Barnes\", \"email\":\"Sabin6120@yahoo.com\"}}",
                context);
            _testOutputHelper.WriteLine($" User Result: {resultingUser}");
            Response response = new Response();
            response.status = "Success";
            response.body = JsonConvert.SerializeObject(new User(2, "Thrasonic", "Zachari Barnes", "Sabin6120@yahoo.com"));
            response.message = $"Called API with action update successfully";

            dynamic result = JObject.FromObject(response);
            var obj1Str = JsonConvert.SerializeObject(result);
            var obj2Str = JsonConvert.SerializeObject(resultingUser);
            _testOutputHelper.WriteLine(obj2Str);
            Assert.Equal(obj1Str, obj2Str);
        }

        // [Fact]
        // public async void TestCreateUserLogic()
        // {
        //     // Invoke the lambda function and confirm the string was upper cased.
        //     // var function = new Mock<GemTD.API>();
        //     var API = new GemTD.API();
        //     var context = new TestLambdaContext();
        //     Response response = new Response();
        //     response.status = "Success";
        //     // response.body = "{\"userID\":4}";
        //     response.body = 5;
        //     response.message = $"Called API with action create successfully";
        //     // var response = await DoAsyncWork(input);
        //     dynamic result = JObject.FromObject(response);
        //     string parameters = "{\"action\":\"create\",\"userName\":\"TestUser\",\"name\":\"Tester\", \"email\":\"Nope@note.com\"}";
        //     // function
        //     //     .Setup(i => i.RequestHandler(parameters, context))
        //     //     .Returns(Task.FromResult(default(object)))
        //     //     .Raises( i => JObject.FromObject(response));
        //     dynamic upperCase =
        //         await API.MOCKRequestHandler(parameters, context);
        //     // _testOutputHelper.WriteLine(upperCase);

        //     var obj1Str = JsonConvert.SerializeObject(result);
        //     var obj2Str = JsonConvert.SerializeObject(upperCase);
        //     _testOutputHelper.WriteLine(obj2Str);
        //     Assert.Equal(obj1Str, obj2Str);
        // }
    }
}
