create table if not exists goals (id uuid primary key, tenant_id uuid not null, branch_id uuid, professional_id uuid, name text not null, metric text not null, start_date date not null, end_date date not null);
create table if not exists goal_targets (id uuid primary key, goal_id uuid references goals(id), target_value numeric(12,2) not null);
create table if not exists goal_progress (id uuid primary key, goal_id uuid references goals(id), current_value numeric(12,2) not null, progress_percent numeric(5,2) not null, achieved boolean default false, updated_at timestamptz default now());
create table if not exists goal_rewards (id uuid primary key, goal_id uuid references goals(id), reward_name text not null, reward_value numeric(12,2));
