-- 元数据管理

-- 【公共表】

-- 存储导入的信息，公有的
-- Path 绝对路径
-- Hash 哈希值 = MD5Hash(Size.toString() + 文件的前 64 位字节 + 文件总长度/2 后的 64 位字节 + 文件的后 64 位字节)
--      理论上相同 Size 下最大支持的文件 2^128  
-- DataType: 0-Video 1-Picture 2-Game 3-Comics
-- Rating 刮削的评分，满分 5 分
-- RatingCount 评分的人数
-- Genre 类别
-- FavoriteCount 收藏该资源的人数


-- Grade 自己的评分
-- Label 自己的标签

-- ViewDate 最近一次播放的日期
-- FirstScanDate 最新一次扫描该资源的日期
-- LastScanDate 最新一次扫描该资源的日期
-- CreateDate 数据创建时间
-- UpdateDate 最近时间

-- 索引说明
-- 给一些经常排序的字段添加索引：Grade,LastScanDate,FirstScanDate,ReleaseDate,ViewDate
drop table if exists metadata;
BEGIN;
create table if not exists metadata (
    DataID INTEGER PRIMARY KEY autoincrement,
    DBId INTEGER,
    Title TEXT,
    Size  INTEGER DEFAULT 0,
    Path TEXT,
    Hash VARCHAR(32),
    Country VARCHAR(50),
    ReleaseDate VARCHAR(30),
    ReleaseYear INT DEFAULT 1900,
    ViewCount INT DEFAULT 0,
    DataType INT DEFAULT 0,
    Rating FLOAT DEFAULT 0.0,
    RatingCount INT DEFAULT 0,
    FavoriteCount INT DEFAULT 0,
    Genre TEXT,
    Grade FLOAT DEFAULT 0.0,

    ViewDate VARCHAR(30),
    FirstScanDate VARCHAR(30),
    LastScanDate VARCHAR(30),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX metadata_idx_DataID ON metadata (DataID);
CREATE INDEX metadata_idx_DBId_DataID ON metadata (DBId,DataType);

CREATE INDEX metadata_idx_Hash ON metadata (Hash);
CREATE INDEX metadata_idx_DBId_Hash ON metadata (DBId,Hash);
CREATE INDEX metadata_idx_DBId_DataType_Hash ON metadata (DBId,DataType,Hash);

CREATE INDEX metadata_idx_DBId_DataType_ReleaseDate ON metadata (DBId,DataType,ReleaseDate);
CREATE INDEX metadata_idx_DBId_DataType_FirstScanDate ON metadata (DBId,DataType,FirstScanDate);
CREATE INDEX metadata_idx_DBId_DataType_LastScanDate ON metadata (DBId,DataType,LastScanDate);
CREATE INDEX metadata_idx_DBId_DataType_Grade ON metadata (DBId,DataType,Grade);
CREATE INDEX metadata_idx_DBId_DataType_Size ON metadata (DBId,DataType,Size);
CREATE INDEX metadata_idx_DBId_DataType_ViewDate ON metadata (DBId,DataType,ViewDate);
COMMIT;



-- 影视信息表
-- VID 识别出来的标识符
-- PreviewImages 预览图路径
-- VideoType: 0-Normal 1-Censored 2-UnCensored
-- ImageUrls: {"actress":[],"smallimage":"","bigimage":"","extraimages":[]}
-- Series ：系列，对应旧数据的 Series
-- web_type : 所属网址 => [db,library,bus]
-- WebUrl : 对应的网址
-- SubSection: 分段视频位置
drop table if exists metadata_video;
BEGIN;
create table metadata_video(
    MVID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    VID VARCHAR(500),
    VideoType INT DEFAULT 0,
    Series TEXT,
    Director VARCHAR(100),
    Studio TEXT,
    Publisher TEXT,
    Plot TEXT,
    Outline TEXT,
    Duration INT DEFAULT 0,
    SubSection TEXT,
    ImageUrls TEXT DEFAULT '',
    
    WebType  VARCHAR(100),
    WebUrl  VARCHAR(2000),

    ExtraInfo TEXT,
    unique(DataID,VID)
);
CREATE INDEX metadata_video_idx_DataID_VID ON metadata_video (DataID,VID);
CREATE INDEX metadata_video_idx_VID ON metadata_video (VID);
CREATE INDEX metadata_video_idx_VideoType ON metadata_video (VideoType);
COMMIT;


-- insert into metadata( Title, Size, Path, ReleaseDate, ReleaseYear, ViewCount, DataType, Rating, RatingCount, FavoriteCount, Genre, Tag, Grade, Label )
-- values
-- ('逃学威龙1',1024,'D:\逃学威龙1.mp4','1991-07-18',1991,0,0,4.5,1573,2721,'搞笑/休闲/警察/学校','',0.0,'无厘头/美女'),
-- ('逃学威龙2',1024,'D:\逃学威龙2.mp4','1991-07-18',1991,0,0,4.5,1573,2721,'搞笑/休闲/警察/学校','',0.0,'无厘头/美女'),
-- ('逃学威龙3-龙过鸡年',1024,'D:\逃学威龙3-龙过鸡年.mp4','1991-07-18',1991,0,0,4.5,1573,2721,'搞笑/休闲/警察/学校','',0.0,'无厘头/美女');

-- insert into metadata_video(DataID,VID,Director,Country,Studio,Plot,Outline,Duration,PreviewImages,ExtraInfo)
-- values(1,'逃学威龙1','陈嘉上/王晶','中国','永盛电影制作有限公司','','',101,'D:\预览图\逃学威龙','{"外文名":"Fight Back to School","类型":"喜剧","色彩":"彩色"}');



-- 翻译转换表
-- FieldType: 字段，Title,Plot,Outline,Studio,Genre,Actor 等都支持翻译
drop table if exists metadata_to_translation;
BEGIN;
create table metadata_to_translation(
    id INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    FieldType VARCHAR(100),
    TransaltionID INTEGER,
    unique(DataID,FieldType,TransaltionID)
);
CREATE INDEX metadata_to_translation_idx_DataID_FieldType ON metadata_to_translation (DataID,FieldType);
COMMIT;

-- 标记转换表
drop table if exists metadata_to_tagstamp;
BEGIN;
create table metadata_to_tagstamp(
    id INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    TagID INTEGER,
    unique(DataID,TagID)
);
CREATE INDEX metadata_to_tagstamp_idx_DataID ON metadata_to_tagstamp (DataID);
CREATE INDEX metadata_to_tagstamp_idx_TagID ON metadata_to_tagstamp (TagID);
COMMIT;

-- 标签转换表
drop table if exists metadata_to_label;
BEGIN;
create table metadata_to_label(
    id INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    LabelName VARCHAR(200),
    unique(DataID,LabelName)
);
CREATE INDEX metadata_to_label_idx_DataID ON metadata_to_label (DataID);
CREATE INDEX metadata_to_label_idx_LabelName ON metadata_to_label (LabelName);
COMMIT;

-- 演员出演的作品和演员对应关系（多对多）
-- 作品可以是：影视、写真、游戏等
BEGIN;
create table metadata_to_actor(
    ID INTEGER PRIMARY KEY autoincrement,
    ActorID INTEGER,
    DataID INT,
    unique(ActorID,DataID)
);
CREATE INDEX metadata_to_actor_idx_ActorID ON metadata_to_actor (ActorID,DataID);
CREATE INDEX metadata_to_actor_idx_DataID ON metadata_to_actor (DataID,ActorID);
COMMIT;

