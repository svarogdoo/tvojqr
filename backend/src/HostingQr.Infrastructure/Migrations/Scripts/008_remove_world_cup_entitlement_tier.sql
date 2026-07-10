alter table user_entitlements
drop constraint if exists ck_user_entitlements_tier;

alter table user_entitlements
add constraint ck_user_entitlements_tier check (tier in ('admin', 'free', 'standard', 'plus')) not valid;
