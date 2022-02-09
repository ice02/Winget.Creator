using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Winget.Pusher.Core.Models.Database;

namespace Winget.Creator.Database
{
    public class MSIXContext : DbContext
    {
        private readonly string _databasePath;
        public MSIXContext(string dbPath)
        {
            _databasePath = dbPath;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlite($@"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<MetadataMSIXTable>(
                    eb =>
                    {
                        eb.HasNoKey();
                    })
                .Entity<ProductCodesMapMSIXTable>(
                    eb =>
                    {
                        eb.HasNoKey();
                    })
                .Entity<PublishersMapMSIXTable>(
                    eb =>
                    {
                        eb.HasNoKey();
                    })
                ;
        }

        public DbSet<IdsMSIXTable> IdsMSIXTable { get; set; }
        public DbSet<ManifestMSIXTable> ManifestMSIXTable { get; set; }
        public DbSet<MetadataMSIXTable> MetadataMSIXTable { get; set; }
        public DbSet<PublishersMSIXTable> PublishersMSIXTable { get; set; }
        public DbSet<PublishersMapMSIXTable> PublishersMapMSIXTable { get; set; }
        public DbSet<PathPartsMSIXTable> PathPartsMSIXTable { get; set; }
        public DbSet<ProductCodesMSIXTable> ProductCodesMSIXTable { get; set; }
        public DbSet<ProductCodesMapMSIXTable> ProductCodesMapMSIXTable { get; set; }
        public DbSet<VersionsMSIXTable> VersionsMSIXTable { get; set; }
        public DbSet<NameTable> NameTable { get; set; }
    }
}
