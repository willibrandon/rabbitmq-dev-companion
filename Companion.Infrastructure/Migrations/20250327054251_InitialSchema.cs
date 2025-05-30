﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Companion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "topologies",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topologies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bindings",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    source_exchange = table.Column<string>(type: "text", nullable: false),
                    destination_queue = table.Column<string>(type: "text", nullable: false),
                    routing_key = table.Column<string>(type: "text", nullable: false),
                    arguments = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    TopologyId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bindings", x => x.id);
                    table.ForeignKey(
                        name: "FK_bindings_topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "topologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exchanges",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    durable = table.Column<bool>(type: "boolean", nullable: false),
                    auto_delete = table.Column<bool>(type: "boolean", nullable: false),
                    @internal = table.Column<bool>(name: "internal", type: "boolean", nullable: false),
                    arguments = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    TopologyId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchanges", x => x.id);
                    table.ForeignKey(
                        name: "FK_exchanges_topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "topologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "queues",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    durable = table.Column<bool>(type: "boolean", nullable: false),
                    exclusive = table.Column<bool>(type: "boolean", nullable: false),
                    auto_delete = table.Column<bool>(type: "boolean", nullable: false),
                    arguments = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    max_length = table.Column<int>(type: "integer", nullable: true),
                    message_ttl = table.Column<int>(type: "integer", nullable: true),
                    dead_letter_exchange = table.Column<string>(type: "text", nullable: true),
                    dead_letter_routing_key = table.Column<string>(type: "text", nullable: true),
                    TopologyId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queues", x => x.id);
                    table.ForeignKey(
                        name: "FK_queues_topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "topologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bindings_TopologyId",
                table: "bindings",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_exchanges_TopologyId",
                table: "exchanges",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_queues_TopologyId",
                table: "queues",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bindings");

            migrationBuilder.DropTable(
                name: "exchanges");

            migrationBuilder.DropTable(
                name: "queues");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "topologies");
        }
    }
}
