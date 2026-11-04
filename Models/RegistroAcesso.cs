namespace CompleteSystem.Models;

/// <summary>
/// Tipo de evento gravado no log de acessos e requisições.
/// </summary>
public enum TipoAcaoRegistro
{
    LoginBemSucedido = 1,
    LoginFalho = 2,
    Requisicao = 3,
    AcessoNegado = 4
}

/// <summary>
/// Log de acesso e de requisição HTTP. O administrador usa esses registros
/// para auditar quem entrou no sistema e o que foi acessado.
/// </summary>
public class RegistroAcesso
{
    public long Id { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string? Email { get; set; }
    public TipoAcaoRegistro TipoAcao { get; set; }
    public string MetodoHttp { get; set; } = string.Empty;
    public string Caminho { get; set; } = string.Empty;
    public int CodigoStatus { get; set; }
    public string? EnderecoIp { get; set; }
    public string? Navegador { get; set; }
    public string? Detalhes { get; set; }
    public DateTime OcorridoEm { get; set; } = DateTime.UtcNow;
}
