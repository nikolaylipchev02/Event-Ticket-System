using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketsInventoryDbName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_tables
                        WHERE schemaname = 'public' AND tablename = 'ticketsInventory'
                    ) THEN
                        ALTER TABLE public."ticketsInventory" RENAME TO tickets_inventory;
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_indexes
                        WHERE schemaname = 'public' AND indexname = 'IX_ticketsInventory_EventId'
                    ) THEN
                        ALTER INDEX public."IX_ticketsInventory_EventId" RENAME TO "IX_tickets_inventory_EventId";
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'PK_ticketsInventory'
                    ) THEN
                        ALTER TABLE public.tickets_inventory RENAME CONSTRAINT "PK_ticketsInventory" TO "PK_tickets_inventory";
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS public.tickets_inventory (
                    "Id" uuid NOT NULL,
                    "EventId" uuid NOT NULL,
                    "RemainingTickets" integer NOT NULL,
                    CONSTRAINT "PK_tickets_inventory" PRIMARY KEY ("Id")
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_tickets_inventory_EventId"
                ON public.tickets_inventory ("EventId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TABLE IF EXISTS public.tickets_inventory;
                """);
        }
    }
}
