namespace YopoBackend.DTOs
{
    /// <summary>
    /// DTO for module information response.
    /// </summary>
    public class ModuleDto
    {
        /// <summary>
        /// Gets or sets the module ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the module name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the module description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether the module is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the module version.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets when the module was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the module was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for module list response.
    /// </summary>
    public class ModuleListDto
    {
        /// <summary>
        /// Gets or sets the list of modules.
        /// </summary>
        public List<ModuleDto> Modules { get; set; } = new();

        /// <summary>
        /// Gets or sets the total count of modules.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
