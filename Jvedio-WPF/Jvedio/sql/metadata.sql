-- 元数据管理

-- 【公共表】

-- 存储导入的信息，公有的
-- Path 绝对路径
-- UID 识别出来的标识符
-- DataType: 0-视频 1-漫画 2-图片 3-游戏
-- Rating 刮削的评分
-- Grade 自己的评分
drop table if exists data;
create table if not exists data (
    DataID INTEGER PRIMARY KEY autoincrement,
    Title TEXT,
    Size  DOUBLE DEFAULT 0,
    Path TEXT,
    ReleaseDate VARCHAR(30) DEFAULT '1900-01-01',
    ReleaseYear INT DEFAULT 1900,
    ViewCount INT DEFAULT 0,
    DataType INT DEFAULT 0,
    Rating FLOAT DEFAULT 0.0,
    Favorited INT DEFAULT 0,


    Genre TEXT,
    Tag TEXT,
    Studio TEXT,
    
    Plot TEXT,
    Outline TEXT,

    Grade FLOAT DEFAULT 0.0,
    Label TEXT,



    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
);

-- 影视信息表
drop table if exists data_movie;
create table data_movie(
    DataID INTEGER PRIMARY KEY autoincrement,
    UID VARCHAR(500),
    Director VARCHAR(100),
    Country VARCHAR(50),
);

-- 漫画信息表
-- CommicType: Doujinshi,Manga,Artist CG,Western,Non-H,Image Set,Cosplay,Asian,Misc
drop table if exists data_comic;
create table data_comic(
    DataID INTEGER PRIMARY KEY autoincrement,
    UID VARCHAR(500),
    Author VARCHAR(100),
    CommicType VARCHAR(100),
    Language VARCHAR(100),
    Count INT DEFAULT 0,

);






-- 翻译转换表
-- FieldType: 字段，Title,Plot,Outline,Studio,Genre 等都支持翻译
drop table if exists data_to_translation;
create table data_to_translation(
    ID INTEGER PRIMARY KEY autoincrement,
    FieldType VARCHAR(100),
    TransaltionID INT
);

