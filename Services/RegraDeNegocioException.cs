namespace CompleteSystem.Services;

/// <summary>
/// Erro esperado de regra de negócio. O controller traduz em mensagem para o usuário.
/// </summary>
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem)
    {
    }
}
