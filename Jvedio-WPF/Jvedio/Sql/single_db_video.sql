-- 元数据管理

-- 【公共表】

-- 存储导入的信息，公有的
-- Path 绝对路径
-- Hash 计算的文件哈希值
-- DataType: 0-Video 1-Picture 2-Game 3-Comics
-- Rating 刮削的评分，满分 5 分
-- RatingCount 评分的人数
-- Genre 类别，以符号 / 分割
-- Tag 标签，以符号 / 分割
-- FavoriteCount 收藏该资源的人数


-- Grade 自己的评分
-- Label 自己的标签

-- ViewDate 最近一次播放的日期
-- FirstScanDate 最新一次扫描该资源的日期
-- LastScanDate 最新一次扫描该资源的日期
-- CreateDate 数据创建时间
-- UpdateDate 最近时间
drop table if exists metadata;
BEGIN;
create table if not exists metadata (
    DataID INTEGER PRIMARY KEY autoincrement,
    Title TEXT,
    Size  INTEGER DEFAULT 0,
    Path TEXT,
    Hash VARCHAR(32),
    Country VARCHAR(50),
    ReleaseDate VARCHAR(30) DEFAULT '1900-01-01',
    ReleaseYear INT DEFAULT 1900,
    ViewCount INT DEFAULT 0,
    DataType INT DEFAULT 0,
    Rating FLOAT DEFAULT 0.0,
    RatingCount INT DEFAULT 0,
    FavoriteCount INT DEFAULT 0,
    Genre TEXT,
    Tag TEXT,

    

    Grade FLOAT DEFAULT 0.0,
    Label TEXT,

    ViewDate VARCHAR(30),
    FirstScanDate VARCHAR(30),
    LastScanDate VARCHAR(30),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX metadata_idx_ReleaseDate ON metadata (ReleaseDate);
CREATE INDEX metadata_idx_DataType ON metadata (DataType);
CREATE INDEX metadata_idx_Hash ON metadata (Hash);
CREATE INDEX metadata_idx_ViewDate ON metadata (ViewDate);
CREATE INDEX metadata_idx_FirstScanDate ON metadata (FirstScanDate);
CREATE INDEX metadata_idx_LastScanDate ON metadata (LastScanDate);
COMMIT;


-- 每个作品关联，如分段视频
-- RelevanceType 0-分段视频 1-同一演员出演
drop table if exists metadata_relevance;
BEGIN;
create table metadata_relevance(
    Id INTEGER PRIMARY KEY autoincrement,
    RelevanceType INT DEFAULT 0,
    DataID INTEGER,
    TargetDataID INTEGER
);
CREATE INDEX metadata_relevance_idx_DataID ON metadata_relevance (DataID,RelevanceType);
COMMIT;



-- 影视信息表
-- VID 识别出来的标识符
-- PreviewImages 预览图路径
-- VideoType: 0-Normal 1-Censored 2-UnCensored
-- ImageUrls: {"actress":[],"smallimage":"","bigimage":"","extraimages":[]}
-- web_type : 所属网址 => [db,library,bus]
-- WebUrl : 对应的网址
drop table if exists metadata_video;
BEGIN;
create table metadata_video(
    DataID INTEGER PRIMARY KEY autoincrement,
    VID VARCHAR(500),
    VideoType INT DEFAULT 0,
    Director VARCHAR(100),
    Studio TEXT,
    Publisher TEXT,
    Plot TEXT,
    Outline TEXT,
    Duration INT DEFAULT 0,

    ImageUrls TEXT DEFAULT '',
    
    PreviewImagePaths TEXT,
    ScreenShotPaths TEXT,
    GifImagePath TEXT,
    BigImagePath TEXT,
    SmallImagePath TEXT,

    WebType  VARCHAR(100),
    WebUrl  VARCHAR(2000),

    ExtraInfo TEXT,
    unique(DataID,VID)
);
CREATE INDEX metadata_video_idx_VID ON metadata_video (VID);
CREATE INDEX metadata_video_idx_VideoType ON metadata_video (VideoType);
COMMIT;


insert into metadata( Title, Size, Path, ReleaseDate, ReleaseYear, ViewCount, DataType, Rating, RatingCount, FavoriteCount, Genre, Tag, Grade, Label )
values
('逃学威龙1',1024,'D:\逃学威龙1.mp4','1991-07-18',1991,0,0,4.5,1573,2721,'搞笑/休闲/警察/学校','',0.0,'无厘头/美女'),
('逃学威龙2',1024,'D:\逃学威龙2.mp4','1991-07-18',1991,0,0,4.5,1573,2721,'搞笑/休闲/警察/学校','',0.0,'无厘头/美女'),
('逃学威龙3-龙过鸡年',1024,'D:\逃学威龙3-龙过鸡年.mp4','1991-07-18',1991,0,0,4.5,1573,2721,'搞笑/休闲/警察/学校','',0.0,'无厘头/美女');

insert into metadata_video(DataID,VID,Director,Country,Studio,Plot,Outline,Duration,PreviewImages,ExtraInfo)
values(1,'逃学威龙1','陈嘉上/王晶','中国','永盛电影制作有限公司','','',101,'D:\预览图\逃学威龙','{"外文名":"Fight Back to School","类型":"喜剧","色彩":"彩色"}');



-- 翻译转换表
-- FieldType: 字段，Title,Plot,Outline,Studio,Genre,Actor 等都支持翻译
drop table if exists metadata_to_translation;
BEGIN;
create table metadata_to_translation(
    ID INTEGER PRIMARY KEY autoincrement,
    FieldType VARCHAR(100),
    TransaltionID INT
);
CREATE INDEX metadata_to_translation_idx_ID_FieldType ON metadata_to_translation (ID,FieldType);
COMMIT;

