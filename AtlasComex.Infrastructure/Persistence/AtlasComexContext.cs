using System;
using System.Collections.Generic;
using AtlasComex.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AtlasComex.Infrastructure.Persistence;

public partial class AtlasComexContext : DbContext
{
    public AtlasComexContext(DbContextOptions<AtlasComexContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EstabelecimentoImportador> EstabelecimentoImportadors { get; set; }

    public virtual DbSet<Importador> Importadors { get; set; }

    public virtual DbSet<Processo> Processos { get; set; }

    public virtual DbSet<DocumentoProcesso> DocumentoProcessos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EstabelecimentoImportador>(entity =>
        {
            entity.HasKey(e => e.IdEstabelecimento);

            entity.ToTable("EstabelecimentoImportador");

            entity.HasIndex(e => e.Cnpj, "UQ_EstabelecimentoImportador_Cnpj").IsUnique();

            entity.Property(e => e.Ativo).HasDefaultValue(true, "DF_EstabelecimentoImportador_Ativo");
            entity.Property(e => e.Cnpj)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.NomeEstabelecimento).HasMaxLength(200);

            entity.HasOne(d => d.IdImportadorNavigation).WithMany(p => p.EstabelecimentoImportadors)
                .HasForeignKey(d => d.IdImportador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EstabelecimentoImportador_Importador");
        });

        modelBuilder.Entity<Importador>(entity =>
        {
            entity.HasKey(e => e.IdImportador);

            entity.ToTable("Importador");

            entity.Property(e => e.Ativo).HasDefaultValue(true, "DF_Importador_Ativo");
            entity.Property(e => e.DataCriacao).HasDefaultValueSql("(sysutcdatetime())", "DF_Importador_DataCriacao");
            entity.Property(e => e.NomeFantasia).HasMaxLength(200);
            entity.Property(e => e.RazaoSocial).HasMaxLength(200);
        });

        modelBuilder.Entity<Processo>(entity =>
        {
            entity.HasKey(e => e.IdProcesso);

            entity.ToTable("Processo");

            entity.HasIndex(e => new { e.IdEstabelecimento, e.NumeroProcesso }, "UQ_Processo_Estabelecimento_Numero").IsUnique();

            entity.Property(e => e.CeMercanteRuc).HasMaxLength(50);
            entity.Property(e => e.ConhecimentoHouse).HasMaxLength(50);
            entity.Property(e => e.ConhecimentoMaster).HasMaxLength(50);
            entity.Property(e => e.DataCriacao).HasDefaultValueSql("(sysutcdatetime())", "DF_Processo_DataCriacao");
            entity.Property(e => e.Duimp).HasMaxLength(20);
            entity.Property(e => e.Exportador).HasMaxLength(200);
            entity.Property(e => e.Modal).HasMaxLength(20);
            entity.Property(e => e.NumeroProcesso).HasMaxLength(30);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);
            entity.Property(e => e.RecintoAlfandegado).HasMaxLength(150);
            entity.Property(e => e.ReferenciaCliente).HasMaxLength(80);
            entity.Property(e => e.Situacao).HasMaxLength(100);

            entity.HasOne(d => d.IdEstabelecimentoNavigation).WithMany(p => p.Processos)
                .HasForeignKey(d => d.IdEstabelecimento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Processo_EstabelecimentoImportador");
        });

        modelBuilder.Entity<DocumentoProcesso>(entity =>
        {
            entity.HasKey(e => e.IdDocumento);

            entity.ToTable("DocumentoProcesso");

            entity.Property(e => e.CaminhoArmazenamento).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(150);
            entity.Property(e => e.DataUpload)
                .HasDefaultValueSql(
                    "(sysutcdatetime())",
                    "DF_DocumentoProcesso_DataUpload");
            entity.Property(e => e.IncluirNaDuimp)
                .HasDefaultValue(false, "DF_DocumentoProcesso_IncluirNaDuimp");
            entity.Property(e => e.NomeArquivo).HasMaxLength(260);
            entity.Property(e => e.TipoDocumento).HasMaxLength(80);

            entity.HasOne(d => d.IdProcessoNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdProcesso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentoProcesso_Processo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
