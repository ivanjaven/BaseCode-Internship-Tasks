using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BaseCode.Models;
using BaseCode.Models.Requests.Car;
using BaseCode.Models.Responses.Car;

namespace BaseCode.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarController : Controller
    {
        private DBContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;

        public CarController(DBContext context, IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
        }

        [HttpPost("GetAllCars")]
        public IActionResult GetAllCars([FromBody] GetAllCarsRequest r)
        {
            if (r.Page < 1) r.Page = 1;
            if (r.PageSize < 1) r.PageSize = 10;
            if (r.PageSize > 100) r.PageSize = 100; 

            GetCarsResponse resp = db.GetAllCars(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetCarById")]
        public IActionResult GetCarById([FromBody] GetCarByIdRequest r)
        {
            GetCarResponse resp = new GetCarResponse();
            if (r.CarId <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "Please provide a valid Car ID.";
                return BadRequest(resp);
            }
            resp = db.GetCarById(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetCarByName")]
        public IActionResult GetCarByName([FromBody] GetCarByNameRequest r)
        {
            if (r.Page < 1) r.Page = 1;
            if (r.PageSize < 1) r.PageSize = 10;
            if (r.PageSize > 100) r.PageSize = 100; 

            GetCarsResponse resp = new GetCarsResponse();
            if (string.IsNullOrEmpty(r.CarName))
            {
                resp.isSuccess = false;
                resp.Message = "Please provide a car name to search.";
                return BadRequest(resp);
            }
            resp = db.GetCarByName(r);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
    }
}