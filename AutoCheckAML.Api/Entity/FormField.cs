using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Campo de un formulario.
    /// Define cada pregunta/campo que compone un formulario.
    /// </summary>
    public class FormField : AuditableEntity
    {
        /// <summary>
        /// ID de la plantilla a la que pertenece.
        /// </summary>
        public int FormTemplateId { get; set; }

        /// <summary>
        /// Etiqueta o pregunta del campo.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Descripción o instrucción para el campo.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo de campo: Text, TextArea, Number, Date, Select, MultiSelect, Checkbox, Radio, File
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// Indica si el campo es obligatorio.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Orden de aparición en el formulario.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Opciones para campos Select/Radio (JSON).
        /// Formato: [{"value": "opcion1", "label": "Opción 1"}, ...]
        /// </summary>
        public string Options { get; set; }

        /// <summary>
        /// Validaciones personalizadas (JSON).
        /// Formato: {"minLength": 5, "maxLength": 50, "pattern": "regex", ...}
        /// </summary>
        public string ValidationRules { get; set; }

        /// <summary>
        /// Valor predeterminado del campo.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Indicador si el campo está activo.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual FormTemplate FormTemplate { get; set; }
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
