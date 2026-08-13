using System.Data.Common;
using BarberSync.Application.Abstractions;
using BarberSync.Application.Operations;

namespace BarberSync.Infrastructure.Repositories;

public sealed class PostgresServiceOrderRepository(IDbConnectionFactory connections) : IServiceOrderRepository, IPaymentRepository
{
    private const string OpenOrderSql = """
        INSERT INTO barber.service_orders
            (id,tenant_id,branch_id,appointment_id,client_id,number,notes)
        SELECT @id,@tenant,@branch,@appointment,@client,
               concat('OS-',to_char(now(),'YYYYMMDD-'),substr(@id::text,1,8)),@notes
        WHERE EXISTS (
            SELECT 1 FROM barber.clients
            WHERE id=@client AND tenant_id=@tenant AND deleted_at IS NULL
        )
        """;

    private const string AddServiceItemSql = """
        INSERT INTO barber.service_order_items
            (id,tenant_id,branch_id,service_order_id,item_type,service_id,product_id,professional_id,description,quantity,unit_price,total)
        SELECT @item,@tenant,@branch,@order,@type,@service,@product,@professional,name,@quantity,price,round(@quantity*price,2)
        FROM barber.services
        WHERE id=@service AND tenant_id=@tenant AND deleted_at IS NULL AND status='Active'
        """;

    private const string AddProductItemSql = """
        INSERT INTO barber.service_order_items
            (id,tenant_id,branch_id,service_order_id,item_type,service_id,product_id,professional_id,description,quantity,unit_price,total)
        SELECT @item,@tenant,@branch,@order,@type,@service,@product,@professional,name,@quantity,sale_price,round(@quantity*sale_price,2)
        FROM barber.products
        WHERE id=@product AND tenant_id=@tenant AND deleted_at IS NULL AND status='Active'
          AND (allow_negative_stock OR current_stock >= @quantity)
        """;

