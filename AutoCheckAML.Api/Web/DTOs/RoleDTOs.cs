using System;
using System.Collections.Generic;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int HierarchyLevel { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int HierarchyLevel { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UpdateRoleRequest
    {
        public string Description { get; set; }
        public int HierarchyLevel { get; set; }
        public bool IsActive { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
