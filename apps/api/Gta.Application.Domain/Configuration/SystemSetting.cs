using Gta.Application.Domain.Common;

namespace Gta.Application.Domain.Configuration;

public sealed class SystemSetting : AuditableEntity
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required string Description { get; set; }
    public bool IsDevelopmentOnly { get; set; }
}
