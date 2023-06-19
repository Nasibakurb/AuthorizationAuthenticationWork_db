Use [master]
GO 

DROP DATABASE IF EXISTS [DBus];
GO

CREATE DATABASE DBus;
GO 

USE DBus;
GO 

CREATE TABLE user_login(
[Id] int not null identity(1, 1) primary key,
[Username] nvarchar(30) not null check ([Username] <> N'') unique,
[Password] nvarchar(15) not null check ([Password] <> N''),
);
GO



