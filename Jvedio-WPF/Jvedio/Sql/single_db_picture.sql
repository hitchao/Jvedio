

-- 图片信息表
-- VID 识别出来的标识符
-- PreviewImages 预览图路径
-- ImageUrls: {"actress":[],"smallimage":"","bigimage":"","extraimages":[]}
-- web_type : 所属网址 => [db,library,bus]
-- WebUrl : 对应的网址
-- SubSection: 分段视频位置
drop table if exists metadata_picture;
BEGIN;
create table metadata_picture(
    PID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    Director VARCHAR(100),
    Studio TEXT,
    Publisher TEXT,
    Plot TEXT,
    Outline TEXT,
    PicCount INTEGER DEFAULT 0,
    PicPaths TEXT,
    VideoPaths TEXT,
    ExtraInfo TEXT,
    
    unique(DataID,PID)
);
CREATE INDEX metadata_picture_idx_DataID_PID ON metadata_picture (DataID,PID);
COMMIT;

-- -- 只有图片会记录详细信息
-- drop table if exists metadata_picture_fileinfo;
-- BEGIN;
-- create table metadata_picture_fileinfo(
--     FID INTEGER PRIMARY KEY autoincrement,
--     PID INTEGER,
--     FileName VARCHAR(3000),
--     Dir VARCHAR(3000),
--     Size INTEGER,
--     Ext VARCHAR(100),
--     FileCreateDate VARCHAR(30),
--     FileUpdateDate VARCHAR(30),
--     Hash  VARCHAR(32),
--     Width INT,
--     Height INT,
--     ExtraInfo TEXT,
--     CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
--     UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
--     unique(DataID,VID)
-- );
-- CREATE INDEX metadata_picture_fileinfo_PID ON metadata_picture_fileinfo (DataID,PID);
-- COMMIT;