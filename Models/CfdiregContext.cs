using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Validacion.Models;

public partial class CfdiregContext : DbContext
{
    public CfdiregContext()
    {
    }

    public CfdiregContext(DbContextOptions<CfdiregContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cfdi> Cfdis { get; set; }

    public virtual DbSet<LogsValidacion> LogsValidacions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Database=CFDIReg;Integrated Security=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cfdi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CFDIs__3214EC07D03E1F13");

            entity.ToTable("CFDIs");

            entity.Property(e => e.Estatus).HasMaxLength(50);
            entity.Property(e => e.FechaEmision).HasColumnType("datetime");
            entity.Property(e => e.FolioFiscal).HasMaxLength(50);
            entity.Property(e => e.RfcEmisor).HasMaxLength(50);
            entity.Property(e => e.RfcReceptor).HasMaxLength(50);
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<LogsValidacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LogsVali__3214EC07F157B5AB");

            entity.ToTable("LogsValidacion");

            entity.Property(e => e.ErrorMessage).HasMaxLength(255);
            entity.Property(e => e.Estatus).HasMaxLength(50);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FolioFiscal).HasMaxLength(50);
            entity.Property(e => e.RfcEmisor).HasMaxLength(50);
            entity.Property(e => e.RfcReceptor).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
