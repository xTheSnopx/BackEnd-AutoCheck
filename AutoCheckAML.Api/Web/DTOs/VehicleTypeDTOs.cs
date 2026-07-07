using System;using System.Collections.Generic;

namespace AutoCheckAML.Api.Web.DTOs
{
    /// <summary>
    /// DTO para tipos de vehículos configurables por Admin.
    /// </summary>
    public class VehicleTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVehicleTypeRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateVehicleTypeRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
