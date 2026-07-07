using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Tipos de vehículos configurables por el Administrador.
    /// Se usan como opciones del campo "Tipo de Vehículo" en el formulario.
    /// </summary>
    public class VehicleType : BaseEntity
    {
        /// <summary>
        /// Nombre del tipo de vehículo (ej: Camioneta, Camión, Volqueta, Moto).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción opcional del tipo de vehículo.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica si está activo para uso en formularios.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Orden de aparición en la lista desplegable.
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }
}
