using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        DataContextDapper _dapper;
        public UserController(IConfiguration config)
        {
            _dapper= new DataContextDapper(config);
        }
        [HttpGet ("TestConnection")]
        public DateTime TestConnection(){
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }
        [HttpGet("test/{testValue}")]
        // public IActionResult Test()
        public string[] Test(string testValue)
        {
            string[] responseArray = new string[]{
                "test1",
                "test2",
                testValue
            };
            return responseArray;
        }
    }
}