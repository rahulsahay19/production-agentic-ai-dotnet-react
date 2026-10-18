using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexaStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOptimisticConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL already provides xmin as a system column on every table.
            // This migration records the EF model change without altering the schema.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No schema change is required when removing the xmin mapping.
        }
    }
}
