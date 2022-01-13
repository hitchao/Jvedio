
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
values ( 55, 'C:\123\test.sqlite', 'test', 51344, 'Video', 'C:\123.png', 55);


-- 演员管理
-- 演员名称唯一，作为连接

BEGIN;

-- 演员基本信息
create table actor_info(
    ID INTEGER PRIMARY KEY autoincrement,
    Name VARCHAR(500),
    Country VARCHAR(500),
    Nation VARCHAR(500),
    BirthPlace VARCHAR(500),
    Birthday VARCHAR(100),
    Constellation  VARCHAR(100),
    BloodType VARCHAR(100),
    Height INT,
    Weight INT,
    Hobby TEXT,

    Cup VARCHAR(1),
    Chest INT,
    Waist INT,
    Hipline INT,

    ExtraInfo TEXT,

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    unique(Name)
)；
CREATE INDEX name_idx ON databases (Name);


-- 演员出演的作品和演员对应关系（多对多）
-- 作品可以是：影视、写真、游戏等
create table actor_works_to_info(
    ID INTEGER PRIMARY KEY autoincrement,
    WorkID INT,
)


-- 同一个人有多个名字
create table actor_alias(
    ID INTEGER PRIMARY KEY autoincrement,
    PrimaryName VARCHAR(500),
    Name VARCHAR(500),
)

-- 演员照片
create table actor_images(
    ID INTEGER PRIMARY KEY autoincrement,
    Url TEXT, 
    Source VARCHAR(10),
    Path TEXT,

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
)

-- 演员照片和演员对应关系
create table actor_info_images(
    ID INTEGER PRIMARY KEY autoincrement,
    Name VARCHAR(500),
    ImageID INT,
)







COMMIT;
