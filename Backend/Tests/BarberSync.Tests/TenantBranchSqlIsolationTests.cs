namespace BarberSync.Tests;

/// <summary>
/// Regression guards for the command aggregate. These assertions deliberately inspect
/// the SQL at its execution boundary: a future repository refactor must preserve both
/// tenant and branch predicates before it can pass CI.
/// </summary>
public sealed class TenantBranchSqlIsolationTests
{
    private static readonly string Repository = File.ReadAllText(FindRepositoryFile(
        "Backend", "Infrastructure", "BarberSync.Infrastructure", "Repositories", "PostgresServiceOrderRepository.cs"));

    [Theory]
    [InlineData("WHERE id=@client AND tenant_id=@tenant AND branch_id=@branch")]
    [InlineData("i.tenant_id=@tenant AND i.branch_id=@branch")]
    [InlineData("p.tenant_id=@tenant AND p.branch_id=@branch AND p.service_order_id=@order")]
    [InlineData("service_order_id=@id AND tenant_id=@tenant AND branch_id=@branch")]
    public void Command_reads_and_writes_must_apply_tenant_and_branch_scope(string requiredPredicate)
        => Assert.Contains(requiredPredicate, Repository, StringComparison.Ordinal);

    [Fact]
    public void Shared_services_may_be_tenant_wide_but_products_must_belong_to_active_branch()
    {
        Assert.Contains("(branch_id IS NULL OR branch_id=@branch)", Repository, StringComparison.Ordinal);
        Assert.Contains("id=@product AND tenant_id=@tenant AND branch_id=@branch", Repository, StringComparison.Ordinal);
    }

    [Fact]
    public void Idempotency_replay_must_not_cross_order_or_branch_boundaries()
    {
        Assert.Contains("p.branch_id=@branch AND p.service_order_id=@order AND p.idempotency_key=@key", Repository, StringComparison.Ordinal);
        Assert.Contains("Add(existing,\"branch\",branch);Add(existing,\"order\",orderId)", Repository, StringComparison.Ordinal);
    }

    private static string FindRepositoryFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, .. parts]);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Repository file not found: {Path.Combine(parts)}");
    }
}
