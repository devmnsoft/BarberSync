using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Api.Services.Communication;

public sealed class CommunicationService(IConfiguration configuration, IHttpContextAccessor accessor)
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada.");
    private Guid Scope(string name) => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(name), out var id)
        ? id : throw new UnauthorizedAccessException($"Claim obrigatória {name} ausente ou inválida.");
    private Guid TenantId => Scope("tenant_id");
    private Guid BranchId => Scope("branch_id");
    private Guid? UserId => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? accessor.HttpContext?.User.FindFirstValue("sub"), out var id) ? id : null;
    private async Task<NpgsqlConnection> Open(CancellationToken ct) { var connection = new NpgsqlConnection(connectionString); await connection.OpenAsync(ct); return connection; }
    private void ScopeCommand(NpgsqlCommand command) { command.Parameters.AddWithValue("tenant", TenantId); command.Parameters.AddWithValue("branch", BranchId); }

    public async Task<JsonDocument> Dashboard(CancellationToken ct)
    {
        const string sql = """select jsonb_build_object('pending',count(*) filter(where status='Pending'),'sent',count(*) filter(where status='Sent'),'failed',count(*) filter(where status='Failed'),'skipped',count(*) filter(where status='Skipped'),'activeCampaigns',(select count(*) from barber.communication_campaigns where tenant_id=@tenant and branch_id=@branch and status in ('Scheduled','Running')),'activeTemplates',(select count(*) from barber.communication_templates where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null),'activeAutomations',(select count(*) from barber.communication_automations where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null),'providerNotConfigured',count(*) filter(where failure_reason='ProviderNotConfigured')) from barber.communication_outbox where tenant_id=@tenant and branch_id=@branch""";
        await using var connection = await Open(ct); await using var command = new NpgsqlCommand(sql, connection); ScopeCommand(command);
        return JsonDocument.Parse((await command.ExecuteScalarAsync(ct))?.ToString() ?? "{}");
    }

    public async Task<JsonDocument> List(string table, CancellationToken ct)
    {
        var allowed = new HashSet<string> { "communication_channels", "communication_templates", "communication_campaigns", "communication_automations", "communication_outbox", "notification_preferences", "notification_inbox", "communication_suppression_list" };
        if (!allowed.Contains(table)) throw new ArgumentOutOfRangeException(nameof(table));
        var recipientScope = table is "notification_inbox" or "notification_preferences" ? " and user_id=@user" : "";
        var sql = $"select coalesce(jsonb_agg(to_jsonb(x) order by x.created_at desc),'[]'::jsonb) from (select * from barber.{table} where tenant_id=@tenant and branch_id=@branch{recipientScope}" + (table.Contains("outbox") || table.Contains("inbox") || table.Contains("preferences") ? "" : " and deleted_at is null") + ") x";
        await using var connection = await Open(ct); await using var command = new NpgsqlCommand(sql, connection); ScopeCommand(command);
        if (recipientScope.Length > 0) command.Parameters.AddWithValue("user", (object?)UserId ?? DBNull.Value);
        return JsonDocument.Parse((await command.ExecuteScalarAsync(ct))?.ToString() ?? "[]");
    }

    public async Task<Guid> CreateTemplate(TemplateRequest request, CancellationToken ct)
    {
        ValidateTemplate(request); var id = Guid.NewGuid();
        const string sql = "insert into barber.communication_templates(id,tenant_id,branch_id,name,code,channel_type,subject,body,variables_json,status,created_by) values(@id,@tenant,@branch,@name,@code,@channel,@subject,@body,@variables::jsonb,@status,@user)";
        await using var connection = await Open(ct); await using var command = new NpgsqlCommand(sql, connection); ScopeCommand(command);
        command.Parameters.AddWithValue("id",id); command.Parameters.AddWithValue("name",request.Name.Trim()); command.Parameters.AddWithValue("code",request.Code.Trim()); command.Parameters.AddWithValue("channel",request.ChannelType); command.Parameters.AddWithValue("subject",(object?)request.Subject ?? DBNull.Value); command.Parameters.AddWithValue("body",request.Body); command.Parameters.AddWithValue("variables",NpgsqlDbType.Jsonb,JsonSerializer.Serialize(request.Variables ?? [])); command.Parameters.AddWithValue("status",request.Status); command.Parameters.AddWithValue("user",(object?)UserId ?? DBNull.Value); await command.ExecuteNonQueryAsync(ct); return id;
    }

    public async Task<Guid> CreateCampaign(CampaignRequest request, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(request.Name)||request.TemplateId==Guid.Empty||!AllowedChannels.Contains(request.ChannelType)||!AllowedAudiences.Contains(request.AudienceType)||request.AudienceType=="Segment"&&string.IsNullOrWhiteSpace(request.SegmentKey)) throw new CommunicationValidationException("Revise nome, template, canal e audiência selecionados.");
        var id=Guid.NewGuid(); const string sql="insert into barber.communication_campaigns(id,tenant_id,branch_id,name,description,audience_type,segment_key,template_id,channel_type,status,scheduled_at,created_by) values(@id,@tenant,@branch,@name,@description,@audience,@segment,@template,@channel,'Draft',@scheduled,@user)";
        await using var connection=await Open(ct); await using var command=new NpgsqlCommand(sql,connection); ScopeCommand(command); command.Parameters.AddWithValue("id",id);command.Parameters.AddWithValue("name",request.Name.Trim());command.Parameters.AddWithValue("description",(object?)request.Description??DBNull.Value);command.Parameters.AddWithValue("audience",request.AudienceType);command.Parameters.AddWithValue("segment",(object?)request.SegmentKey??DBNull.Value);command.Parameters.AddWithValue("template",request.TemplateId);command.Parameters.AddWithValue("channel",request.ChannelType);command.Parameters.AddWithValue("scheduled",(object?)request.ScheduledAt??DBNull.Value);command.Parameters.AddWithValue("user",(object?)UserId??DBNull.Value);await command.ExecuteNonQueryAsync(ct);return id;
    }

    public async Task<Guid> CreateAutomation(AutomationRequest request,CancellationToken ct){if(string.IsNullOrWhiteSpace(request.Name)||request.TemplateId==Guid.Empty||!AllowedChannels.Contains(request.ChannelType)||!AllowedTriggers.Contains(request.TriggerType)||request.OffsetMinutes is < -525600 or > 525600)throw new CommunicationValidationException("Revise nome, evento, template, canal e antecedência.");var id=Guid.NewGuid();const string sql="insert into barber.communication_automations(id,tenant_id,branch_id,name,trigger_type,template_id,channel_type,offset_minutes,conditions_json,status,created_by) values(@id,@tenant,@branch,@name,@trigger,@template,@channel,@offset,@conditions::jsonb,@status,@user)";await using var connection=await Open(ct);await using var command=new NpgsqlCommand(sql,connection);ScopeCommand(command);command.Parameters.AddWithValue("id",id);command.Parameters.AddWithValue("name",request.Name.Trim());command.Parameters.AddWithValue("trigger",request.TriggerType);command.Parameters.AddWithValue("template",request.TemplateId);command.Parameters.AddWithValue("channel",request.ChannelType);command.Parameters.AddWithValue("offset",request.OffsetMinutes);command.Parameters.AddWithValue("conditions",NpgsqlDbType.Jsonb,request.ConditionsJson??"{}");command.Parameters.AddWithValue("status",request.Status);command.Parameters.AddWithValue("user",(object?)UserId??DBNull.Value);await command.ExecuteNonQueryAsync(ct);return id;}

    public async Task<int> ChangeOutbox(Guid id,string action,CancellationToken ct){var condition=action=="retry"?"status in ('Failed','Skipped')":"status='Pending'";var set=action=="retry"?"status='Pending',failure_reason=null,failed_at=null":"status='Cancelled',updated_at=now()";await using var connection=await Open(ct);await using var command=new NpgsqlCommand($"update barber.communication_outbox set {set},updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and {condition}",connection);ScopeCommand(command);command.Parameters.AddWithValue("id",id);return await command.ExecuteNonQueryAsync(ct);}
    public async Task<int> Read(Guid? id,CancellationToken ct){await using var connection=await Open(ct);var sql="update barber.notification_inbox set status='Read',read_at=coalesce(read_at,now()) where tenant_id=@tenant and branch_id=@branch and user_id=@user and status='Unread'"+(id.HasValue?" and id=@id":"");await using var command=new NpgsqlCommand(sql,connection);ScopeCommand(command);command.Parameters.AddWithValue("user",(object?)UserId??DBNull.Value);if(id.HasValue)command.Parameters.AddWithValue("id",id.Value);return await command.ExecuteNonQueryAsync(ct);}
    public async Task ReplacePreferences(PreferenceRequest request,CancellationToken ct){if(request.Items is null||request.Items.Count==0)throw new CommunicationValidationException("Selecione ao menos uma preferência.");await using var connection=await Open(ct);await using var transaction=await connection.BeginTransactionAsync(ct);foreach(var item in request.Items){if(!AllowedChannels.Contains(item.ChannelType)||string.IsNullOrWhiteSpace(item.EventType))throw new CommunicationValidationException("Canal e evento são obrigatórios.");await using var command=new NpgsqlCommand("insert into barber.notification_preferences(id,tenant_id,branch_id,recipient_type,user_id,channel_type,event_type,is_enabled,source,updated_at) values(gen_random_uuid(),@tenant,@branch,'User',@user,@channel,@event,@enabled,@source,now()) on conflict(tenant_id,branch_id,user_id,channel_type,event_type) where user_id is not null do update set is_enabled=excluded.is_enabled,source=excluded.source,updated_at=now()",connection,transaction);ScopeCommand(command);command.Parameters.AddWithValue("user",(object?)UserId??DBNull.Value);command.Parameters.AddWithValue("channel",item.ChannelType);command.Parameters.AddWithValue("event",item.EventType);command.Parameters.AddWithValue("enabled",item.IsEnabled);command.Parameters.AddWithValue("source",request.Source??"SelfService");await command.ExecuteNonQueryAsync(ct);}await transaction.CommitAsync(ct);}
    public async Task<byte[]> Export(DateOnly from,DateOnly to,CancellationToken ct){if(from>to)throw new CommunicationValidationException("A data inicial não pode ser posterior à final.");await using var connection=await Open(ct);await using var command=new NpgsqlCommand("select channel_type,status,scheduled_at,sent_at,failure_reason from barber.communication_outbox where tenant_id=@tenant and branch_id=@branch and created_at::date between @from and @to order by created_at",connection);ScopeCommand(command);command.Parameters.AddWithValue("from",from);command.Parameters.AddWithValue("to",to);var csv=new StringBuilder("canal,status,agendado,enviado,motivo\n");await using var reader=await command.ExecuteReaderAsync(ct);while(await reader.ReadAsync(ct))csv.AppendLine(string.Join(',',Enumerable.Range(0,5).Select(i=>'\"'+(reader.IsDBNull(i)?"":reader.GetValue(i).ToString()?.Replace("\"","\"\""))+'\"')));return Encoding.UTF8.GetBytes(csv.ToString());}
    private static readonly HashSet<string> AllowedChannels=["InApp","Push","Email","Sms","WhatsApp","Webhook"];
    private static readonly HashSet<string> AllowedAudiences=["Segment","ManualClients","Professionals","AllClients"];
    private static readonly HashSet<string> AllowedTriggers=["AppointmentReminder","AppointmentCreated","AppointmentCancelled","NoShow","PaymentConfirmed","PackageExpiring","CouponExpiring","CashbackAvailable","ClientInactive","Birthday","LowStock","ReceivableOverdue"];
    private static readonly HashSet<string> AllowedTokens=["{{client.name}}","{{professional.name}}","{{appointment.date}}","{{appointment.time}}","{{service.name}}","{{branch.name}}","{{payment.amount}}","{{package.name}}","{{coupon.code}}","{{loyalty.points}}"];
    private static void ValidateTemplate(TemplateRequest request){if(string.IsNullOrWhiteSpace(request.Name)||string.IsNullOrWhiteSpace(request.Code)||string.IsNullOrWhiteSpace(request.Body)||!AllowedChannels.Contains(request.ChannelType)||request.ChannelType=="Email"&&string.IsNullOrWhiteSpace(request.Subject)||request.Variables?.Any(x=>!AllowedTokens.Contains(x))==true)throw new CommunicationValidationException("Revise nome, código, canal, assunto, corpo e variáveis permitidas.");}
}
public sealed record TemplateRequest(string Name,string Code,string ChannelType,string? Subject,string Body,List<string>? Variables,string Status="Active");
public sealed record CampaignRequest(string Name,string? Description,string AudienceType,string? SegmentKey,Guid TemplateId,string ChannelType,DateTimeOffset? ScheduledAt);
public sealed record AutomationRequest(string Name,string TriggerType,Guid TemplateId,string ChannelType,int OffsetMinutes,string? ConditionsJson,string Status="Active");
public sealed record PreferenceItem(string ChannelType,string EventType,bool IsEnabled);
public sealed record PreferenceRequest(List<PreferenceItem> Items,string? Source);
public sealed class CommunicationValidationException(string message):Exception(message);
