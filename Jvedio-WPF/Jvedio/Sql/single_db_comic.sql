

-- 漫画信息表
-- ComicType: [Doujinshi,Manga,Artist CG,Game CG,Western,NonH,ImageSet,Cosplay,Asian,Misc]
-- web_type : 所属网址 => []
-- WebUrl : 对应的网址
drop table if exists metadata_comic;
BEGIN;
create table metadata_comic(
    CID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    Language VARCHAR(100),
    ComicType INT DEFAULT 0,
    Artist TEXT,
    Plot TEXT,
    Outline TEXT,
    PicCount INTEGER DEFAULT 0,
    PicPaths TEXT,

    WebType  VARCHAR(100),
    WebUrl  VARCHAR(2000),
    ExtraInfo TEXT,
    
    unique(DataID,CID)
);
CREATE INDEX metadata_comic_idx_DataID_CID ON metadata_comic (DataID,CID);
COMMIT;