namespace CompleteSystem.Models.DTOs;

public class RegistroAcessoRespostaDTO
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TipoAcao { get; set; } = string.Empty;
    public string MetodoHttp { get; set; } = string.Empty;
    public string Caminho { get; set; } = string.Empty;
    public int CodigoStatus { get; set; }
    public string EnderecoIp { get; set; } = string.Empty;
    public string Navegador { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public DateTime OcorridoEm { get; set; }
}
