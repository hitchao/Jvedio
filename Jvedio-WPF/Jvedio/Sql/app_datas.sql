-- 单文件存储全部信息

-- 【公共表】
-- 文件名：app_config.sqlite
-- app_xxx 存储 application 级别的信息
-- common_xxx 存储公共信息

-- 所有字段命名都和映射类一致
-- 启动界面管理
-- DataType: 0-Video 1-Picture 2-Game 3-Comics
-- Hide 是否隐藏 0-不隐藏，1 隐藏
-- ScanPath 绑定的需要扫描的路径
drop table if exists app_databases;
BEGIN;
create table app_databases (
    DBId INTEGER PRIMARY KEY autoincrement,
    Name VARCHAR(500),
    Count INTEGER DEFAULT 0,
    DataType INT DEFAULT 0,
    ImagePath TEXT DEFAULT '',
    ViewCount INT DEFAULT 0,
    Hide INT DEFAULT 0,
    ScanPath TEXT,

    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX name_idx ON app_databases (Name);
CREATE INDEX type_idx ON app_databases (DataType);
COMMIT;

-- insert into app_databases ( Count, Name, ImagePath, ViewCount )
-- values ( 55,'test', '123.png', 55);



-- 【翻译表】
-- Platform 翻译平台：[baidu,youdao,google]
drop table if exists common_transaltions;
create table common_transaltions(
    TransaltionID INTEGER PRIMARY KEY autoincrement,

    SourceLang VARCHAR(100),
    TargetLang VARCHAR(100),
    SourceText TEXT,
    TargetText TEXT,
    Platform VARCHAR(100),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);

-- insert into common_transaltions
-- (SourceLang,TargetLang,SourceText,TargetText,Platform)
-- values ('简体中文','English','人是生而自由的','Man is born free','youdao');

-- 【磁力链接】
-- Magnet 40位磁力链接
-- TorrentUrl 种子下载地址
-- VID 视频的 VID
-- Tag 磁力标签
-- DownloadNumber 下载次数
-- ExtraInfo ：{"Seeds":"1","Peers":"0"}
drop table if exists common_magnets;
BEGIN;
create table common_magnets (
    MagnetID INTEGER PRIMARY KEY autoincrement,
    MagnetLink VARCHAR(40),
    TorrentUrl VARCHAR(2000),
    DataID INTEGER,
    VID INTEGER,
    Title TEXT,
    Size INTEGER DEFAULT 0,
    Releasedate VARCHAR(10) DEFAULT '1900-01-01',
    Tag TEXT,

    DownloadNumber INT DEFAULT 0,
    ExtraInfo TEXT,

    CreateDate VARCHAR(30) NOT NULL DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) NOT NULL DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),

    unique(MagnetLink)
);
CREATE INDEX common_magnets_idx_DataID ON common_magnets (DataID);
CREATE INDEX common_magnets_idx_VID ON common_magnets (VID);
COMMIT;

-- insert into common_magnets
-- (Magnet,TorrentUrl,DataID,Title,Size,Releasedate,Tag,DownloadNumber,ExtraInfo)
-- values ('7c5cd6144ae373fec931f20deabcf25eda85cb40','种子下载地址',5,'磁力链接1',1034.24,'2014-10-30','高清 中文',15,'{"Seeds":"1","Peers":"0"}');

-- 【db和library等识别码和网址的对应关系】
-- web_type : 所属网址 => [db,library,bus]
-- CodeType ：演员对应或影片对应 => [actor,video]
drop table if exists common_url_code;
BEGIN;
create table common_url_code (
    CodeId INTEGER PRIMARY KEY autoincrement,
    LocalValue VARCHAR(500),
    ValueType  VARCHAR(20) DEFAULT 'video',
    RemoteValue VARCHAR(100),
    WebType VARCHAR(100),
    
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    unique(ValueType,WebType,LocalValue,RemoteValue)
);
CREATE INDEX common_url_code_idx_ValueType_WebType_LocalValue ON common_url_code (ValueType,WebType,LocalValue);
COMMIT;


-- Beauty 颜值打分
-- Gender 性别
-- Race 人种
-- Mask 口罩/面具 0-否 1-是
-- Glasses 是否戴眼镜
drop table if exists common_ai_face;
BEGIN;
create table common_ai_face (
    AIId INTEGER PRIMARY KEY autoincrement,
    Age INT DEFAULT 0,
    Beauty FLOAT DEFAULT 0,
    Expression VARCHAR(100),
    FaceShape VARCHAR(100),
    Gender INT DEFAULT 0,
    Glasses INT DEFAULT 0,
    Race VARCHAR(100),
    Emotion VARCHAR(100),
    Mask INT DEFAULT 0,
    Platform VARCHAR(100),
    
    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
COMMIT;




-- SearchField: [video,actor,director,series,tag,label,...]
drop table if exists common_search_history;
BEGIN;
create table common_search_history (
    id INTEGER PRIMARY KEY autoincrement,
    SearchValue TEXT,
    SearchField VARCHAR(200),

    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX common_search_history_idx_SearchField ON common_search_history (SearchField);
COMMIT;

-- 【标记】
drop table if exists common_tagstamp;
BEGIN;
create table common_tagstamp (
    TagID INTEGER PRIMARY KEY autoincrement,
    Foreground VARCHAR(100),
    Background VARCHAR(100),
    TagName VARCHAR(200),

    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
insert into common_tagstamp(Background,Foreground,TagName) values('154,88,183,255','255,255,255,255','HD'),('154,205,50,255','255,255,255,255','Translated');
COMMIT;




-- 【存储刮削的图片】
-- PathType: 0-绝对路径 1-相对于Jvedio路径 2-相对于影片路径 3-网络绝对路径
-- drop table if exists common_images;
-- create table common_images(
--     ImageID INTEGER PRIMARY KEY autoincrement,

--     Name VARCHAR(500),
--     Path VARCHAR(1000),
--     PathType INT DEFAULT 0,
--     Ext VARCHAR(100),
--     Size INTEGER,
--     Height INT,
--     Width INT,

--     Url TEXT,
--     ExtraInfo TEXT,
--     Source VARCHAR(100),

--     CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
--     UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),

--     unique(PathType,Path)
-- );

-- insert into common_images
-- (Name,Path,PathType,Ext,Size,Height,Width,Url,ExtraInfo,Source)
-- values ('test','C:\test.jpg',0,'jpg',2431,720,1080,'http://www.demo.com/123.jpg','{"BitDepth":"32"}','IMDB');
