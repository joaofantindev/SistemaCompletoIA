# relembrando conceitos importantes

- DTO é **classe** de transporte
- DataAnnotations validam formato de campo, o Service valida regra de negócio ("usuario ja cadastrado, senha errada")

---

# entendendo o (u => u.X);


```cs
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entidade =>
            {
                entidade.ToTable("Usuarios");
                entidade.HasKey(u => u.Id);

                entidade.Property(u => u.Nome).HasMaxLength(100).IsRequired();

                entidade.HasIndex(u => u.Email).IsUnique();

                entidade.Property(u => u.SenhaHash).HasMaxLength(255).IsRequired();
                
            });
        }

```

