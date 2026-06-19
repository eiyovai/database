# GitHub 上传与共享指南

> 按照以下步骤将项目上传到 GitHub，与组员共享

## 第一步：创建 GitHub 仓库

1. 打开 [https://github.com](https://github.com) 并登录你的账号
2. 点击右上角 `+` → `New repository`
3. 填写：
   - **Repository name**: `campus-visitor-management`（或你们喜欢的名字）
   - **Description**: `校园开放预约与访客管理系统 - 数据库课程设计`
   - **Visibility**: 选 `Private`（私密，老师可看即可）
   - 不要勾选任何初始化选项（README、.gitignore、license）
4. 点击 `Create repository`

## 第二步：上传项目代码

创建好仓库后，在页面中会显示一串命令。复制并执行以下命令：

```bash
cd "C:\Users\eiyovai\Desktop\DataBase"

git init
git add .
git commit -m "初始提交：校园开放预约与访客管理系统"

git branch -M main
git remote add origin https://github.com/你的用户名/campus-visitor-management.git
git push -u origin main
```

> ⚠️ 把上面的 `你的用户名` 替换成你的 GitHub 用户名

## 第三步：添加组员为协作者

1. 在 GitHub 仓库页面点击 `Settings` → `Collaborators` → `Add people`
2. 输入组员的 GitHub 用户名或邮箱
3. 点击 `Add` 即可

## 第四步：组员克隆项目

组员在自己电脑上运行：

```bash
git clone https://github.com/你的用户名/campus-visitor-management.git
```

然后按照 `README.md` 中的步骤初始化数据库和启动项目。

## 第五步：提交更新

当你修改代码后：

```bash
cd "C:\Users\eiyovai\Desktop\DataBase"
git add .
git commit -m "描述你改了什么东西"
git push
```

组员拉取最新代码：

```bash
git pull
```

## ⚡ 快速上传（含引导）

也可以直接复制以下命令（替换用户名后一行一行执行）：

```bash
cd "C:\Users\eiyovai\Desktop\DataBase"
git init
git add .
git commit -m "初始提交：校园开放预约与访客管理系统"
git branch -M main
git remote add origin https://github.com/你的用户名/campus-visitor-management.git
git push -u origin main
```

## 📦 提交给老师的方式

### 方式一：GitHub 链接（推荐）
将仓库链接发给老师即可：
```
https://github.com/你的用户名/campus-visitor-management
```

### 方式二：打包 ZIP
如果老师要求提交压缩包：

```bash
# 在项目文件夹外，将整个 DataBase 文件夹压缩为 ZIP
# 注意：需要先删除 node_modules 和 bin/obj 减少体积
```

建议提交前先运行以下命令清理：

```bash
# 删除前端依赖（组员自己 npm install）
rm -rf frontend/node_modules
rm -rf frontend/dist

# 删除后端编译文件
rm -rf backend/bin
rm -rf backend/obj
```

然后将整个 `DataBase` 文件夹压缩为 `.zip` 提交。
