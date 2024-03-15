using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.Migrations
{
    /// <inheritdoc />
    public partial class AddTestRelationAndNameProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TestEntity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RelationId",
                table: "TestEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestRelationEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRelationEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestEntity_RelationId",
                table: "TestEntity",
                column: "RelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestEntity_TestRelationEntity_RelationId",
                table: "TestEntity",
                column: "RelationId",
                principalTable: "TestRelationEntity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestEntity_TestRelationEntity_RelationId",
                table: "TestEntity");

            migrationBuilder.DropTable(
                name: "TestRelationEntity");

            migrationBuilder.DropIndex(
                name: "IX_TestEntity_RelationId",
                table: "TestEntity");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TestEntity");

            migrationBuilder.DropColumn(
                name: "RelationId",
                table: "TestEntity");
        }
    }
}
