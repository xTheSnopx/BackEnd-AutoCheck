using System;
using System.Collections.Generic;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? CrewId { get; set; }
        public string CrewName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int? CrewId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public int? CrewId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; }
    }
}
