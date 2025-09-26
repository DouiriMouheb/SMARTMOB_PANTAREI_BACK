using Microsoft.EntityFrameworkCore;
using SMARTMOB_PANTAREI_BACK.Models;

namespace SMARTMOB_PANTAREI_BACK.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Acquisizioni> Acquisizioni { get; set; }
        public DbSet<Postazioni> Postazioni { get; set; }
        public DbSet<PostazioniPerLinea> PostazioniPerLinea { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Acquisizioni entity
            modelBuilder.Entity<Acquisizioni>(entity =>
            {
                entity.ToTable("ACQUISIZIONI");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CodLinea)
                    .HasColumnName("COD_LINEA")
                    .HasMaxLength(50);

                entity.Property(e => e.CodPostazione)
                    .HasColumnName("COD_POSTAZIONE")
                    .HasMaxLength(50);

                entity.Property(e => e.FotoAcquisizione)
                    .HasColumnName("FOTO_ACQUISIZIONE")
                    .HasMaxLength(255);

                entity.Property(e => e.CodiceArticolo)
                    .HasColumnName("CODICE_ARTICOLO")
                    .HasMaxLength(50);

                entity.Property(e => e.IdCatasta)
                    .HasColumnName("ID_CATASTA")
                    .HasMaxLength(50);

                entity.Property(e => e.AbilitaCq)
                    .HasColumnName("ABILITA_CQ")
                    .IsRequired();

                entity.Property(e => e.EsitoCqArticolo)
                    .HasColumnName("ESITO_CQ_ARTICOLO");

                entity.Property(e => e.NumSpineContate)
                    .HasColumnName("NUM_SPINE_CONTATE");

                entity.Property(e => e.NumSpineAttese)
                    .HasColumnName("NUM_SPINE_ATTESE");

                entity.Property(e => e.DataInserimento)
                    .HasColumnName("DT_INS")
                    .HasColumnType("datetime");

                entity.Property(e => e.DataAggiornamento)
                    .HasColumnName("DT_AGG")
                    .HasColumnType("datetime");

                entity.Property(e => e.Descrizione)
                    .HasColumnName("DESCRIZIONE")
                    .HasMaxLength(500);
            });

            // Configure Postazioni entity
            modelBuilder.Entity<Postazioni>(entity =>
            {
                entity.ToTable("AG_POSTAZIONI");
                entity.HasKey(e => new { e.CodLineaProd, e.CodPostazione });

                entity.Property(e => e.CodLineaProd)
                    .HasColumnName("COD_LINEA_PROD")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.CodPostazione)
                    .HasColumnName("COD_POSTAZIONE")
                    .HasMaxLength(50)
                    .IsRequired();

                // Temporarily commented out to test
                /*
                entity.Property(e => e.FlgRullieraBox)
                    .HasColumnName("FLG_RULLIERA0_BOX1")
                    .IsRequired();
                */

                entity.Property(e => e.DataInserimento)
                    .HasColumnName("DT_INS")
                    .HasColumnType("datetime");
            });

            // Configure PostazioniPerLinea view
            modelBuilder.Entity<PostazioniPerLinea>(entity =>
            {
                entity.ToTable("vw_PostazioniPerLinea");
                entity.HasKey(e => e.CodLinea);
                
                entity.Property(e => e.CodLinea)
                    .HasColumnName("COD_LINEA")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.CodPostazioneList)
                    .HasColumnName("COD_POSTAZIONE_LIST")
                    .HasMaxLength(4000); // Allow for concatenated values
            });
        }
    }
}