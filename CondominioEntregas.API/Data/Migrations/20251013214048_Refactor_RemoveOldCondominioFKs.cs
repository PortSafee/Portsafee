using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_RemoveOldCondominioFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Run operations only if the old columns exist (makes this migration idempotent)
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Moradores' AND column_name='CondApartamentoId')
       OR EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Moradores' AND column_name='CondCasaId') THEN

        -- copy values into CondominioId when needed
        EXECUTE 'UPDATE ""Moradores"" SET ""CondominioId"" = COALESCE(""CondApartamentoId"", ""CondCasaId"", ""CondominioId"") WHERE ""CondominioId"" IS NULL';

        -- drop constraints and indexes and columns if they exist
        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Moradores' AND column_name='CondApartamentoId') THEN
            EXECUTE 'ALTER TABLE ""Moradores"" DROP CONSTRAINT IF EXISTS ""FK_Moradores_Condominios_CondApartamentoId""';
            EXECUTE 'DROP INDEX IF EXISTS ""IX_Moradores_CondApartamentoId""';
            EXECUTE 'ALTER TABLE ""Moradores"" DROP COLUMN IF EXISTS ""CondApartamentoId""';
        END IF;

        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Moradores' AND column_name='CondCasaId') THEN
            EXECUTE 'ALTER TABLE ""Moradores"" DROP CONSTRAINT IF EXISTS ""FK_Moradores_Condominios_CondCasaId""';
            EXECUTE 'DROP INDEX IF EXISTS ""IX_Moradores_CondCasaId""';
            EXECUTE 'ALTER TABLE ""Moradores"" DROP COLUMN IF EXISTS ""CondCasaId""';
        END IF;
    END IF;

    -- ensure index on CondominioId exists
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND tablename='Moradores' AND indexname='IX_Moradores_CondominioId') THEN
        EXECUTE 'CREATE INDEX ""IX_Moradores_CondominioId"" ON ""Moradores"" (""CondominioId"")';
    END IF;
END
$$;

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate old columns as nullable
            migrationBuilder.Sql(@"
                ALTER TABLE ""Moradores"" ADD COLUMN IF NOT EXISTS ""CondApartamentoId"" integer;
                ALTER TABLE ""Moradores"" ADD COLUMN IF NOT EXISTS ""CondCasaId"" integer;
            ");

            // Copy CondominioId back into both old columns (conservative approach)
            migrationBuilder.Sql(@"
                UPDATE ""Moradores"" SET ""CondApartamentoId"" = ""CondominioId"", ""CondCasaId"" = ""CondominioId"" WHERE ""CondominioId"" IS NOT NULL;
            ");

            // Recreate indexes and FKs
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Moradores_CondApartamentoId"" ON ""Moradores"" (""CondApartamentoId"");
                CREATE INDEX IF NOT EXISTS ""IX_Moradores_CondCasaId"" ON ""Moradores"" (""CondCasaId"");
                ALTER TABLE ""Moradores"" ADD CONSTRAINT IF NOT EXISTS ""FK_Moradores_Condominios_CondApartamentoId"" FOREIGN KEY (""CondApartamentoId"") REFERENCES ""Condominios"" (""Id"");
                ALTER TABLE ""Moradores"" ADD CONSTRAINT IF NOT EXISTS ""FK_Moradores_Condominios_CondCasaId"" FOREIGN KEY (""CondCasaId"") REFERENCES ""Condominios"" (""Id"");
            ");
        }
    }
}
