-- 缓存表
-- 减少 IO 读取


-- ImageType 缓存的图片类型

drop table if exists cache_image;
BEGIN;
create table if not exists cache_image (
    CacheID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    ImageType TEXT,
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
CREATE INDEX metadata_idx_DBId_DataID ON metadata (DBId,DataID);
CREATE INDEX metadata_idx_ReleaseDate ON metadata (ReleaseDate);
CREATE INDEX metadata_idx_DataType ON metadata (DBId,DataType);
CREATE INDEX metadata_idx_Hash ON metadata (Hash);
CREATE INDEX metadata_idx_ViewDate ON metadata (ViewDate);
CREATE INDEX metadata_idx_FirstScanDate ON metadata (FirstScanDate);
CREATE INDEX metadata_idx_LastScanDate ON metadata (LastScanDate);
COMMIT;