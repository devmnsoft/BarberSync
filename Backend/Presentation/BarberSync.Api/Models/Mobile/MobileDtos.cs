namespace BarberSync.Api.Models.Mobile;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class MobilePagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<T> Items { get; set; } = new();
}

public class MobileLoginRequestDto { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class MobileLoginResponseDto { public string AccessToken { get; set; } = string.Empty; public DateTime ExpiresAtUtc { get; set; } }
public class MobileUsuarioDto { public Guid Id { get; set; } public string Nome { get; set; } = string.Empty; public string Perfil { get; set; } = string.Empty; }
public class MobileDashboardDto { public int ProximosAgendamentos { get; set; } public int ConvitesPendentes { get; set; } public decimal PagamentosPendentes { get; set; } }
public class MobilePlantaoDto { public Guid Id { get; set; } public string Titulo { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public decimal Valor { get; set; } }
public class MobileConviteDto { public Guid Id { get; set; } public string Origem { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; }
public class MobileEscalaDto { public Guid Id { get; set; } public DateTime DataInicio { get; set; } public DateTime DataFim { get; set; } public string Unidade { get; set; } = string.Empty; }
public class MobilePagamentoDto { public Guid Id { get; set; } public decimal Valor { get; set; } public string Status { get; set; } = string.Empty; public DateTime Competencia { get; set; } }
public class MobileNotificacaoDto { public Guid Id { get; set; } public string Titulo { get; set; } = string.Empty; public bool Lida { get; set; } }
public class MobilePerfilDto { public string Nome { get; set; } = string.Empty; public string Telefone { get; set; } = string.Empty; }
public class MobileDisponibilidadeDto { public bool Segunda { get; set; } public bool Terca { get; set; } public bool Quarta { get; set; } }
public class MobilePreferenciasDto { public bool ReceberPush { get; set; } public bool ReceberEmail { get; set; } }
