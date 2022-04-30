
-- 【演员】
-- Gender:0-未知，1-女 2-男
-- 演员基本信息
drop table if exists actor_info;
BEGIN;
create table actor_info(
    ActorID INTEGER PRIMARY KEY autoincrement,
    ActorName VARCHAR(500),

    Country VARCHAR(500),
    Nation VARCHAR(500),
    BirthPlace VARCHAR(500),
    Birthday VARCHAR(100),
    Age INT,
    BloodType VARCHAR(100),
    Height INT,
    Weight INT,
    Gender INT DEFAULT 0,
    Hobby VARCHAR(500),

    Cup VARCHAR(1) DEFAULT 'Z',
    Chest INT,
    Waist INT,
    Hipline INT,

    WebType  VARCHAR(100),
    WebUrl  VARCHAR(2000),
    Grade FLOAT DEFAULT 0.0,

    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX actor_info_idx_ActorName ON actor_info (ActorName);
COMMIT;

-- 【演员关系表】
-- 翻译的名字也属于此列
-- Relation: [0-sameperson，1-different]
drop table if exists actor_relation;
BEGIN;
create table actor_relation(
    id INTEGER PRIMARY KEY autoincrement,
    ActorID INTEGER,
    ActorIDAnother INTEGER,
    ActorRelation VARCHAR(500),
    unique(ActorID,ActorIDAnother,ActorRelation)
);
CREATE INDEX actor_relation_idx_ActorID ON actor_relation (ActorID);
COMMIT;


