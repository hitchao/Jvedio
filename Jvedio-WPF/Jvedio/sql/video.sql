-- 元数据管理

-- 【公共表】

-- 存储导入的信息，公有的
-- Path 绝对路径
-- DataType: 0-视频 1-漫画 2-图片 3-游戏
-- Rating 刮削的评分，满分 5 分
-- RatingCount 评分的人数
-- Genre 类别，以符号 / 分割
-- Tag 标签，以符号 / 分割
-- Favorited 收藏该资源的人数

-- Grade 自己的评分
-- Label 自己的标签
drop table if exists data;
BEGIN;
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
    RatingCount INT DEFAULT 0,
    Favorited INT DEFAULT 0,
    Genre TEXT,
    Tag TEXT,

    Grade FLOAT DEFAULT 0.0,
    Label TEXT,

    
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX data_idx_ReleaseDate ON data (ReleaseDate);
COMMIT;

-- 影视信息表
-- UID 识别出来的标识符
-- PreviewImages 预览图路径
drop table if exists data_movie;
BEGIN;
create table data_movie(
    DataID INTEGER PRIMARY KEY autoincrement,
    UID VARCHAR(500),
    Director VARCHAR(100),
    Country VARCHAR(50),
    Studio TEXT,
    Publisher TEXT,
    Plot TEXT,
    Outline TEXT,
    Duration INT DEFAULT 0,
    PreviewImages TEXT,

    ExtraInfo TEXT,
    unique(DataID,UID)
);
CREATE INDEX data_movie_idx_UID ON data_movie (UID);
COMMIT;

-- 视频和图片的对应关系
-- 多对多
-- type: 0-缩略图 1-海报图 2-GIF图像
--      仅支持单张图片，避免数据量过大
drop table if exists data_movie_to_image;
BEGIN;
create table data_movie_to_image(
    id INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    ImageID INTEGER,
    ImageType INTEGER
);
CREATE INDEX data_movie_to_image_idx_DataID ON data_movie_to_image (DataID);
COMMIT;
insert into data_movie_to_image(DataID,ImageID,ImageType)
values (1,1,0), (1,2,1), (1,3,2), (1,4,2);



-- 翻译转换表
-- FieldType: 字段，Title,Plot,Outline,Studio,Genre 等都支持翻译
drop table if exists data_to_translation;
BEGIN;
create table data_to_translation(
    ID INTEGER PRIMARY KEY autoincrement,
    FieldType VARCHAR(100),
    TransaltionID INT
);
CREATE INDEX data_to_translation_idx_ID_FieldType ON data_to_translation (ID,FieldType);
COMMIT;

