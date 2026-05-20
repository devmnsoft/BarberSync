INSERT INTO loyalty_accounts (id, client_id, points_balance, cashback_balance, tier_level, updated_at)
SELECT gen_random_uuid(), c.id, 120, 35.00, 2, NOW()
FROM clients c
LIMIT 3;

INSERT INTO alerts_notifications (id, audience_type, audience_id, category, channel, payload, status, trigger_at, created_at)
SELECT gen_random_uuid(), 'client', c.id, 'promo_personalizada', 'whatsapp',
       jsonb_build_object('title','Oferta VIP','message','Ganhe cashback extra hoje!'),
       'queued', NOW() + INTERVAL '1 hour', NOW()
FROM clients c
LIMIT 3;
