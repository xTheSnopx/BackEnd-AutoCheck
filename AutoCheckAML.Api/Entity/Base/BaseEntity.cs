namespace AutoCheckAML.Api.Entity.Base
{
    /// <summary>
    /// Clase base para todas las entidades del sistema.
    /// Proporciona propiedades comunes: Id y CreatedAt
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Identificador único de la entidad.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora de creación del registro en UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
