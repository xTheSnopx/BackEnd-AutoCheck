namespace AutoCheckAML.Api.Entity.Base
{
    /// <summary>
    /// Clase base para todas las entidades del sistema.
    /// Proporciona propiedades comunes: Id y CreatedAt
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
