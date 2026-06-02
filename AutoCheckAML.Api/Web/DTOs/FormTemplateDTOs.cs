using System;
using System.Collections.Generic;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class FormTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormType { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public bool RequiresSignature { get; set; }
        public int DisplayOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public List<FormFieldDto> FormFields { get; set; } = new List<FormFieldDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFormTemplateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormType { get; set; }
        public bool RequiresSignature { get; set; }
        public int DisplayOrder { get; set; }
        public List<CreateFormFieldRequest> FormFields { get; set; } = new List<CreateFormFieldRequest>();
    }

    public class UpdateFormTemplateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormType { get; set; }
        public bool IsActive { get; set; }
        public bool RequiresSignature { get; set; }
        public int DisplayOrder { get; set; }
        public List<UpdateFormFieldRequest> FormFields { get; set; } = new List<UpdateFormFieldRequest>();
    }
}
