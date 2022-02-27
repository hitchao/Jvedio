-- 【演员管理说明】
-- 演员名称唯一，默认无同名演员，若存在同名，手动以标记位 NameFlag 区分
drop table if exists actor_info;
drop table if exists actor_name_to_metadatas;
drop table if exists actor_alias;
drop table if exists actor_info_images;


-- Gender:0-未知，1-女 2-男
-- 演员基本信息
BEGIN;
create table actor_info(
    ActorID INTEGER PRIMARY KEY autoincrement,
    ActorName VARCHAR(500),
    NameFlag INT DEFAULT 0,

    Country VARCHAR(500),
    Nation VARCHAR(500),
    BirthPlace VARCHAR(500),
    Birthday VARCHAR(100),
    BloodType VARCHAR(100),
    Height INT,
    Weight INT,
    Gender INT DEFAULT 0,
    Hobby VARCHAR(500),

    Cup VARCHAR(1) DEFAULT 'Z',
    Chest INT,
    Waist INT,
    Hipline INT,

    WebType  VARCHAR(100),
    WebUrl  VARCHAR(2000),

    ImagePath TEXT,
    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    unique(ActorName,NameFlag)
);
CREATE INDEX actor_info_name_idx ON actor_info (ActorName,NameFlag);
COMMIT;

-- 演员出演的作品和演员对应关系（多对多）
-- 作品可以是：影视、写真、游戏等
BEGIN;
create table actor_name_to_metadatas(
    ID INTEGER PRIMARY KEY autoincrement,
    ActorName VARCHAR(500),
    NameFlag INT DEFAULT 0,

    DataID INT,
    unique(ActorName,NameFlag,DataID)
);
CREATE INDEX actor_name_to_metadatas_idx_Name ON actor_name_to_metadatas (ActorName,NameFlag);
CREATE INDEX actor_name_to_metadatas_idx_DataID ON actor_name_to_metadatas (DataID);
COMMIT;

-- 同一个人有多个名字
-- 翻译的名字也属于此列
BEGIN;
create table actor_alias(
    ID INTEGER PRIMARY KEY autoincrement,
    ActorName VARCHAR(500),
    NameFlag INT DEFAULT 0,

    AliasName VARCHAR(500),
    AliasNameFlag INT DEFAULT 0,
    Descriptions TEXT,
    unique(ActorName,NameFlag,AliasName,AliasNameFlag)
);
CREATE INDEX actor_alias_name_idx ON actor_alias (ActorName,NameFlag);
COMMIT;

-- 演员照片和演员对应关系（一对多）
create table actor_info_images(
    ID INTEGER PRIMARY KEY autoincrement,
    ActorName VARCHAR(500),
    NameFlag INT DEFAULT 0,

    ImageID INTEGER,
    ExtraInfo TEXT,
    unique( ActorName , NameFlag , ImageID )
);

-- 测试数据

-- 同名不同人
insert into actor_info
(ActorName,NameFlag, Country , Nation , BirthPlace , Birthday , BloodType , Height , Weight)
values 
('周星驰',0,'中国','汉族','中国香港','19620622','A',174,70),
('周星驰',1,'中国','汉族','中国香港','19620622','A',174,70);

-- 同人不同名
insert into actor_info
(ActorName, Country , Nation , BirthPlace , Birthday , BloodType , Height , Weight)
values 
('星爷','中国','汉族','中国香港','19620622','A',174,70),
('星仔','中国','汉族','中国香港','19620622','A',174,70),
('周星星','中国','汉族','中国香港','19620622','A',174,70);

-- 同一个名字有多个别名，需要用户自行输入
insert into actor_alias
( ActorName , NameFlag , AliasName , AliasNameFlag,Descriptions)
values 
('周星驰',0,'星爷',0,'周星驰后期影视称呼'),
('周星驰',0,'星仔',0,'前期影视称呼'),
('周星驰',0,'周星星',0,'逃学威龙里的称呼'),
('成龙',0,'陈港生',0,'真名'),
('成龙',0,'房仕龙',0,'认祖归宗后按照族谱而取'),
('成龙',0,'Jacky Chan',0,'英文名');

-- 出演的作品
insert into actor_name_to_metadatas
( ActorName , NameFlag , DataID )
values 
('周星驰',0,1),
('周星驰',0,2),
('星爷',0,3),
('星仔',0,4),
('周星星',0,5);

-- 演员图片
insert into actor_info_images
( ActorName , NameFlag , ImageID )
values 
('周星驰',0,11),
('周星驰',0,21),
('周星驰',0,31),
('周星驰',1,41),
('周星驰',1,51),
('周星驰',1,61);

-- 查询演员的信息和照片
select * from 
actor_info
left join actor_info_images
on actor_info.ActorName=actor_info_images.ActorName and actor_info.NameFlag=actor_info_images.NameFlag
where actor_info.ActorName='周星驰' and actor_info.NameFlag=0 limit 1;


-- 查询演员出演的所有作品，包括别名的


select AliasName , AliasNameFlag from actor_alias
where ActorName='周星驰' and NameFlag=0;

select * from actor_name_to_metadatas;


select * from 
actor_name_to_metadatas
where (ActorName,NameFlag) in 
( VALUES ('周星驰',0),('星爷',0));

select * from 
actor_name_to_metadatas
where (ActorName,NameFlag) in 
(select AliasName , AliasNameFlag from actor_alias
where ActorName='周星驰' and NameFlag=0);