    private const string UpdateItemSql = """
        UPDATE barber.service_order_items i
        SET quantity=@quantity,discount=@discount,professional_id=COALESCE(@professional,professional_id),
            total=round(@quantity*unit_price-@discount,2),updated_at=now()
        WHERE i.id=@item AND i.service_order_id=@order AND i.tenant_id=@tenant AND i.deleted_at IS NULL
          AND (i.product_id IS NULL OR EXISTS (
              SELECT 1 FROM barber.products p
              WHERE p.id=i.product_id AND (p.allow_negative_stock OR p.current_stock>=@quantity)
          ))
        """;
    public async Task<IReadOnlyList<ServiceOrderResponse>> ListAsync(Guid tenant,Guid branch,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct); var ids=new List<Guid>(); await using(var cmd=Command(c,"SELECT id FROM barber.service_orders WHERE tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL ORDER BY created_at DESC")){Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))ids.Add(r.GetGuid(0));} var result=new List<ServiceOrderResponse>();foreach(var id in ids)result.Add((await GetAsync(tenant,branch,id,ct))!);return result; }

    public async Task<ServiceOrderResponse?> GetAsync(Guid tenant,Guid branch,Guid id,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct); return await ReadOrder(c,null,tenant,branch,id,ct); }

    public async Task<ServiceOrderResponse> OpenAsync(Guid tenant,Guid branch,OpenServiceOrderRequest request,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(ct);var id=Guid.NewGuid();await using(var cmd=Command(c,OpenOrderSql,tx)){Add(cmd,"id",id);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);Add(cmd,"appointment",request.AppointmentId);Add(cmd,"client",request.ClientId);Add(cmd,"notes",request.Notes);if(await cmd.ExecuteNonQueryAsync(ct)!=1)throw new KeyNotFoundException("Cliente não encontrado.");}await tx.CommitAsync(ct);return (await GetAsync(tenant,branch,id,ct))!; }

    public Task<ServiceOrderResponse> AddServiceAsync(Guid tenant,Guid branch,Guid id,AddServiceItemRequest request,CancellationToken ct) => AddCatalogItem(tenant,branch,id,"Service",request.ServiceId,null,request.ProfessionalId,request.Quantity,ct);
    public Task<ServiceOrderResponse> AddProductAsync(Guid tenant,Guid branch,Guid id,AddProductItemRequest request,CancellationToken ct) => AddCatalogItem(tenant,branch,id,"Product",null,request.ProductId,request.ProfessionalId,request.Quantity,ct);

    private async Task<ServiceOrderResponse> AddCatalogItem(Guid tenant,Guid branch,Guid orderId,string type,Guid? service,Guid? product,Guid? professional,decimal quantity,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(ct);await EnsureOpen(c,tx,tenant,branch,orderId,ct);var itemSql=type=="Service"?AddServiceItemSql:AddProductItemSql;var item=Guid.NewGuid();await using(var cmd=Command(c,itemSql,tx)){Add(cmd,"item",item);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);Add(cmd,"order",orderId);Add(cmd,"type",type);Add(cmd,"service",service);Add(cmd,"product",product);Add(cmd,"professional",professional);Add(cmd,"quantity",quantity);if(await cmd.ExecuteNonQueryAsync(ct)!=1)throw new InvalidOperationException(type=="Product"?"Produto indisponível ou sem estoque suficiente.":"Serviço indisponível.");}await Recalculate(c,tx,tenant,branch,orderId,ct);await tx.CommitAsync(ct);return (await GetAsync(tenant,branch,orderId,ct))!; }

    public async Task<ServiceOrderResponse> UpdateItemAsync(Guid tenant,Guid branch,Guid id,Guid itemId,UpdateOrderItemRequest request,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(ct);await EnsureOpen(c,tx,tenant,branch,id,ct);await using(var cmd=Command(c,UpdateItemSql,tx)){Add(cmd,"quantity",request.Quantity);Add(cmd,"discount",request.Discount);Add(cmd,"professional",request.ProfessionalId);Add(cmd,"item",itemId);Add(cmd,"order",id);Add(cmd,"tenant",tenant);if(await cmd.ExecuteNonQueryAsync(ct)!=1)throw new InvalidOperationException("Item não encontrado ou estoque insuficiente.");}await Recalculate(c,tx,tenant,branch,id,ct);await tx.CommitAsync(ct);return (await GetAsync(tenant,branch,id,ct))!; }

    public async Task<ServiceOrderResponse> RemoveItemAsync(Guid tenant,Guid branch,Guid id,Guid itemId,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(ct);await EnsureOpen(c,tx,tenant,branch,id,ct);await using(var cmd=Command(c,"UPDATE barber.service_order_items SET deleted_at=now(),is_active=false,updated_at=now() WHERE id=@item AND service_order_id=@order AND tenant_id=@tenant AND deleted_at IS NULL",tx)){Add(cmd,"item",itemId);Add(cmd,"order",id);Add(cmd,"tenant",tenant);if(await cmd.ExecuteNonQueryAsync(ct)!=1)throw new KeyNotFoundException("Item não encontrado.");}await Recalculate(c,tx,tenant,branch,id,ct);await tx.CommitAsync(ct);return (await GetAsync(tenant,branch,id,ct))!; }

    public async Task<ServiceOrderResponse> ApplyDiscountAsync(Guid tenant,Guid branch,Guid user,Guid id,ApplyDiscountRequest request,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(ct);await EnsureOpen(c,tx,tenant,branch,id,ct);await using(var cmd=Command(c,"UPDATE barber.service_orders SET discount=@amount,total=greatest(0,subtotal-@amount+surcharge),updated_at=now() WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND @amount<=subtotal",tx)){Add(cmd,"amount",request.Amount);Add(cmd,"id",id);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);if(await cmd.ExecuteNonQueryAsync(ct)!=1)throw new InvalidOperationException("O desconto não pode superar o subtotal.");}await Audit(c,tx,tenant,branch,user,"ServiceOrder.Discount",id,request.Reason,ct);await tx.CommitAsync(ct);return (await GetAsync(tenant,branch,id,ct))!; }

    public async Task<ServiceOrderResponse> ApplyCouponAsync(Guid tenant, Guid branch, Guid user, Guid id, ApplyCouponRequest request, CancellationToken ct)
    {
        await using var c = await connections.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        await EnsureOpen(c, tx, tenant, branch, id, ct);
        const string sql = """
            WITH eligible AS (
              SELECT c.id, o.client_id, LEAST(o.subtotal,round(o.subtotal*c.discount_percent/100,2)) amount
              FROM barber.coupons c JOIN barber.service_orders o ON o.id=@order AND o.tenant_id=c.tenant_id
              WHERE c.tenant_id=@tenant AND (c.branch_id IS NULL OR c.branch_id=@branch)
                AND lower(c.code)=lower(@code) AND c.status='Active' AND c.is_active AND c.deleted_at IS NULL
                AND c.discount_percent>0 AND (c.valid_from IS NULL OR c.valid_from<=now())
                AND (c.valid_until IS NULL OR c.valid_until>=now())
                AND (NOT(c.payload?'clientId') OR c.payload->>'clientId'=o.client_id::text)
                AND (NOT(c.payload?'maxUses') OR (SELECT count(*) FROM barber.coupon_redemptions r WHERE r.coupon_id=c.id)<(c.payload->>'maxUses')::int)
            ), redeemed AS (
              INSERT INTO barber.coupon_redemptions(id,tenant_id,branch_id,coupon_id,service_order_id,client_id,discount_amount,redeemed_by)
              SELECT @redemption,@tenant,@branch,id,@order,client_id,amount,@user FROM eligible RETURNING discount_amount
            )
            UPDATE barber.service_orders SET discount=discount+(SELECT discount_amount FROM redeemed),
              total=greatest(0,total-(SELECT discount_amount FROM redeemed)),updated_at=now()
            WHERE id=@order AND EXISTS(SELECT 1 FROM redeemed)
            """;
        await using var cmd = Command(c, sql, tx);
        Add(cmd,"redemption",Guid.NewGuid()); Add(cmd,"tenant",tenant); Add(cmd,"branch",branch);
        Add(cmd,"order",id); Add(cmd,"code",request.Code.Trim()); Add(cmd,"user",user);
        if (await cmd.ExecuteNonQueryAsync(ct)!=1) throw new InvalidOperationException("Cupom inválido, expirado, inelegível ou esgotado.");
        await Audit(c,tx,tenant,branch,user,"Coupon.Redeem",id,$"Cupom {request.Code.Trim()} aplicado",ct);
        await tx.CommitAsync(ct); return (await GetAsync(tenant,branch,id,ct))!;
    }

    public async Task<ServiceOrderResponse> ApplyCashbackAsync(Guid tenant, Guid branch, Guid user, Guid id, ApplyCashbackRequest request, CancellationToken ct)
    {
        await using var c = await connections.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        await EnsureOpen(c,tx,tenant,branch,id,ct);
        const string sql = """
            WITH account AS (
              SELECT la.id FROM barber.loyalty_accounts la JOIN barber.service_orders o ON o.client_id=la.client_id AND o.tenant_id=la.tenant_id
              WHERE o.id=@order AND la.tenant_id=@tenant AND la.status='Active' AND la.deleted_at IS NULL
                AND la.points>=@amount AND @amount<=o.total
                AND NOT EXISTS(SELECT 1 FROM barber.loyalty_transactions lt WHERE lt.type='Redeem' AND lt.payload->>'serviceOrderId'=@order::text)
              FOR UPDATE OF la
            ), debit AS (
              UPDATE barber.loyalty_accounts la SET points=points-@amount,updated_at=now() FROM account a WHERE la.id=a.id RETURNING la.id
            ), movement AS (
              INSERT INTO barber.loyalty_transactions(id,tenant_id,branch_id,loyalty_account_id,points,type,payload)
              SELECT @movement,@tenant,@branch,id,-@amount,'Redeem',jsonb_build_object('serviceOrderId',@order) FROM debit RETURNING id
            )
            UPDATE barber.service_orders SET discount=discount+@amount,total=total-@amount,updated_at=now()
            WHERE id=@order AND EXISTS(SELECT 1 FROM movement)
            """;
        await using var cmd=Command(c,sql,tx);
        Add(cmd,"movement",Guid.NewGuid()); Add(cmd,"tenant",tenant); Add(cmd,"branch",branch); Add(cmd,"order",id); Add(cmd,"amount",request.Amount);
        if(await cmd.ExecuteNonQueryAsync(ct)!=1) throw new InvalidOperationException("Saldo de cashback insuficiente ou valor acima do total da comanda.");
        await Audit(c,tx,tenant,branch,user,"Loyalty.Redeem",id,$"Cashback resgatado: {request.Amount:0.00}",ct);
        await tx.CommitAsync(ct); return (await GetAsync(tenant,branch,id,ct))!;
    }

    public async Task<PaymentResponse> RegisterAsync(Guid tenant,Guid branch,Guid user,Guid orderId,RegisterPaymentRequest request,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
      await using(var existing=Command(c,"SELECT p.id,p.amount,p.status,o.status,o.total-COALESCE((SELECT sum(paid.amount) FROM barber.payments paid WHERE paid.service_order_id=o.id AND paid.status='Paid'),0) FROM barber.payments p JOIN barber.service_orders o ON o.id=p.service_order_id WHERE p.tenant_id=@tenant AND p.idempotency_key=@key FOR UPDATE OF p",tx)){Add(existing,"tenant",tenant);Add(existing,"key",request.IdempotencyKey);await using var er=await existing.ExecuteReaderAsync(ct);if(await er.ReadAsync(ct)){var replay=new PaymentResponse(er.GetGuid(0),orderId,er.GetString(2),er.GetDecimal(1),0,request.Splits,true,er.GetString(3),Math.Max(0,er.GetDecimal(4)));await er.DisposeAsync();await tx.CommitAsync(ct);return replay;}}
      var order=await ReadOrder(c,tx,tenant,branch,orderId,ct)??throw new KeyNotFoundException("Comanda não encontrada.");if(order.Status is not ("Open" or "PartiallyPaid"))throw new InvalidOperationException("A comanda não está disponível para pagamento.");if(order.Items.Count==0)throw new InvalidOperationException("A comanda não possui itens.");var amount=PaymentRules.ValidateAndTotal(request.Splits,order.Balance);var orderStatus=PaymentRules.OrderStatus(amount,order.Balance);var cash=request.Splits.Where(x=>x.Method.Equals("Cash",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.Amount);Guid? register=null;if(cash>0){await using var rc=Command(c,"SELECT id FROM barber.cash_registers WHERE tenant_id=@tenant AND branch_id=@branch AND status='Open' AND deleted_at IS NULL FOR UPDATE",tx);Add(rc,"tenant",tenant);Add(rc,"branch",branch);register=await rc.ExecuteScalarAsync(ct) as Guid?;if(register is null)throw new InvalidOperationException("Abra o caixa antes de receber em dinheiro.");}
      var payment=Guid.NewGuid();await using(var cmd=Command(c,"INSERT INTO barber.payments(id,tenant_id,branch_id,service_order_id,cash_register_id,idempotency_key,method,amount,paid_at,status,payload) VALUES(@id,@tenant,@branch,@order,@register,@key,@method,@amount,now(),'Paid',jsonb_build_object('note',@note))",tx)){Add(cmd,"id",payment);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);Add(cmd,"order",orderId);Add(cmd,"register",register);Add(cmd,"key",request.IdempotencyKey);Add(cmd,"method",request.Splits.Count>1?"Mixed":request.Splits[0].Method);Add(cmd,"amount",amount);Add(cmd,"note",request.Note);await cmd.ExecuteNonQueryAsync(ct);}foreach(var split in request.Splits){await using var sc=Command(c,"INSERT INTO barber.payment_splits(id,payment_id,method,amount) VALUES(@id,@payment,@method,@amount)",tx);Add(sc,"id",Guid.NewGuid());Add(sc,"payment",payment);Add(sc,"method",split.Method);Add(sc,"amount",split.Amount);await sc.ExecuteNonQueryAsync(ct);}if(cash>0){await using var cc=Command(c,"INSERT INTO barber.cash_transactions(id,tenant_id,branch_id,cash_register_id,payment_id,type,amount,description) VALUES(@id,@tenant,@branch,@register,@payment,'Sale',@amount,'Recebimento de comanda')",tx);Add(cc,"id",Guid.NewGuid());Add(cc,"tenant",tenant);Add(cc,"branch",branch);Add(cc,"register",register);Add(cc,"payment",payment);Add(cc,"amount",cash);await cc.ExecuteNonQueryAsync(ct);}
      if(orderStatus=="Paid")await PostSale(c,tx,tenant,branch,user,orderId,payment,ct);await using(var close=Command(c,"UPDATE barber.service_orders SET status=@status,updated_at=now() WHERE id=@id AND status IN ('Open','PartiallyPaid')",tx)){Add(close,"id",orderId);Add(close,"status",orderStatus);if(await close.ExecuteNonQueryAsync(ct)!=1)throw new InvalidOperationException("A comanda foi alterada por outra operação.");}await Audit(c,tx,tenant,branch,user,"Payment.Register",payment,orderStatus=="Paid"?"Pagamento confirmado":"Pagamento parcial confirmado",ct);await tx.CommitAsync(ct);var received=request.Splits.Where(x=>x.Method.Equals("Cash",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.ReceivedAmount??x.Amount);return new(payment,orderId,"Paid",amount,Math.Max(0,received-cash),request.Splits,false,orderStatus,Math.Max(0,order.Balance-amount)); }

    public async Task<PaymentResponse> RefundAsync(Guid tenant,Guid branch,Guid user,Guid paymentId,RefundPaymentRequest request,CancellationToken ct)
    { await using var c=await connections.OpenConnectionAsync(ct);await using var tx=await c.BeginTransactionAsync(ct);Guid order;decimal amount;await using(var cmd=Command(c,"UPDATE barber.payments SET status='Refunded',updated_at=now() WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND status='Paid' RETURNING service_order_id,amount",tx)){Add(cmd,"id",paymentId);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);await using var r=await cmd.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))throw new InvalidOperationException("Pagamento não encontrado ou já estornado.");order=r.GetGuid(0);amount=r.GetDecimal(1);}
      await using(var cash=Command(c,"INSERT INTO barber.cash_transactions(id,tenant_id,branch_id,cash_register_id,payment_id,type,amount,description) SELECT gen_random_uuid(),tenant_id,branch_id,cash_register_id,id,'Refund',-amount,@reason FROM barber.payments WHERE id=@payment AND cash_register_id IS NOT NULL",tx)){Add(cash,"payment",paymentId);Add(cash,"reason",request.Reason);await cash.ExecuteNonQueryAsync(ct);}
      await using(var stock=Command(c,"WITH returned AS (SELECT i.product_id,sum(i.quantity) qty FROM barber.service_order_items i JOIN barber.service_orders o ON o.id=i.service_order_id AND o.status='Paid' WHERE i.service_order_id=@order AND i.product_id IS NOT NULL AND i.deleted_at IS NULL GROUP BY i.product_id), upd AS (UPDATE barber.products p SET current_stock=p.current_stock+r.qty,updated_at=now() FROM returned r WHERE p.id=r.product_id RETURNING p.id,p.current_stock AS balance_after,r.qty) INSERT INTO barber.stock_movements(id,tenant_id,branch_id,product_id,service_order_id,type,quantity,balance_after,reason,created_by) SELECT gen_random_uuid(),@tenant,@branch,id,@order,'Refund',qty,balance_after,@reason,@user FROM upd",tx)){Add(stock,"order",order);Add(stock,"tenant",tenant);Add(stock,"branch",branch);Add(stock,"reason",request.Reason);Add(stock,"user",user);await stock.ExecuteNonQueryAsync(ct);}
      // Sale effects are posted by the payment that settles the order. A customer can,
      // however, ask to refund an earlier partial payment. Reverse by order instead of
      // payment so stock, commissions and loyalty cannot be left posted in that case.
      await using(var commission=Command(c,"UPDATE barber.commissions c SET status='Reversed' FROM barber.service_order_items i WHERE c.service_order_item_id=i.id AND i.service_order_id=@order AND c.status<>'Reversed'",tx)){Add(commission,"order",order);await commission.ExecuteNonQueryAsync(ct);}
      await using(var loyalty=Command(c,"""
        WITH earned AS (
          SELECT lt.id,lt.loyalty_account_id,lt.points
          FROM barber.loyalty_transactions lt
          JOIN barber.payments p ON p.id=lt.payment_id
          WHERE p.service_order_id=@order AND lt.type='Earn' AND lt.status='Posted'
            AND NOT EXISTS (
              SELECT 1 FROM barber.loyalty_transactions reversal
              WHERE reversal.type='Refund' AND reversal.payload->>'reversedTransactionId'=lt.id::text)
        ), totals AS (
          SELECT loyalty_account_id,sum(points) points FROM earned GROUP BY loyalty_account_id
        ), debited AS (
          UPDATE barber.loyalty_accounts account
          SET points=greatest(0,account.points-totals.points),updated_at=now()
          FROM totals WHERE account.id=totals.loyalty_account_id
          RETURNING account.id
        )
        INSERT INTO barber.loyalty_transactions(id,tenant_id,branch_id,loyalty_account_id,payment_id,points,type,payload)
        SELECT gen_random_uuid(),@tenant,@branch,earned.loyalty_account_id,@payment,-earned.points,'Refund',
          jsonb_build_object('serviceOrderId',@order,'reversedTransactionId',earned.id)
        FROM earned JOIN debited ON debited.id=earned.loyalty_account_id
        """,tx)){Add(loyalty,"tenant",tenant);Add(loyalty,"branch",branch);Add(loyalty,"order",order);Add(loyalty,"payment",paymentId);await loyalty.ExecuteNonQueryAsync(ct);}
      await using(var cmd=Command(c,"UPDATE barber.service_orders SET status=CASE WHEN EXISTS(SELECT 1 FROM barber.payments WHERE service_order_id=@id AND status='Paid') THEN 'PartiallyPaid' ELSE 'Open' END,updated_at=now() WHERE id=@id",tx)){Add(cmd,"id",order);await cmd.ExecuteNonQueryAsync(ct);}await Audit(c,tx,tenant,branch,user,"Payment.Refund",paymentId,request.Reason,ct);await tx.CommitAsync(ct);return new(paymentId,order,"Refunded",amount,0,[]); }

    private static async Task PostSale(DbConnection c,DbTransaction tx,Guid tenant,Guid branch,Guid user,Guid order,Guid payment,CancellationToken ct)
    { await using(var stock=Command(c,"WITH sold AS (SELECT i.product_id,sum(i.quantity) qty FROM barber.service_order_items i WHERE i.service_order_id=@order AND i.product_id IS NOT NULL AND i.deleted_at IS NULL GROUP BY i.product_id), upd AS (UPDATE barber.products p SET current_stock=p.current_stock-s.qty,updated_at=now() FROM sold s WHERE p.id=s.product_id AND (p.allow_negative_stock OR p.current_stock>=s.qty) RETURNING p.id,p.current_stock AS balance_after,s.qty) INSERT INTO barber.stock_movements(id,tenant_id,branch_id,product_id,service_order_id,type,quantity,balance_after,reason,created_by) SELECT gen_random_uuid(),@tenant,@branch,id,@order,'Sale',qty,balance_after,'Venda em comanda',@user FROM upd",tx)){Add(stock,"order",order);Add(stock,"tenant",tenant);Add(stock,"branch",branch);Add(stock,"user",user);await stock.ExecuteNonQueryAsync(ct);}await using(var comm=Command(c,"INSERT INTO barber.commissions(id,tenant_id,branch_id,professional_id,payment_id,service_order_item_id,base_amount,percentage,amount) SELECT gen_random_uuid(),@tenant,@branch,i.professional_id,@payment,i.id,i.total,COALESCE(ps.commission_percent,s.commission_percent,p.default_commission,0),round(i.total*COALESCE(ps.commission_percent,s.commission_percent,p.default_commission,0)/100,2) FROM barber.service_order_items i JOIN barber.professionals p ON p.id=i.professional_id LEFT JOIN barber.services s ON s.id=i.service_id LEFT JOIN barber.professional_services ps ON ps.professional_id=i.professional_id AND ps.service_id=i.service_id WHERE i.service_order_id=@order AND i.deleted_at IS NULL",tx)){Add(comm,"tenant",tenant);Add(comm,"branch",branch);Add(comm,"payment",payment);Add(comm,"order",order);await comm.ExecuteNonQueryAsync(ct);}await using(var loyalty=Command(c,"""
      WITH reward AS (
        SELECT la.id loyalty_account_id,floor(o.total) points
        FROM barber.service_orders o
        JOIN barber.loyalty_accounts la ON la.client_id=o.client_id AND la.tenant_id=o.tenant_id
        WHERE o.id=@order AND la.status='Active' AND la.deleted_at IS NULL
        FOR UPDATE OF la
      ), credited AS (
        UPDATE barber.loyalty_accounts account SET points=account.points+reward.points,updated_at=now()
        FROM reward WHERE account.id=reward.loyalty_account_id
        RETURNING account.id,reward.points
      )
      INSERT INTO barber.loyalty_transactions(id,tenant_id,branch_id,loyalty_account_id,payment_id,points,type,payload)
      SELECT gen_random_uuid(),@tenant,@branch,id,@payment,points,'Earn',jsonb_build_object('serviceOrderId',@order)
      FROM credited
      """,tx)){Add(loyalty,"tenant",tenant);Add(loyalty,"branch",branch);Add(loyalty,"payment",payment);Add(loyalty,"order",order);await loyalty.ExecuteNonQueryAsync(ct);} }

    private static async Task<ServiceOrderResponse?> ReadOrder(DbConnection c,DbTransaction? tx,Guid tenant,Guid branch,Guid id,CancellationToken ct)
    { string number,status;Guid client;Guid? appointment;decimal subtotal,discount,surcharge,total,paid;await using(var cmd=Command(c,"SELECT o.number,o.client_id,o.appointment_id,o.status,o.subtotal,o.discount,o.surcharge,o.total,COALESCE((SELECT sum(amount) FROM barber.payments WHERE service_order_id=o.id AND status='Paid'),0) FROM barber.service_orders o WHERE o.id=@id AND o.tenant_id=@tenant AND o.branch_id=@branch AND o.deleted_at IS NULL",tx)){Add(cmd,"id",id);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);await using var r=await cmd.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))return null;number=r.IsDBNull(0)?id.ToString("N")[..8]:r.GetString(0);client=r.GetGuid(1);appointment=r.IsDBNull(2)?null:r.GetGuid(2);status=r.GetString(3);subtotal=r.GetDecimal(4);discount=r.GetDecimal(5);surcharge=r.GetDecimal(6);total=r.GetDecimal(7);paid=r.GetDecimal(8);}var items=new List<ServiceOrderItemResponse>();await using(var cmd=Command(c,"SELECT id,item_type,service_id,product_id,professional_id,description,quantity,unit_price,discount,total FROM barber.service_order_items WHERE service_order_id=@id AND deleted_at IS NULL ORDER BY created_at",tx)){Add(cmd,"id",id);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))items.Add(new(r.GetGuid(0),r.GetString(1),r.IsDBNull(2)?null:r.GetGuid(2),r.IsDBNull(3)?null:r.GetGuid(3),r.IsDBNull(4)?null:r.GetGuid(4),r.IsDBNull(5)?"Item":r.GetString(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9)));}return new(id,number,client,appointment,status,subtotal,discount,surcharge,total,paid,Math.Max(0,total-paid),items); }
    private static async Task EnsureOpen(DbConnection c,DbTransaction tx,Guid tenant,Guid branch,Guid id,CancellationToken ct){await using var cmd=Command(c,"SELECT status FROM barber.service_orders WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL FOR UPDATE",tx);Add(cmd,"id",id);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);var status=await cmd.ExecuteScalarAsync(ct) as string;if(status is null)throw new KeyNotFoundException("Comanda não encontrada.");if(status!="Open")throw new InvalidOperationException("Itens de uma comanda paga ou encerrada não podem ser alterados.");}
    private static async Task Recalculate(DbConnection c,DbTransaction tx,Guid tenant,Guid branch,Guid id,CancellationToken ct){await using var cmd=Command(c,"UPDATE barber.service_orders SET subtotal=COALESCE((SELECT sum(total) FROM barber.service_order_items WHERE service_order_id=@id AND deleted_at IS NULL),0),total=greatest(0,COALESCE((SELECT sum(total) FROM barber.service_order_items WHERE service_order_id=@id AND deleted_at IS NULL),0)-discount+surcharge),updated_at=now() WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch",tx);Add(cmd,"id",id);Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);await cmd.ExecuteNonQueryAsync(ct);}
    private static async Task Audit(DbConnection c,DbTransaction tx,Guid tenant,Guid branch,Guid user,string operation,Guid entity,string description,CancellationToken ct){await using var cmd=Command(c,"INSERT INTO barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,description,module,action) VALUES(@id,@tenant,@branch,@user,@operation,'ServiceOrder',@entity,@description,'POS',@operation)",tx);Add(cmd,"id",Guid.NewGuid());Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);Add(cmd,"user",user);Add(cmd,"operation",operation);Add(cmd,"entity",entity);Add(cmd,"description",description);await cmd.ExecuteNonQueryAsync(ct);}
    private static DbCommand Command(DbConnection c,string sql,DbTransaction? tx=null){var cmd=c.CreateCommand();cmd.CommandText=sql;cmd.Transaction=tx;return cmd;}
    private static void Add(DbCommand cmd,string name,object? value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value??DBNull.Value;cmd.Parameters.Add(p);}
}
