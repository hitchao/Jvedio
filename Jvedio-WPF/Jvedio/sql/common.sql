-- 【公共表】

-- 存储刮削的图片
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

    unique(Path)
);

insert into common_images
(Name,Path,PathType,Ext,Size,Height,Width,Url,ExtraInfo,Source)
values ('test','C:\test.jpg',0,'jpg',2431,720,1080,'http://www.demo.com/123.jpg','{"BitDepth":"32"}','IMDB');

drop table if exists common_transaltions;
create table common_transaltions(
    TransaltionID INTEGER PRIMARY KEY autoincrement,

    SourceText TEXT,
    SourceLang VARCHAR(100),
    TargetLang VARCHAR(100),
    TargetText TEXT,
    Platfrom VARCHAR(100),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
);

insert into common_transaltions
(Name,Path,PathType,Ext,Size,Height,Width,Url,ExtraInfo,Source)
values ('test','C:\test.jpg',0,'jpg',2431,720,1080,'http://www.demo.com/123.jpg','{"BitDepth":"32"}','IMDB');

