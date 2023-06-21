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

CREATE TABLE user_IpDate(
[Id] int not null identity(1, 1) primary key,
[IP] nvarchar(15) not null check ([IP] <> N''),
[Date] date not null check ([Date] <> N''),
User_login_id int not null
);
GO

ALTER TABLE user_IpDate
ADD FOREIGN KEY(User_login_id) REFERENCES user_login (Id);
GO

