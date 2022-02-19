
-- 漫画信息表
-- CommicType: Doujinshi,Manga,Artist CG,Western,Non-H,Image Set,Cosplay,Asian,Misc
-- Parody: 仿照
-- Mixed： 多人
drop table if exists data_comic;
create table data_comic(
    DataID INTEGER PRIMARY KEY autoincrement,
    Artists VARCHAR(2000),
    CommicType VARCHAR(100),
    Language VARCHAR(100),
    Count INT DEFAULT 0,
    Parody VARCHAR(2000),
    Character VARCHAR(2000),
    Other VARCHAR(2000),
    Mixed VARCHAR(2000)
);
