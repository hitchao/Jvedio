
-- 所有字段命名都和映射类一致

-- 启动界面管理
BEGIN;
create table databases (
    ID INTEGER PRIMARY KEY autoincrement,
    Path TEXT DEFAULT '',
    Name VARCHAR(500),
    Size DOUBLE DEFAULT 0,
    Count INT DEFAULT 0,
    Type VARCHAR(20) DEFAULT 'Video',
    ImagePath TEXT DEFAULT '',
    ViewCount INT DEFAULT 0,

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    unique(Type, Name),
    unique(Path)
);
CREATE INDEX name_idx ON databases (Name);
CREATE INDEX type_idx ON databases (Type);
COMMIT;

insert into databases ( Count, Path, Name, Size, Type, ImagePath, ViewCount )
values ( 55, 'C:\123\test.sqlite', 'test', 51344, 'Video', '123.png', 55);

