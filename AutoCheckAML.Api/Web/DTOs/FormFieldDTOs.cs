using System;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class FormFieldDto
    {
        public int Id { get; set; }
        public int FormTemplateId { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string Options { get; set; }
        public string ValidationRules { get; set; }
        public string DefaultValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFormFieldRequest
    {
        public string Label { get; set; }
        public string Description { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string Options { get; set; }
        public string ValidationRules { get; set; }
        public string DefaultValue { get; set; }
    }

    public class UpdateFormFieldRequest
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string Options { get; set; }
        public string ValidationRules { get; set; }
        public string DefaultValue { get; set; }
        public bool IsActive { get; set; }
    }
}
