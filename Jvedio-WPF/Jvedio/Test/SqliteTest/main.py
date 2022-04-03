
import sqlite3
import os
from datetime import datetime
import time
from concurrent.futures import ThreadPoolExecutor, wait, FIRST_COMPLETED, ALL_COMPLETED


sqlite_path="test.db"

def create_table():
    
    if os.path.exists(sqlite_path) : return
    sql="create table test(id INTEGER PRIMARY KEY autoincrement,text varchar(1000));"
    conn = sqlite3.connect(sqlite_path)
    c = conn.cursor()
    c.execute(sql)
    conn.commit()
    conn.close()

def now():
    return datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3]

def test_sqlite_concurrent(items):
    max=items[0]
    flag=items[1]
 
    print("{} => 开始插入 {} 个数据".format(now(),max))
    start=time.time()
    l=[]
    for i in range(0,max):
        l.append("('{}-demo-text-{}')".format(flag,i))
    values= str.join(",",l)
    #sql="BEGIN TRANSACTION;insert into test(text) values {};END TRANSACTION;".format(values)
    #sql="BEGIN TRANSACTION;insert into test(text) values {};END TRANSACTION;".format(values)
    sql="insert into test(text) values {};".format(values)
    conn = sqlite3.connect(sqlite_path)
    cursor = conn.cursor()
    cursor.executescript(sql)
    conn.commit()
    conn.close()
    end =time.time()
    print("{} => 完成，耗时：{} s".format(now(),end-start))

def readall(n):
    conn = sqlite3.connect(sqlite_path)
    c = conn.cursor()
    c.execute("SELECT * FROM test")
    result = c.fetchall()
    conn.close()
    print(result)

if __name__=="__main__":
    create_table()
    max_count=[(1000000,'a'),(1000000,'b'),(1000000,'c')]

    # 
    with ThreadPoolExecutor(max_workers=4) as t:
        all_task = [t.submit(test_sqlite_concurrent, count) for count in max_count]
        wait(all_task, return_when=ALL_COMPLETED)
 

### 结果 ###
# 1. 加不加 BEGIN TRANSACTION 无所谓，sqlite 都会加上排它锁，导致读的时候提示 databse is locked
#
#
#
#
#