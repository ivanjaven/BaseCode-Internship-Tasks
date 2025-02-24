using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BaseCode.Models;
using BaseCode.Models.Requests.Roles;
using BaseCode.Models.Responses;
using BaseCode.Models.Responses.Roles;
using BaseCode.Models.Requests;

namespace BaseCode.Controllers
{
    // TASK 5 (FEB 20) ROLES AND PERMISSIONS
    [ApiController]
    [Route("[controller]")]
    public class RolePermissionController : Controller
    {
        private DBContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;

        public RolePermissionController(DBContext context, IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
        }

        [HttpPost("CreateRole")]
        public IActionResult CreateRole([FromBody] CreateRoleRequest r)
        {
            RoleResponse resp = new RoleResponse();

            if (!db.CheckPermission(r.UserId, "ROLE_MANAGEMENT_WRITE"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to create roles.";
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.RoleName))
            {
                resp.Message = "Please specify Role Name.";
                return BadRequest(resp);
            }

            resp = db.CreateRole(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetRoles")]
        public IActionResult GetRoles([FromBody] GetRoleRequest r)
        {
            GetRoleListResponse resp = new GetRoleListResponse();

            if (!db.CheckPermission(r.UserId, "ROLE_MANAGEMENT_READ"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to view roles.";
                return BadRequest(resp);
            }

            resp = db.GetRoles(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetPermissions")]
        public IActionResult GetPermissions([FromBody] GetRoleRequest r)
        {
            PermissionResponse resp = new PermissionResponse();

            if (!db.CheckPermission(r.UserId, "PERMISSION_MANAGEMENT_READ"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to view permissions.";
                return BadRequest(resp);
            }

            resp = db.GetPermissions();

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("AssignPermissionToRole")]
        public IActionResult AssignPermissionToRole([FromBody] AssignPermissionRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            if (!db.CheckPermission(r.UserId, "PERMISSION_MANAGEMENT_WRITE"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to assign permissions.";
                return BadRequest(resp);
            }

            resp = db.AssignPermissionToRole(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("AssignUserRole")]
        public IActionResult AssignUserRole([FromBody] AssignUserRoleRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            if (!db.CheckPermission(r.RequestUserId, "USER_ROLE_MANAGEMENT_WRITE"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to assign user roles.";
                return BadRequest(resp);
            }

            resp = db.AssignUserRole(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetUserPermissions")]
        public IActionResult GetUserPermissions([FromBody] GetRoleRequest r)
        {
            PermissionResponse resp = new PermissionResponse();

            resp = db.GetUserPermissions(r.UserId);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // SAMPLE FOR CUSTOMERLIST AND USERLIST

        [HttpPost("GetAllUsers")]
        public IActionResult GetAllUsers([FromBody] GetUserListRequest r)
        {
            GetUserListResponse resp = new GetUserListResponse();

            if (!db.CheckPermission(r.UserId, "USER_LIST_READ"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to view all users.";
                return BadRequest(resp);
            }

            resp = db.GetUserList(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetCustomers")]
        public IActionResult GetCustomers([FromBody] GetUserListRequest r)
        {
            GetUserListResponse resp = new GetUserListResponse();

            if (!db.CheckPermission(r.UserId, "CUSTOMER_LIST_READ"))
            {
                resp.isSuccess = false;
                resp.Message = "Permission denied. You don't have access to view customers.";
                return BadRequest(resp);
            }

            resp = db.GetCustomerUsers();

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
    }
}