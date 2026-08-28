using System.Text.Json;
using BarberSync.Application.Abstractions;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Api.Services.Clients360;

public sealed class Clients360Store(IConfiguration configuration, ICurrentUserContext current)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada.");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyDictionary<string, string> Collections = new Dictionary<string, string>
    {
        ["technical-sheets"]="client_technical_sheets", ["anamnesis"]="client_anamnesis_forms",
        ["visual-records"]="client_visual_records", ["consents"]="client_consent_acceptances",
        ["budgets"]="client_budgets", ["treatment-plans"]="client_treatment_plans", ["follow-ups"]="client_follow_ups"
    };

    public async Task<JsonElement> Dashboard(CancellationToken ct) => await Json(@"
select jsonb_build_object('clients',count(distinct c.id),'technicalSheets',count(distinct ts.client_id),
'completedAnamnesis',count(distinct af.client_id) filter(where af.status in ('Completed','Reviewed')),
'criticalRestrictions',count(distinct cr.client_id) filter(where cr.status='Active' and cr.severity='Critical'),
'activePlans',count(distinct tp.id) filter(where tp.status='Active'),'pendingFollowUps',count(distinct fu.id) filter(where fu.status in ('Pending','Overdue')),
'sourceStatus','available') from barber.clients c
left join barber.client_technical_sheets ts on ts.client_id=c.id and ts.tenant_id=c.tenant_id and ts.branch_id=c.branch_id
left join barber.client_anamnesis_forms af on af.client_id=c.id and af.tenant_id=c.tenant_id and af.branch_id=c.branch_id
left join barber.client_restrictions cr on cr.client_id=c.id and cr.tenant_id=c.tenant_id and cr.branch_id=c.branch_id
left join barber.client_treatment_plans tp on tp.client_id=c.id and tp.tenant_id=c.tenant_id and tp.branch_id=c.branch_id
left join barber.client_follow_ups fu on fu.client_id=c.id and fu.tenant_id=c.tenant_id and fu.branch_id=c.branch_id
where c.tenant_id=@tenant and c.branch_id=@branch and c.deleted_at is null", null, ct);

    public async Task<JsonElement> Search(string? query, CancellationToken ct) => await Json(@"
select coalesce(jsonb_agg(x order by x->>'name'),'[]') from (select jsonb_build_object('id',id,'name',name,'email',email,'phone',phone,'status',status) x
from barber.clients where tenant_id=@tenant and branch_id=@branch and deleted_at is null and
(@query='' or name ilike '%'||@query||'%' or coalesce(email,'') ilike '%'||@query||'%' or coalesce(phone,'') ilike '%'||@query||'%') limit 30) q", c => c.Parameters.AddWithValue("query", query?.Trim() ?? ""), ct);

    public Task<JsonElement> Profile(Guid clientId, CancellationToken ct) => Json(@"
select jsonb_build_object('client',jsonb_build_object('id',c.id,'name',c.name,'email',c.email,'phone',c.phone,'status',c.status),
'preferences',coalesce((select jsonb_agg(to_jsonb(p) order by p.created_at desc) from barber.client_preferences p where p.tenant_id=@tenant and p.branch_id=@branch and p.client_id=c.id),'[]'),
'restrictions',coalesce((select jsonb_agg(to_jsonb(r) order by case r.severity when 'Critical' then 1 when 'High' then 2 else 3 end,r.created_at desc) from barber.client_restrictions r where r.tenant_id=@tenant and r.branch_id=@branch and r.client_id=c.id and r.status='Active'),'[]'),
'quality',jsonb_build_object('latestReview',(select to_jsonb(r) from barber.client_reviews r where r.tenant_id=@tenant and r.branch_id=@branch and r.client_id=c.id order by r.created_at desc limit 1),'recoveryStatus',(select q.status from barber.quality_recovery_cases q where q.tenant_id=@tenant and q.branch_id=@branch and q.client_id=c.id order by q.opened_at desc limit 1),'followUps',coalesce((select jsonb_agg(to_jsonb(f) order by f.due_at) from barber.quality_follow_up_queue f where f.tenant_id=@tenant and f.branch_id=@branch and f.client_id=c.id and f.status in('Pending','Overdue')),'[]'),'retentionSegments',coalesce((select jsonb_agg(jsonb_build_object('name',s.name,'score',sc.score,'reason',sc.reason)) from barber.quality_retention_segment_clients sc join barber.quality_retention_segments s on s.id=sc.segment_id where sc.tenant_id=@tenant and sc.branch_id=@branch and sc.client_id=c.id),'[]')))
from barber.clients c where c.id=@client and c.tenant_id=@tenant and c.branch_id=@branch and c.deleted_at is null", c => c.Parameters.AddWithValue("client", clientId), ct);

    public Task<JsonElement> Timeline(Guid clientId, CancellationToken ct) => Json(@"select coalesce(jsonb_agg(to_jsonb(e) order by occurred_at desc),'[]') from (select id,event_type,event_title,event_description,source_type,source_id,coalesce(occurred_at,created_at) occurred_at from barber.client_timeline_events where tenant_id=@tenant and branch_id=@branch and client_id=@client union all select id,'QualityRecovery','Caso de recuperação',summary,'QualityRecovery',id,opened_at from barber.quality_recovery_cases where tenant_id=@tenant and branch_id=@branch and client_id=@client union all select id,'QualityFollowUp',title,description,'QualityFollowUp',id,created_at from barber.quality_follow_up_queue where tenant_id=@tenant and branch_id=@branch and client_id=@client) e", c => c.Parameters.AddWithValue("client", clientId), ct);

    public Task<JsonElement> List(Guid clientId, string collection, CancellationToken ct)
    {
        var table = Collections[collection];
        return Json($"select coalesce(jsonb_agg(to_jsonb(x) order by created_at desc),'[]') from barber.{table} x where tenant_id=@tenant and branch_id=@branch and client_id=@client", c => c.Parameters.AddWithValue("client", clientId), ct);
    }

    public async Task<JsonElement> Create(Guid clientId, string collection, JsonElement body, CancellationToken ct)
    {
        RequireClient(body, clientId); Validate(collection, body, false);
        var id = Guid.NewGuid(); var table = Collections[collection];
        var fields = collection switch
        {
            "technical-sheets" => ("professional_id,sheet_type,title,summary,technical_notes,status,created_by", "@professional,@type,@title,@description,@notes,'Draft',@user"),
            "anamnesis" => ("professional_id,form_type,risk_level,review_notes,status,created_by", "@professional,@type,@risk,@notes,@status,@user"),
            "consents" => ("consent_term_id,accepted_by,acceptance_channel,ip_address,user_agent,status", "@term,@user,@channel,@ip::inet,@agent,'Active'"),
            "budgets" => ("professional_id,title,description,status,valid_until,subtotal,discount_total,total,created_by", "@professional,@title,@description,'Draft',@date::date,@subtotal,@discount,@total,@user"),
            "treatment-plans" => ("professional_id,title,objective,status,start_date,end_date,estimated_total,created_by", "@professional,@title,@description,@status,@date::date,@endDate::date,@total,@user"),
            "follow-ups" => ("professional_id,source_type,source_id,title,description,due_at,status,created_by", "@professional,@type,@source,@title,@description,@date::timestamptz,'Pending',@user"),
            _ => throw new InvalidOperationException("Uploads visuais usam armazenamento protegido e não aceitam JSON direto.")
        };
        await using var connection = await Open(ct); await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var command = new NpgsqlCommand($"insert into barber.{table}(id,tenant_id,branch_id,client_id,{fields.Item1}) values(@id,@tenant,@branch,@client,{fields.Item2}) returning to_jsonb({table})", connection, transaction);
        Scope(command); command.Parameters.AddWithValue("id",id); command.Parameters.AddWithValue("client",clientId); AddPayload(command, body);
        var result = await command.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("A gravação não retornou resultado.");
        await Timeline(connection, transaction, clientId, collection, id, $"{Title(collection)} criado", ct);
        await Audit(connection, transaction, collection, id, "Created", ct); await transaction.CommitAsync(ct);
        return Parse(result);
    }

    public async Task<JsonElement> Transition(Guid clientId, string collection, Guid id, string action, JsonElement body, CancellationToken ct)
    {
        var table = Collections[collection];
        var (status, extra) = (collection, action) switch
        {
            ("visual-records","archive") when Required(body,"reason") is not null => ("Archived",",archived_at=now(),archive_reason=@reason"),
            ("consents","revoke") when Required(body,"reason") is not null => ("Revoked",",revoked_at=now(),revoked_reason=@reason"),
            ("budgets","approve") => ("Approved",",approved_at=now()"),
            ("budgets","reject") when Required(body,"reason") is not null => ("Rejected",",rejected_at=now(),rejection_reason=@reason"),
            ("treatment-plans","complete") => ("Completed",",completed_at=now()"),
            ("treatment-plans","cancel") when Required(body,"reason") is not null => ("Cancelled",",cancelled_at=now(),cancel_reason=@reason"),
            ("follow-ups","complete") => ("Done",",completed_at=now(),completed_by=@user"),
            _ => throw new Clients360ValidationException("Transição inválida ou motivo obrigatório ausente.")
        };
        await using var connection=await Open(ct); await using var transaction=await connection.BeginTransactionAsync(ct);
        await using var command=new NpgsqlCommand($"update barber.{table} set status=@status,updated_at=now(){extra} where id=@id and client_id=@client and tenant_id=@tenant and branch_id=@branch returning to_jsonb({table})",connection,transaction);
        Scope(command); command.Parameters.AddWithValue("id",id); command.Parameters.AddWithValue("client",clientId); command.Parameters.AddWithValue("status",status); command.Parameters.AddWithValue("reason",Text(body,"reason") ?? "");
        var result=await command.ExecuteScalarAsync(ct) ?? throw new KeyNotFoundException("Registro não encontrado nesta unidade.");
        await Timeline(connection,transaction,clientId,collection,id,$"{Title(collection)}: {status}",ct); await Audit(connection,transaction,collection,id,action,ct); await transaction.CommitAsync(ct); return Parse(result);
    }

    public Task<JsonElement> Options(CancellationToken ct) => Json(@"select jsonb_build_object(
'professionals',coalesce((select jsonb_agg(jsonb_build_object('value',id,'label',name) order by name) from barber.professionals where tenant_id=@tenant and branch_id=@branch and deleted_at is null),'[]'),
'services',coalesce((select jsonb_agg(jsonb_build_object('value',id,'label',name,'price',price) order by name) from barber.services where tenant_id=@tenant and branch_id=@branch and deleted_at is null),'[]'),
'products',coalesce((select jsonb_agg(jsonb_build_object('value',id,'label',name,'price',sale_price) order by name) from barber.products where tenant_id=@tenant and branch_id=@branch and deleted_at is null),'[]'),
'terms',coalesce((select jsonb_agg(jsonb_build_object('value',id,'label',title||' · v'||version) order by title) from barber.client_consent_terms where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null),'[]'))",null,ct);

    private async Task<JsonElement> Json(string sql, Action<NpgsqlCommand>? configure, CancellationToken ct) { await using var c=await Open(ct); await using var command=new NpgsqlCommand(sql,c); Scope(command); configure?.Invoke(command); var value=await command.ExecuteScalarAsync(ct); if(value is null or DBNull) throw new KeyNotFoundException("Cliente não encontrado nesta unidade."); return Parse(value); }
    private async Task<NpgsqlConnection> Open(CancellationToken ct) { var c=new NpgsqlConnection(_connectionString); await c.OpenAsync(ct); return c; }
    private void Scope(NpgsqlCommand c){c.Parameters.AddWithValue("tenant",current.TenantId);c.Parameters.AddWithValue("branch",current.BranchId);c.Parameters.AddWithValue("user",current.UserId);}
    private static JsonElement Parse(object value)=>JsonDocument.Parse(value.ToString()??"null").RootElement.Clone();
    private static string? Text(JsonElement b,string n)=>b.TryGetProperty(n,out var v)&&v.ValueKind!=JsonValueKind.Null?v.ToString():null;
    private static string? Required(JsonElement b,string n)=>string.IsNullOrWhiteSpace(Text(b,n))?throw new Clients360ValidationException($"{n} é obrigatório."):Text(b,n);
    private static void RequireClient(JsonElement b,Guid id){if(b.TryGetProperty("clientId",out var supplied)&&Guid.TryParse(supplied.ToString(),out var parsed)&&parsed!=id)throw new Clients360ValidationException("O cliente do payload não corresponde à rota.");}
    private static void Validate(string collection,JsonElement b,bool update){if(collection is "technical-sheets" or "anamnesis" && string.IsNullOrWhiteSpace(Text(b,"type")))throw new Clients360ValidationException("type é obrigatório.");if(collection is not "consents" && collection is not "anamnesis" && string.IsNullOrWhiteSpace(Text(b,"title")))throw new Clients360ValidationException("title é obrigatório.");if(collection=="budgets"&&decimal.TryParse(Text(b,"total"),out var total)&&decimal.TryParse(Text(b,"subtotal"),out var subtotal)&&decimal.TryParse(Text(b,"discount"),out var discount)&&total!=subtotal-discount)throw new Clients360ValidationException("O total deve corresponder ao subtotal menos o desconto.");}
    private static Guid? GuidValue(JsonElement b,string n)=>Guid.TryParse(Text(b,n),out var value)?value:null;
    private static decimal Decimal(JsonElement b,string n)=>decimal.TryParse(Text(b,n),out var value)?value:0;
    private void AddPayload(NpgsqlCommand c,JsonElement b){c.Parameters.AddWithValue("professional",(object?)GuidValue(b,"professionalId")??DBNull.Value);c.Parameters.AddWithValue("term",(object?)GuidValue(b,"termId")??DBNull.Value);c.Parameters.AddWithValue("source",(object?)GuidValue(b,"sourceId")??DBNull.Value);c.Parameters.AddWithValue("type",Text(b,"type")??"Manual");c.Parameters.AddWithValue("title",Text(b,"title")??"");c.Parameters.AddWithValue("description",Text(b,"description")??"");c.Parameters.AddWithValue("notes",Text(b,"notes")??"");c.Parameters.AddWithValue("risk",Text(b,"risk")??"Low");c.Parameters.AddWithValue("status",Text(b,"status")??"Draft");c.Parameters.AddWithValue("channel",Text(b,"channel")??"Admin");c.Parameters.AddWithValue("ip",Text(b,"ipAddress")??"127.0.0.1");c.Parameters.AddWithValue("agent",Text(b,"userAgent")??"");c.Parameters.AddWithValue("date",(object?)Text(b,"date")??DBNull.Value);c.Parameters.AddWithValue("endDate",(object?)Text(b,"endDate")??DBNull.Value);c.Parameters.AddWithValue("subtotal",Decimal(b,"subtotal"));c.Parameters.AddWithValue("discount",Decimal(b,"discount"));c.Parameters.AddWithValue("total",Decimal(b,"total"));}
    private async Task Timeline(NpgsqlConnection c,NpgsqlTransaction t,Guid client,string type,Guid source,string title,CancellationToken ct){await using var command=new NpgsqlCommand("insert into barber.client_timeline_events(tenant_id,branch_id,client_id,event_type,event_title,source_type,source_id) values(@tenant,@branch,@client,@type,@title,@type,@source)",c,t);Scope(command);command.Parameters.AddWithValue("client",client);command.Parameters.AddWithValue("type",type);command.Parameters.AddWithValue("title",title);command.Parameters.AddWithValue("source",source);await command.ExecuteNonQueryAsync(ct);}
    private async Task Audit(NpgsqlConnection c,NpgsqlTransaction t,string entity,Guid id,string action,CancellationToken ct){await using var command=new NpgsqlCommand("insert into barber.governance_audit_events(tenant_id,branch_id,user_id,module_key,action,entity_type,entity_id,trace_id) values(@tenant,@branch,@user,'Clients360',@action,@entity,@id,@trace)",c,t);Scope(command);command.Parameters.AddWithValue("action",action);command.Parameters.AddWithValue("entity",entity);command.Parameters.AddWithValue("id",id);command.Parameters.AddWithValue("trace",System.Diagnostics.Activity.Current?.TraceId.ToString()??"");await command.ExecuteNonQueryAsync(ct);}
    private static string Title(string collection)=>collection switch{"technical-sheets"=>"Ficha técnica","anamnesis"=>"Anamnese","consents"=>"Consentimento","budgets"=>"Orçamento","treatment-plans"=>"Plano de tratamento","follow-ups"=>"Follow-up",_=>"Registro visual"};
}

public sealed class Clients360ValidationException(string message) : Exception(message);
