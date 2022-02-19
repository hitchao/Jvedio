-- 【公共表】

-- 【存储刮削的图片】
-- PathType: 0-绝对路径 1-相对于Jvedio路径 2-相对于影片路径 3-网络绝对路径
drop table if exists common_images;
create table common_images(
    ImageID INTEGER PRIMARY KEY autoincrement,

    Name VARCHAR(500),
    Path VARCHAR(1000),
    PathType INT,
    Ext VARCHAR(100),
    Size DOUBLE,
    Height INT,
    Width INT,

    Url TEXT,
    ExtraInfo TEXT,
    Source VARCHAR(100),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),

    unique(Path)
);

insert into common_images
(Name,Path,PathType,Ext,Size,Height,Width,Url,ExtraInfo,Source)
values ('test','C:\test.jpg',0,'jpg',2431,720,1080,'http://www.demo.com/123.jpg','{"BitDepth":"32"}','IMDB');


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

insert into common_transaltions
(SourceLang,TargetLang,SourceText,TargetText,Platform)
values ('简体中文','English','人是生而自由的','Man is born free','youdao');

-- 【磁力链接】
-- Magnet 40位磁力链接
-- TorrentUrl 种子下载地址
-- UID 视频的 UID
-- Tag 磁力标签
-- Downloads 下载次数
-- ExtraInfo ：{"Seeds":"1","Peers":"0"}
drop table if exists common_magnets;
BEGIN;
create table if not exists common_magnets (
    MagnetID INTEGER PRIMARY KEY autoincrement,
    Magnet VARCHAR(40),
    TorrentUrl VARCHAR(2000),
    UID VARCHAR(500),
    Title TEXT,
    Size DOUBLE DEFAULT 0,
    Releasedate VARCHAR(10) DEFAULT '1900-01-01',
    Tag TEXT,
    Downloads INT DEFAULT 0,
    ExtraInfo TEXT,

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),

    unique(Magnet)
);
CREATE INDEX common_magnets_idx_UID ON common_magnets (UID);
COMMIT;

insert into common_magnets
(Magnet,TorrentUrl,UID,Title,Size,Releasedate,Tag,Downloads,ExtraInfo)
values ('7c5cd6144ae373fec931f20deabcf25eda85cb40','种子下载地址','121213_713','磁力链接1',1034.24,'2014-10-30','高清 中文',15,'{"Seeds":"1","Peers":"0"}');

-- 【db和library等识别码和网址的对应关系】
-- web_type : 所属网址 => [db,library]
drop table if exists common_url_code;
BEGIN;
create table if not exists common_url_code (
    code_id INTEGER PRIMARY KEY autoincrement,
    UID VARCHAR(500),
    code VARCHAR(100),
    web_type VARCHAR(100),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX common_url_code_idx_UID ON common_url_code (web_type,UID);
COMMIT;
insert into common_url_code
(UID,code,web_type)
values ('ABCD-123','1BKY9','db');