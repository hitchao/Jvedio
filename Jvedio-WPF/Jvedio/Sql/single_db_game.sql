

-- 漫画信息表
-- 2dfan, steam
-- RunPath: [可执行程序所在位置]
-- Branch : 品牌
-- Play : 剧本
-- ExtraName: 又名
drop table if exists metadata_game;
BEGIN;
create table metadata_game(
    GID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    Branch VARCHAR(100),
    OriginalPainting VARCHAR(200),
    VoiceActors VARCHAR(200),
    Play VARCHAR(200),
    Music VARCHAR(200),
    Singers VARCHAR(200),
    Plot TEXT,
    Outline TEXT,
    ExtraName TEXT,
    Studio TEXT,
    Publisher TEXT,


    WebType  VARCHAR(100),
    WebUrl  VARCHAR(2000),
    ExtraInfo TEXT,
    
    unique(DataID,GID)
);
CREATE INDEX metadata_game_idx_DataID_GID ON metadata_game (DataID,GID);
COMMIT;