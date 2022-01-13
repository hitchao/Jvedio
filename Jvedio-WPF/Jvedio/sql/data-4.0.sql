
-- 5.0 前的数据库

-- 演员表
create table if not exists actress (
    id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(50),
    birthday VARCHAR(10),
    age INT,
    height INT,
    cup VARCHAR(1),
    chest INT,
    waist INT,
    hipline INT,
    birthplace VARCHAR(50),
    hobby TEXT,
    sourceurl TEXT,
    source VARCHAR(10),
    imageurl TEXT
)