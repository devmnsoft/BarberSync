using System.Data.Common;
using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using BarberSync.Infrastructure.Saas;
using BarberSync.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Cryptography;

namespace BarberSync.Tests;

public sealed class PostgresControlPlaneIntegrationTests
{
    private static string? ConnectionString => Environment.GetEnvironmentVariable("BARBERSYNC_TEST_CONNECTION");

    [Fact]
    public async Task Control_plane_read_models_execute_against_canonical_schema()
    {
        if(string.IsNullOrWhiteSpace(ConnectionString))return;
        var factory=new TestConnectionFactory(ConnectionString);
        var tenants=new PostgresPlatformTenantRepository(factory);
        var modules=new PostgresSaasModuleRepository(factory,new LocalEffectiveAccessCache());

        var dashboard=await tenants.DashboardAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),DateOnly.FromDateTime(DateTime.UtcNow),CancellationToken.None);
        var page=await tenants.ListAsync(new(null,null,null,null,null,null,null,null,null,null,1,25),CancellationToken.None);
        var catalog=await modules.ListAsync(CancellationToken.None);
        var knownTenant=page.Items.FirstOrDefault();
        if(knownTenant is not null)
        {
            Assert.NotNull(await tenants.RolesAsync(knownTenant.Id,CancellationToken.None));
            Assert.NotNull(await tenants.BranchesAsync(knownTenant.Id,CancellationToken.None));
            Assert.NotNull(await tenants.UsageAsync(knownTenant.Id,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),DateOnly.FromDateTime(DateTime.UtcNow),CancellationToken.None));
        }

        Assert.True(dashboard.ActiveTenants>=0);
        Assert.Equal(23,catalog.Count);
        Assert.True(page.Total>=page.Items.Count);
    }

    [Fact]
    public async Task Login_entitlement_and_platform_scope_use_persisted_identifiers_and_audit()
    {
        if(string.IsNullOrWhiteSpace(ConnectionString))return;
        var factory=new TestConnectionFactory(ConnectionString);
        var tenantId=Guid.NewGuid();var branchId=Guid.NewGuid();var userId=Guid.NewGuid();var platformId=Guid.NewGuid();
        var tenantEmail=$"conta-{tenantId:N}@example.test";var userEmail=$"user-{userId:N}@example.test";var platformEmail=$"platform-{platformId:N}@example.test";
        var userModel=new AuthUser(userId,tenantId,branchId,userEmail,[],[]);var platformModel=new AuthUser(platformId,Guid.Empty,Guid.Empty,platformEmail,[],[],true,platformId);
        var hasher=new PasswordHasher<AuthUser>();var password=Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))+"aA1!";
        await using(var connection=(NpgsqlConnection)await factory.OpenConnectionAsync())
        await using(var command=new NpgsqlCommand("""
            UPDATE barber.tenants SET deleted_at=now(),is_active=false,document_number_normalized=NULL,institutional_email=NULL WHERE document_number_normalized='04252011000110' AND deleted_at IS NULL;
            INSERT INTO barber.tenants(id,name,slug,document_type,document_number,document_number_normalized,institutional_email,status,is_active) VALUES(@tenant,'Cliente integração',@slug,'CNPJ','04.252.011/0001-10','04252011000110',@tenant_email,'Active',true);
            INSERT INTO barber.branches(id,tenant_id,name,code,status,is_active) VALUES(@branch,@tenant,'Matriz','MATRIZ','Active',true);
            INSERT INTO barber.users(id,tenant_id,branch_id,email,password_hash,full_name,cpf_normalized,status,is_active) VALUES(@user,@tenant,@branch,@user_email,@user_hash,'Usuário integração','52998224725','Active',true);
            INSERT INTO barber.user_roles(user_id,role_id) SELECT @user,id FROM barber.roles WHERE code='Owner' LIMIT 1;
            INSERT INTO barber.tenant_subscriptions(id,tenant_id,plan_id,status,billing_cycle,period_start,period_end,starts_at,ends_at) VALUES(gen_random_uuid(),@tenant,'31000000-0000-4000-8000-000000000001','Active','Monthly',current_date,current_date+365,now(),now()+interval '365 days');
            INSERT INTO barber.platform_users(id,email,full_name,password_hash,status,is_active) VALUES(@platform,@platform_email,'SuperAdmin integração',@platform_hash,'Active',true);
            INSERT INTO barber.platform_user_roles(user_id,role_id) SELECT @platform,id FROM barber.platform_roles WHERE code='SuperAdmin';
            """,connection))
        {
            command.Parameters.AddWithValue("tenant",tenantId);command.Parameters.AddWithValue("branch",branchId);command.Parameters.AddWithValue("user",userId);command.Parameters.AddWithValue("platform",platformId);
            command.Parameters.AddWithValue("slug",$"integration-{tenantId:N}");command.Parameters.AddWithValue("tenant_email",tenantEmail);command.Parameters.AddWithValue("user_email",userEmail);command.Parameters.AddWithValue("platform_email",platformEmail);
            command.Parameters.AddWithValue("user_hash",hasher.HashPassword(userModel,password));command.Parameters.AddWithValue("platform_hash",hasher.HashPassword(platformModel,password));await command.ExecuteNonQueryAsync();
        }

        var options=Options.Create(new JwtOptions{Issuer="tests",Audience="tests",SigningKey=Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)),AccessTokenMinutes=15,RefreshTokenDays=7});
        var tokenService=new JwtTokenService(options);var entitlements=new PostgresModuleEntitlementService(factory,new LocalEffectiveAccessCache());
        var auth=new PostgresAuthService(factory,tokenService,hasher,options,entitlements);
        var cnpj=await auth.LoginAsync(new(null,password,null,"04.252.011/0001-10","529.982.247-25"),"127.0.0.1","login-cnpj",CancellationToken.None);
        var email=await auth.LoginAsync(new(null,password,null,tenantEmail.ToUpperInvariant(),userEmail.ToUpperInvariant()),"127.0.0.1","login-email",CancellationToken.None);
        var platform=await auth.LoginAsync(new(null,password,null,null,platformEmail),"127.0.0.1","login-platform",CancellationToken.None);

        Assert.NotNull(cnpj);Assert.NotNull(email);Assert.NotNull(platform);
        Assert.Contains("modules",ReadJwtPayload(cnpj!.AccessToken),StringComparison.Ordinal);
        var scopeService=new PostgresPlatformScopeService(factory,tokenService,options);
        var scope=await scopeService.StartAsync(platformId,new(tenantId,branchId,"Suporte solicitado pelo cliente"),"127.0.0.1","scope-test",CancellationToken.None);
        Assert.NotNull(scope);
        await scopeService.EndAsync(platformId,scope!.ScopeSessionId,"scope-end-test",CancellationToken.None);
        await using var verify=(NpgsqlConnection)await factory.OpenConnectionAsync();await using var audit=new NpgsqlCommand("SELECT count(*) FROM barber.audit_logs WHERE actor_user_id=@actor AND scope_session_id=@scope AND action IN('PlatformContextStarted','PlatformContextEnded')",verify);audit.Parameters.AddWithValue("actor",platformId);audit.Parameters.AddWithValue("scope",scope.ScopeSessionId);
        Assert.Equal(2L,(long)(await audit.ExecuteScalarAsync())!);
    }

    private static string ReadJwtPayload(string token){var part=token.Split('.')[1].Replace('-','+').Replace('_','/');return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(part.PadRight(part.Length+(4-part.Length%4)%4,'=')));}
    private sealed class TestConnectionFactory(string connectionString):IDbConnectionFactory
    {
        public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken=default){var connection=new NpgsqlConnection(connectionString);await connection.OpenAsync(cancellationToken);return connection;}
    }
}
