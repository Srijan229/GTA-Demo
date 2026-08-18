using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Gta.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseApplicationConcurrencyTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Users",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_SystemSettings",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Placements",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_FacultySectionAssignments",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_FacultyReviewActions",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ExperienceRecords",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_EducationRecords",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Documents",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_CourseSections",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Applications",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ApplicationPhases",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ApplicationChoices",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ApplicantProfiles",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob",
                oldRowVersion: true)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Users",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_SystemSettings",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Placements",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_FacultySectionAssignments",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_FacultyReviewActions",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ExperienceRecords",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_EducationRecords",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Documents",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_CourseSections",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_Applications",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ApplicationPhases",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ApplicationChoices",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "gta_ApplicantProfiles",
                type: "longblob",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);
        }
    }
}
