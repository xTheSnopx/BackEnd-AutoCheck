using System;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class CrewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagedByUserId { get; set; }
        public string ManagedByUserName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public int MemberCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCrewRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagedByUserId { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
    }

    public class UpdateCrewRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagedByUserId { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }
}
