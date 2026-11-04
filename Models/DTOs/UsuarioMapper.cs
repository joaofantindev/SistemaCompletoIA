namespace CompleteSystem.Models.DTOs;

public static class UsuarioMapper
{
    public static UsuarioResponseDTO ToResponseDTO(this Usuario usuario, string? perfil) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email ?? string.Empty,
        Ativo = usuario.Ativo,
        CriadoEm = usuario.CriadoEm,
        Perfil = perfil ?? string.Empty
    };

    public static UsuarioUpdateDTO ToUpdateDTO(this UsuarioResponseDTO usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Ativo = usuario.Ativo,
        Perfil = usuario.Perfil
    };
}
