namespace CompleteSystem.Models.DTOs;

public static class RegistroAcessoMapperDTO
{
    public static RegistroAcessoRespostaDTO ToResponseDTO(this RegistroAcesso registro) => new()
    {
        Id = registro.Id,
        UsuarioId = registro.UsuarioId,
        NomeUsuario = registro.Usuario?.Nome ?? string.Empty,
        Email = registro.Email ?? string.Empty,
        TipoAcao = registro.TipoAcao.ToString(),
        MetodoHttp = registro.MetodoHttp,
        Caminho = registro.Caminho,
        CodigoStatus = registro.CodigoStatus,
        EnderecoIp = registro.EnderecoIp ?? string.Empty,
        Navegador = registro.Navegador ?? string.Empty,
        Detalhes = registro.Detalhes ?? string.Empty,
        OcorridoEm = registro.OcorridoEm
    };
}
