using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Gta.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySqlSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_AcademicTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_AcademicTerms", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Action = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    EntityReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    CorrelationId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    RedactedDetailsJson = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_AuditLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    SubjectCode = table.Column<string>(type: "longtext", nullable: false),
                    CatalogNumber = table.Column<string>(type: "longtext", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_Courses", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_EmailOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Recipient = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    TextBody = table.Column<string>(type: "longtext", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_EmailOutboxMessages", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    NormalizedName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_Roles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_SectionImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ImportedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ImportedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    AcceptedRows = table.Column<int>(type: "int", nullable: false),
                    RejectedRows = table.Column<int>(type: "int", nullable: false),
                    ErrorSummaryJson = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_SectionImportBatches", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Key = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Value = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    IsDevelopmentOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_SystemSettings", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UniversityId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_Users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_ApplicationPhases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Program = table.Column<string>(type: "longtext", nullable: false),
                    OpensAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ClosesAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_ApplicationPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_ApplicationPhases_gta_AcademicTerms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalTable: "gta_AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_CourseSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "char(36)", nullable: false),
                    SectionNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Schedule = table.Column<string>(type: "longtext", nullable: true),
                    DeliveryMethod = table.Column<string>(type: "longtext", nullable: true),
                    AvailablePositions = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_CourseSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_CourseSections_gta_AcademicTerms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalTable: "gta_AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_CourseSections_gta_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "gta_Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_ApplicantProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PreferredName = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    Program = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Degree = table.Column<string>(type: "longtext", nullable: true),
                    Major = table.Column<string>(type: "longtext", nullable: true),
                    Gpa = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    ExpectedGraduationTerm = table.Column<string>(type: "longtext", nullable: true),
                    ExpectedGraduationYear = table.Column<int>(type: "int", nullable: true),
                    LinkedInUrl = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_ApplicantProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_ApplicantProfiles_gta_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "gta_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    StorageKey = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    MediaType = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    ByteLength = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    ActiveSlot = table.Column<int>(type: "int", nullable: true, computedColumnSql: "CASE WHEN `State` = 1 THEN 1 ELSE NULL END", stored: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_Documents_gta_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "gta_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_gta_UserRoles_gta_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "gta_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_UserRoles_gta_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "gta_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicationPhaseId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Reference = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    EmploymentBasis = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_Applications_gta_ApplicationPhases_ApplicationPhaseId",
                        column: x => x.ApplicationPhaseId,
                        principalTable: "gta_ApplicationPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_Applications_gta_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "gta_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_FacultySectionAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    FacultyUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseSectionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_FacultySectionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_FacultySectionAssignments_gta_CourseSections_CourseSecti~",
                        column: x => x.CourseSectionId,
                        principalTable: "gta_CourseSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_FacultySectionAssignments_gta_Users_FacultyUserId",
                        column: x => x.FacultyUserId,
                        principalTable: "gta_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_EducationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicantProfileId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Institution = table.Column<string>(type: "longtext", nullable: false),
                    Degree = table.Column<string>(type: "longtext", nullable: true),
                    FieldOfStudy = table.Column<string>(type: "longtext", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_EducationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_EducationRecords_gta_ApplicantProfiles_ApplicantProfileId",
                        column: x => x.ApplicantProfileId,
                        principalTable: "gta_ApplicantProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_ExperienceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicantProfileId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Organization = table.Column<string>(type: "longtext", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsGtaExperience = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_ExperienceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_ExperienceRecords_gta_ApplicantProfiles_ApplicantProfile~",
                        column: x => x.ApplicantProfileId,
                        principalTable: "gta_ApplicantProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_ApplicationChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseSectionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PreferenceOrder = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_ApplicationChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_ApplicationChoices_gta_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "gta_Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_ApplicationChoices_gta_CourseSections_CourseSectionId",
                        column: x => x.CourseSectionId,
                        principalTable: "gta_CourseSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_ApplicationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FromState = table.Column<int>(type: "int", nullable: false),
                    ToState = table.Column<int>(type: "int", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_ApplicationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_ApplicationStatusHistory_gta_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "gta_Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_FacultyReviewActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicationChoiceId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FacultyUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_FacultyReviewActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_FacultyReviewActions_gta_ApplicationChoices_ApplicationC~",
                        column: x => x.ApplicationChoiceId,
                        principalTable: "gta_ApplicationChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_FacultyReviewActions_gta_Users_FacultyUserId",
                        column: x => x.FacultyUserId,
                        principalTable: "gta_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gta_Placements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApplicationChoiceId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseSectionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ActiveSlot = table.Column<int>(type: "int", nullable: true, computedColumnSql: "CASE WHEN `IsActive` = 1 THEN 1 ELSE NULL END", stored: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gta_Placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gta_Placements_gta_ApplicationChoices_ApplicationChoiceId",
                        column: x => x.ApplicationChoiceId,
                        principalTable: "gta_ApplicationChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gta_Placements_gta_CourseSections_CourseSectionId",
                        column: x => x.CourseSectionId,
                        principalTable: "gta_CourseSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_gta_ApplicantProfiles_UserId",
                table: "gta_ApplicantProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_ApplicationChoices_ApplicationId_CourseSectionId",
                table: "gta_ApplicationChoices",
                columns: new[] { "ApplicationId", "CourseSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_ApplicationChoices_CourseSectionId",
                table: "gta_ApplicationChoices",
                column: "CourseSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_ApplicationPhases_AcademicTermId",
                table: "gta_ApplicationPhases",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_Applications_ApplicantUserId_ApplicationPhaseId",
                table: "gta_Applications",
                columns: new[] { "ApplicantUserId", "ApplicationPhaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_Applications_ApplicationPhaseId",
                table: "gta_Applications",
                column: "ApplicationPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_Applications_Reference",
                table: "gta_Applications",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_ApplicationStatusHistory_ApplicationId",
                table: "gta_ApplicationStatusHistory",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_AuditLogs_CorrelationId",
                table: "gta_AuditLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_AuditLogs_OccurredAtUtc",
                table: "gta_AuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_gta_CourseSections_AcademicTermId",
                table: "gta_CourseSections",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_CourseSections_CourseId_AcademicTermId_SectionNumber",
                table: "gta_CourseSections",
                columns: new[] { "CourseId", "AcademicTermId", "SectionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_Documents_OwnerUserId_Type_ActiveSlot",
                table: "gta_Documents",
                columns: new[] { "OwnerUserId", "Type", "ActiveSlot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_Documents_OwnerUserId_Type_State",
                table: "gta_Documents",
                columns: new[] { "OwnerUserId", "Type", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_gta_Documents_StorageKey",
                table: "gta_Documents",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_EducationRecords_ApplicantProfileId",
                table: "gta_EducationRecords",
                column: "ApplicantProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_EmailOutboxMessages_State_NextAttemptAtUtc",
                table: "gta_EmailOutboxMessages",
                columns: new[] { "State", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_gta_ExperienceRecords_ApplicantProfileId",
                table: "gta_ExperienceRecords",
                column: "ApplicantProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_FacultyReviewActions_ApplicationChoiceId",
                table: "gta_FacultyReviewActions",
                column: "ApplicationChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_FacultyReviewActions_FacultyUserId",
                table: "gta_FacultyReviewActions",
                column: "FacultyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_FacultySectionAssignments_CourseSectionId",
                table: "gta_FacultySectionAssignments",
                column: "CourseSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_FacultySectionAssignments_FacultyUserId_CourseSectionId_~",
                table: "gta_FacultySectionAssignments",
                columns: new[] { "FacultyUserId", "CourseSectionId", "IsActive" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_Placements_ApplicationChoiceId_CourseSectionId_ActiveSlot",
                table: "gta_Placements",
                columns: new[] { "ApplicationChoiceId", "CourseSectionId", "ActiveSlot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_Placements_CourseSectionId_IsActive",
                table: "gta_Placements",
                columns: new[] { "CourseSectionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_gta_Roles_NormalizedName",
                table: "gta_Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_SectionImportBatches_ImportedAtUtc",
                table: "gta_SectionImportBatches",
                column: "ImportedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_gta_SystemSettings_Key",
                table: "gta_SystemSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_UserRoles_RoleId",
                table: "gta_UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_gta_Users_NormalizedEmail",
                table: "gta_Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gta_Users_UniversityId",
                table: "gta_Users",
                column: "UniversityId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gta_ApplicationStatusHistory");

            migrationBuilder.DropTable(
                name: "gta_AuditLogs");

            migrationBuilder.DropTable(
                name: "gta_Documents");

            migrationBuilder.DropTable(
                name: "gta_EducationRecords");

            migrationBuilder.DropTable(
                name: "gta_EmailOutboxMessages");

            migrationBuilder.DropTable(
                name: "gta_ExperienceRecords");

            migrationBuilder.DropTable(
                name: "gta_FacultyReviewActions");

            migrationBuilder.DropTable(
                name: "gta_FacultySectionAssignments");

            migrationBuilder.DropTable(
                name: "gta_Placements");

            migrationBuilder.DropTable(
                name: "gta_SectionImportBatches");

            migrationBuilder.DropTable(
                name: "gta_SystemSettings");

            migrationBuilder.DropTable(
                name: "gta_UserRoles");

            migrationBuilder.DropTable(
                name: "gta_ApplicantProfiles");

            migrationBuilder.DropTable(
                name: "gta_ApplicationChoices");

            migrationBuilder.DropTable(
                name: "gta_Roles");

            migrationBuilder.DropTable(
                name: "gta_Applications");

            migrationBuilder.DropTable(
                name: "gta_CourseSections");

            migrationBuilder.DropTable(
                name: "gta_ApplicationPhases");

            migrationBuilder.DropTable(
                name: "gta_Users");

            migrationBuilder.DropTable(
                name: "gta_Courses");

            migrationBuilder.DropTable(
                name: "gta_AcademicTerms");
        }
    }
}
