-- 无论在哪个平台，都用 app_configs.sqlite 存储配置信息

drop table if exists app_configs;
BEGIN;
create table app_configs (
    ConfigId INTEGER PRIMARY KEY autoincrement,
    ConfigName VARCHAR(100),
    ConfigValue TEXT DEFAULT '',

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    unique(ConfigName)
);
CREATE INDEX app_configs_idx_ConfigName ON app_configs (ConfigName);
COMMIT;