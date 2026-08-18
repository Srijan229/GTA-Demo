using System.Data;
using System.Text.Json;
using Gta.Application.Application.Administration;
using Gta.Application.Contracts.Administration;
using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Auditing;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Administration;

public sealed class SectionImportService(GtaDbContext db) : ISectionImportService
{
    private static readonly string[] Headers = ["TermCode", "TermName", "TermStart", "TermEnd", "SubjectCode", "CatalogNumber", "CourseTitle", "SectionNumber", "Schedule", "DeliveryMethod", "AvailablePositions", "IsActive"];
    public async Task<SectionImportPreviewResponse> PreviewAsync(Stream content, CancellationToken token) { var parsed = await ParseAsync(content, token); return new(parsed.Rows.Count, parsed.Valid.Count, parsed.Errors.Count, parsed.Errors); }
    public async Task<IReadOnlyCollection<SectionImportHistoryResponse>> GetHistoryAsync(CancellationToken token) => (await db.SectionImportBatches.AsNoTracking().OrderByDescending(x => x.ImportedAtUtc).Take(100).ToListAsync(token)).Select(Map).ToArray();
    public async Task<SectionImportHistoryResponse> ImportAsync(Stream content, string fileName, Guid actorId, string correlationId, CancellationToken token)
    {
        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Upload a CSV file.");
        var parsed = await ParseAsync(content, token); if (parsed.Valid.Count == 0) throw new ArgumentException("The file contains no valid section rows.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token); var now = DateTimeOffset.UtcNow;
        foreach (var row in parsed.Valid)
        {
            var term = await db.AcademicTerms.SingleOrDefaultAsync(x => x.Code == row.TermCode, token) ?? new AcademicTerm { Code = row.TermCode, Name = row.TermName, StartsOn = row.TermStart, EndsOn = row.TermEnd };
            if (term.Id == Guid.Empty || db.Entry(term).State == EntityState.Detached) db.AcademicTerms.Add(term); else { term.Name = row.TermName; term.StartsOn = row.TermStart; term.EndsOn = row.TermEnd; }
            var course = await db.Courses.SingleOrDefaultAsync(x => x.SubjectCode == row.SubjectCode && x.CatalogNumber == row.CatalogNumber, token) ?? new Course { SubjectCode = row.SubjectCode, CatalogNumber = row.CatalogNumber, Title = row.CourseTitle };
            if (db.Entry(course).State == EntityState.Detached) db.Courses.Add(course); else course.Title = row.CourseTitle;
            await db.SaveChangesAsync(token);
            var section = await db.CourseSections.SingleOrDefaultAsync(x => x.CourseId == course.Id && x.AcademicTermId == term.Id && x.SectionNumber == row.SectionNumber, token);
            if (section is null) db.CourseSections.Add(new CourseSection { CourseId = course.Id, AcademicTermId = term.Id, SectionNumber = row.SectionNumber, Schedule = row.Schedule, DeliveryMethod = row.DeliveryMethod, AvailablePositions = row.Positions, IsActive = row.IsActive, CreatedAtUtc = now, UpdatedAtUtc = now });
            else { section.Schedule = row.Schedule; section.DeliveryMethod = row.DeliveryMethod; section.AvailablePositions = row.Positions; section.IsActive = row.IsActive; section.UpdatedAtUtc = now; }
        }
        var batch = new SectionImportBatch { FileName = Path.GetFileName(fileName), ImportedAtUtc = now, ImportedByUserId = actorId, TotalRows = parsed.Rows.Count, AcceptedRows = parsed.Valid.Count, RejectedRows = parsed.Errors.Count, ErrorSummaryJson = JsonSerializer.Serialize(parsed.Errors) };
        db.SectionImportBatches.Add(batch); db.AuditLogs.Add(new AuditLog { ActorUserId = actorId, OccurredAtUtc = now, Action = "SectionsImported", EntityType = "SectionImportBatch", EntityReference = batch.Id.ToString(), Result = "Succeeded", CorrelationId = correlationId, RedactedDetailsJson = JsonSerializer.Serialize(new { batch.AcceptedRows, batch.RejectedRows }) });
        await db.SaveChangesAsync(token); await tx.CommitAsync(token); return Map(batch);
    }
    private static async Task<Parsed> ParseAsync(Stream content, CancellationToken token)
    {
        using var reader = new StreamReader(content, leaveOpen: true); var headerLine = await reader.ReadLineAsync(token) ?? throw new ArgumentException("The CSV file is empty."); var header = Split(headerLine);
        if (!Headers.SequenceEqual(header, StringComparer.OrdinalIgnoreCase)) throw new ArgumentException($"CSV columns must be: {string.Join(",", Headers)}");
        var rows = new List<string[]>(); var valid = new List<Row>(); var errors = new List<SectionImportError>(); var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase); var number = 1;
        while (await reader.ReadLineAsync(token) is { } line) { number++; if (string.IsNullOrWhiteSpace(line)) continue; var values = Split(line); rows.Add(values); try { if (values.Length != Headers.Length) throw new FormatException("Incorrect column count."); var key = $"{values[0]}|{values[4]}|{values[5]}|{values[7]}"; if (!keys.Add(key)) throw new FormatException("Duplicate term/course/section in file."); if (!DateOnly.TryParse(values[2], out var start) || !DateOnly.TryParse(values[3], out var end) || start > end) throw new FormatException("Invalid term dates."); if (!int.TryParse(values[10], out var positions) || positions < 0) throw new FormatException("AvailablePositions must be zero or greater."); if (!bool.TryParse(values[11], out var active)) throw new FormatException("IsActive must be true or false."); if (new[] { values[0], values[1], values[4], values[5], values[6], values[7] }.Any(string.IsNullOrWhiteSpace)) throw new FormatException("Required values are missing."); valid.Add(new(values[0].Trim(), values[1].Trim(), start, end, values[4].Trim().ToUpperInvariant(), values[5].Trim(), values[6].Trim(), values[7].Trim(), Clean(values[8]), Clean(values[9]), positions, active)); } catch (FormatException ex) { errors.Add(new(number, ex.Message)); } }
        return new(rows, valid, errors);
    }
    private static string[] Split(string line) { var result = new List<string>(); var value = ""; var quoted = false; for (var i = 0; i < line.Length; i++) { var c = line[i]; if (c == '"') { if (quoted && i + 1 < line.Length && line[i + 1] == '"') { value += '"'; i++; } else quoted = !quoted; } else if (c == ',' && !quoted) { result.Add(value); value = ""; } else value += c; } result.Add(value); return result.ToArray(); }
    private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static SectionImportHistoryResponse Map(SectionImportBatch x) => new(x.Id, x.FileName, x.ImportedAtUtc, x.TotalRows, x.AcceptedRows, x.RejectedRows, string.IsNullOrWhiteSpace(x.ErrorSummaryJson) ? [] : JsonSerializer.Deserialize<SectionImportError[]>(x.ErrorSummaryJson) ?? []);
    private sealed record Row(string TermCode, string TermName, DateOnly TermStart, DateOnly TermEnd, string SubjectCode, string CatalogNumber, string CourseTitle, string SectionNumber, string? Schedule, string? DeliveryMethod, int Positions, bool IsActive);
    private sealed record Parsed(List<string[]> Rows, List<Row> Valid, List<SectionImportError> Errors);
}
