using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using BaseCode.Models.Requests;
using BaseCode.Models.Responses;
using BaseCode.Models;

namespace BaseCode.Controllers
{
    [ApiController] 
    [Route("[controller]")]
    public class BaseCodeController : Controller
    {
        private DBContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;

        private static readonly string[] Summaries = new[]
       {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public BaseCodeController(DBContext context, IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.FirstName))
            {
                resp.Message = "Please specify Firstname.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.LastName))
            {
                resp.Message = "Please specify lastname.";
                return BadRequest(resp);
            }
            resp = db.CreateUserUsingSqlScript(r);
           // resp = db.CreateUserUsingSqlScript(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser([FromBody] CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.Message = "Please specify UserId.";
                return BadRequest(resp);
            }
            resp.UserId = r.UserId;
            resp = db.UpdateUser(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser([FromBody] CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.Message = "Please specify UserId.";
                return BadRequest(resp);
            }

            resp = db.DeleteUser(r.UserId.ToString());

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
        [HttpPost("GetUserList")]
        public IActionResult GetUserList([FromBody] GetUserListRequest r)
        {
            GetUserListResponse resp = new GetUserListResponse();
       
            resp = db.GetUserList(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetUserById")]
        public IActionResult GetUserById([FromBody] GetUserListRequest r)
        {
            GetUserListResponse resp = db.GetUserById(r.UserId.ToString());

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("CreateUserInfo")]
        public IActionResult CreateUserInfo([FromBody] CreateUserInfoRequest r)
        {
            CreateUserInfoResponse resp = new CreateUserInfoResponse();

            if (string.IsNullOrEmpty(r.Mobile))
            {
                resp.Message = "Please specify Mobile.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please specify Email.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Birthday))
            {
                resp.Message = "Please specify Birthday.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Country))
            {
                resp.Message = "Please specify Country.";
                return BadRequest(resp);
            }
            resp = db.CreateUserInfoUsingSqlScript(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetUserProfileList")]
        public IActionResult GetUserProfileList([FromBody] GetUserProfileListRequest r)
        {
            GetUserProfileListResponse resp = new GetUserProfileListResponse();

            resp = db.GetUserProfileList(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // TASK 2 (FEBRUARY 1)

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] RegisterUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.FirstName))
            {
                resp.Message = "Please specify First Name.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.LastName))
            {
                resp.Message = "Please specify Last Name.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please specify Email.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.PhoneNumber))
            {
                resp.Message = "Please specify Phone Number.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Password))
            {
                resp.Message = "Please specify Password.";
                return BadRequest(resp);
            }

            resp = db.RegisterUsingSqlScript(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("LogInUser")]
        public IActionResult LogInUser([FromBody] LogInUserRequest r)
        {
            LogInUserResponse resp = new LogInUserResponse();

            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please Input Email.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Password))
            {
                resp.Message = "Please Input Password.";
                return BadRequest(resp);
            }

            resp = db.LogInUser(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest r)
        {
            ResetPasswordResponse resp = new ResetPasswordResponse();

            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please Input Password.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.CurrentPassword))
            {
                resp.Message = "Please Input Current Password.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.NewPassword))
            {
                resp.Message = "Please Input New Password.";
                return BadRequest(resp);
            }

            resp = db.ResetPassword(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("UpdateUserInfo")]
        public IActionResult UpdateUser([FromBody] RegisterUserRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            if (r == null || string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid data or missing User ID.";
                return BadRequest(resp);
            }

            resp = db.UpdateUserInfo(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // TASK 3 (FEBRUARY 3 )


        [HttpPost("SendOTPResetCode")]
        public IActionResult SendOTPResetCode([FromBody] OTPRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            if (r == null || string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid data or missing User ID.";
                return BadRequest(resp);
            }

            resp = db.SendOTPResetCode(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }


        [HttpPost("ValidateOTP")]
        public IActionResult ValidateOTP([FromBody] OTPValidationRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            if (r == null || string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid data or missing User ID.";
                return BadRequest(resp);
            }

            resp = db.ValidateOTP(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
    }
}
