#!/bin/bash
# SQL Server 初始化脚本 - 等待数据库就绪后导入数据
set -e

# 启动 SQL Server
/opt/mssql/bin/sqlservr &

# 等待 SQL Server 启动
echo "正在等待 SQL Server 启动..."
for i in {1..60}; do
    if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &> /dev/null; then
        echo "SQL Server 已就绪！"
        break
    fi
    echo "等待中... ($i/60)"
    sleep 2
done

# 执行初始化脚本
echo "开始执行数据库初始化..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -i /init.sql

echo "数据库初始化完成！"

# 保持容器运行
wait
