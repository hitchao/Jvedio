-- 【公共表】

-- 存储刮削的图片
create table common_images(
    ID INTEGER PRIMARY KEY autoincrement,

    Name VARCHAR(500),
    Path VARCHAR(1000),
    Ext VARCHAR(100),
    Size DOUBLE,
    Height INT,
    Width INT,

    Url TEXT,
    ExtraInfo TEXT,
    Source VARCHAR(100),

    unique(Path)
);

insert into common_images
(Name,Path,Ext,Size,Height,Width,Url,ExtraInfo,Source)
values ('test','C:\test.jpg','jpg',2431,720,1080,'http://www.demo.com/123.jpg','','IMDB');

