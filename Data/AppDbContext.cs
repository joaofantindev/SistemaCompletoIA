using CompleteSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Data;

/// <summary>
/// Contexto do banco. Além das entidades do sistema, herda de IdentityDbContext
/// para persistir usuários, perfis (roles), claims, logins e tokens do Identity.
/// </summary>
public class AppDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> opcoes) : base(opcoes)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<RegraCategoria> RegrasCategoria => Set<RegraCategoria>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<RegistroAcesso> RegistrosAcesso => Set<RegistroAcesso>();

    protected override void OnModelCreating(ModelBuilder construtorModelo)
    {
        base.OnModelCreating(construtorModelo);

        ConfigurarUsuario(construtorModelo);
        ConfigurarPerfisDoIdentity(construtorModelo);
        ConfigurarCategoria(construtorModelo);
        ConfigurarRegraCategoria(construtorModelo);
        ConfigurarProduto(construtorModelo);
        ConfigurarCompra(construtorModelo);
        ConfigurarRegistroAcesso(construtorModelo);
    }

    private static void ConfigurarUsuario(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<Usuario>(entidade =>
        {
            entidade.ToTable("Usuarios");
            entidade.Property(usuario => usuario.Nome).HasMaxLength(100).IsRequired();
            entidade.Property(usuario => usuario.Ativo).IsRequired();
            entidade.Property(usuario => usuario.CriadoEm).IsRequired();
            entidade.HasIndex(usuario => usuario.Email).IsUnique();
        });
    }

    private static void ConfigurarPerfisDoIdentity(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<IdentityRole<int>>().ToTable("Perfis");
        construtorModelo.Entity<IdentityUserRole<int>>().ToTable("UsuarioPerfis");
        construtorModelo.Entity<IdentityUserClaim<int>>().ToTable("UsuarioClaims");
        construtorModelo.Entity<IdentityUserLogin<int>>().ToTable("UsuarioLogins");
        construtorModelo.Entity<IdentityUserToken<int>>().ToTable("UsuarioTokens");
        construtorModelo.Entity<IdentityRoleClaim<int>>().ToTable("PerfilClaims");
    }

    private static void ConfigurarCategoria(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<Categoria>(entidade =>
        {
            entidade.ToTable("Categorias");
            entidade.HasKey(categoria => categoria.Id);

            entidade.Property(categoria => categoria.Nome)
                .HasMaxLength(100)
                .IsRequired();

            entidade.Property(categoria => categoria.Descricao)
                .HasMaxLength(300)
                .IsRequired();

            entidade.Property(categoria => categoria.Ativa).IsRequired();
            entidade.Property(categoria => categoria.CriadaEm).IsRequired();
            entidade.HasIndex(categoria => categoria.Nome).IsUnique();
        });
    }

    private static void ConfigurarRegraCategoria(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<RegraCategoria>(entidade =>
        {
            entidade.ToTable("RegrasCategoria");
            entidade.HasKey(regra => regra.Id);

            entidade.Property(regra => regra.Descricao)
                .HasMaxLength(200)
                .IsRequired();

            entidade.Property(regra => regra.PercentualDesconto)
                .HasPrecision(5, 2)
                .IsRequired();

            entidade.Property(regra => regra.Ativa).IsRequired();
            entidade.Property(regra => regra.CriadaEm).IsRequired();

            entidade.HasOne(regra => regra.Categoria)
                .WithMany(categoria => categoria.Regras)
                .HasForeignKey(regra => regra.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurarProduto(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<Produto>(entidade =>
        {
            entidade.ToTable("Produtos");
            entidade.HasKey(produto => produto.Id);

            entidade.Property(produto => produto.Nome)
                .HasMaxLength(100)
                .IsRequired();

            entidade.Property(produto => produto.Marca)
                .HasMaxLength(100)
                .IsRequired();

            entidade.Property(produto => produto.Preco)
                .HasPrecision(11, 2)
                .IsRequired();

            entidade.Property(produto => produto.CadastradoEm).IsRequired();

            entidade.HasOne(produto => produto.Categoria)
                .WithMany(categoria => categoria.Produtos)
                .HasForeignKey(produto => produto.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasOne(produto => produto.Proprietario)
                .WithMany(usuario => usuario.ProdutosCadastrados)
                .HasForeignKey(produto => produto.ProprietarioId)
                .OnDelete(DeleteBehavior.SetNull);

            entidade.HasIndex(produto => new { produto.CategoriaId, produto.Nome }).IsUnique();
        });
    }

    private static void ConfigurarCompra(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<Compra>(entidade =>
        {
            entidade.ToTable("Compras");
            entidade.HasKey(compra => compra.Id);

            entidade.Property(compra => compra.NomeProduto)
                .HasMaxLength(100)
                .IsRequired();

            entidade.Property(compra => compra.Quantidade).IsRequired();

            entidade.Property(compra => compra.ValorUnitario)
                .HasPrecision(11, 2)
                .IsRequired();

            entidade.Property(compra => compra.PercentualDescontoAplicado)
                .HasPrecision(5, 2)
                .IsRequired();

            entidade.Property(compra => compra.ValorTotal)
                .HasPrecision(11, 2)
                .IsRequired();

            entidade.Property(compra => compra.RealizadaEm).IsRequired();

            entidade.HasOne(compra => compra.Cliente)
                .WithMany(usuario => usuario.ComprasRealizadas)
                .HasForeignKey(compra => compra.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            entidade.HasOne(compra => compra.Produto)
                .WithMany(produto => produto.Compras)
                .HasForeignKey(compra => compra.ProdutoId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigurarRegistroAcesso(ModelBuilder construtorModelo)
    {
        construtorModelo.Entity<RegistroAcesso>(entidade =>
        {
            entidade.ToTable("RegistrosAcesso");
            entidade.HasKey(registro => registro.Id);

            entidade.Property(registro => registro.Email).HasMaxLength(256);
            entidade.Property(registro => registro.TipoAcao).IsRequired();
            entidade.Property(registro => registro.MetodoHttp).HasMaxLength(10).IsRequired();
            entidade.Property(registro => registro.Caminho).HasMaxLength(500).IsRequired();
            entidade.Property(registro => registro.CodigoStatus).IsRequired();
            entidade.Property(registro => registro.EnderecoIp).HasMaxLength(100);
            entidade.Property(registro => registro.Navegador).HasMaxLength(500);
            entidade.Property(registro => registro.Detalhes).HasMaxLength(1000);
            entidade.Property(registro => registro.OcorridoEm).IsRequired();

            entidade.HasOne(registro => registro.Usuario)
                .WithMany()
                .HasForeignKey(registro => registro.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            entidade.HasIndex(registro => registro.OcorridoEm);
            entidade.HasIndex(registro => registro.Email);
        });
    }
}
