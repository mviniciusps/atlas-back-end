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

    public virtual DbSet<DocumentoProcesso> DocumentoProcessos { get; set; }

    public virtual DbSet<Duimp> Duimps { get; set; }

    public virtual DbSet<EstabelecimentoImportador> EstabelecimentoImportadors { get; set; }

    public virtual DbSet<Importador> Importadors { get; set; }

    public virtual DbSet<ItemDuimp> ItemDuimps { get; set; }

    public virtual DbSet<Processo> Processos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentoProcesso>(entity =>
        {
            entity.HasKey(e => e.IdDocumento);

            entity.ToTable("DocumentoProcesso");

            entity.Property(e => e.CaminhoArmazenamento).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(150);
            entity.Property(e => e.DataUpload)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_DocumentoProcesso_DataUpload");
            entity.Property(e => e.NomeArquivo).HasMaxLength(260);
            entity.Property(e => e.StatusConferencia)
                .HasMaxLength(30)
                .HasDefaultValue("Aguardando conferência", "DF_DocumentoProcesso_StatusConferencia");
            entity.Property(e => e.TipoDocumento).HasMaxLength(80);

            entity.HasOne(d => d.IdProcessoNavigation).WithMany(p => p.DocumentoProcessos)
                .HasForeignKey(d => d.IdProcesso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentoProcesso_Processo");
        });

        modelBuilder.Entity<Duimp>(entity =>
        {
            entity.HasKey(e => e.IdDuimp);

            entity.ToTable("Duimp");

            entity.HasIndex(e => e.IdProcesso, "UQ_Duimp_IdProcesso").IsUnique();

            entity.Property(e => e.BairroImportador).HasMaxLength(100);
            entity.Property(e => e.CepImportador)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.CodigoMunicipioImportador)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ComplementoEnderecoImportador).HasMaxLength(100);
            entity.Property(e => e.DataAtualizacao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Duimp_DataAtualizacao");
            entity.Property(e => e.DataCriacao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Duimp_DataCriacao");
            entity.Property(e => e.DocumentoImportador)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.LogradouroImportador).HasMaxLength(200);
            entity.Property(e => e.MunicipioImportador).HasMaxLength(100);
            entity.Property(e => e.NomeImportador)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NumeroEnderecoImportador).HasMaxLength(30);
            entity.Property(e => e.PaisImportador)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.StatusIntegracaoPortal)
                .HasMaxLength(30)
                .HasDefaultValue("NaoEnviada", "DF_Duimp_StatusIntegracaoPortal");
            entity.Property(e => e.StatusRascunho)
                .HasMaxLength(30)
                .HasDefaultValue("Rascunho", "DF_Duimp_StatusRascunho");
            entity.Property(e => e.TipoPessoaImportador)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UfImportador)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdProcessoNavigation).WithOne(p => p.DuimpNavigation)
                .HasForeignKey<Duimp>(d => d.IdProcesso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Duimp_Processo");
        });

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

        modelBuilder.Entity<ItemDuimp>(entity =>
        {
            entity.HasKey(e => e.IdItemDuimp);

            entity.ToTable("ItemDuimp");

            entity.HasIndex(e => new { e.IdDuimp, e.NumeroItem }, "UQ_ItemDuimp_Duimp_NumeroItem").IsUnique();

            entity.Property(e => e.Aplicacao).HasMaxLength(100);
            entity.Property(e => e.CaracterizacaoImportacao).HasMaxLength(60);
            entity.Property(e => e.CoberturaCambial).HasMaxLength(50);
            entity.Property(e => e.CodigoExportador).HasMaxLength(100);
            entity.Property(e => e.CodigoFabricante).HasMaxLength(100);
            entity.Property(e => e.CodigoMunicipioConsumo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CondicaoMercadoria).HasMaxLength(20);
            entity.Property(e => e.DataAtualizacao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ItemDuimp_DataAtualizacao");
            entity.Property(e => e.DataCriacao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ItemDuimp_DataCriacao");
            entity.Property(e => e.EnderecoExportador).HasMaxLength(500);
            entity.Property(e => e.EnderecoFabricante).HasMaxLength(500);
            entity.Property(e => e.Incoterm)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MetodoValoracao).HasMaxLength(100);
            entity.Property(e => e.MoedaNegociada)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MunicipioConsumo).HasMaxLength(100);
            entity.Property(e => e.Ncm)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.NomeExportador).HasMaxLength(200);
            entity.Property(e => e.NomeFabricante).HasMaxLength(200);
            entity.Property(e => e.NomeProduto).HasMaxLength(200);
            entity.Property(e => e.NumeroSerie).HasMaxLength(150);
            entity.Property(e => e.PaisExportador)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PaisOrigem)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PesoLiquido).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.QuantidadeMedidaEstatistica).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.QuantidadeUnidadeComercializada).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.UfConsumo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UnidadeComercializada).HasMaxLength(30);
            entity.Property(e => e.UnidadeMedidaEstatistica).HasMaxLength(30);
            entity.Property(e => e.ValorUnitarioCondicaoVenda).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.VersaoExportador).HasMaxLength(50);
            entity.Property(e => e.VersaoFabricante).HasMaxLength(50);
            entity.Property(e => e.VersaoProduto).HasMaxLength(50);

            entity.HasOne(d => d.IdDuimpNavigation).WithMany(p => p.ItemDuimps)
                .HasForeignKey(d => d.IdDuimp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemDuimp_Duimp");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
