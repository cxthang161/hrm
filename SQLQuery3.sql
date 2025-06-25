
create table Roles(
	Id int identity(1,1) not null primary key,
	Name nvarchar(50) not null
)

create table Agents(
	Id int identity(1,1) not null primary key,
	AgentCode nvarchar(50) not null unique,
	AgentName nvarchar(100) not null,
	Address nvarchar(200),
	Phone nvarchar(20),
)

create table Users (
	Id int identity(1,1) not null primary key,
	UserName nvarchar(100) unique not null,
	Password nvarchar(100) not null,
	RoleId int not null foreign key references Roles(Id),
	AgentId int not null foreign key references Agents(Id),
	CreatedAt Datetime default getDate()
)

create table Configs(
	Id int identity(1,1) not null primary key,
	AgentId int not null foreign key references Agents(Id),
	ProductKey nvarchar(100) not null unique,
	ConfigValue nvarchar(max) not null,
	LogoUrl nvarchar(max) not null,
	BackgroundUrl nvarchar(max) not null,
	NameTemplate nvarchar(200) not null,
	UpdatedBy int not null foreign key references Users(Id),
	UpdatedAt Datetime default getDate()
)

create table RefreshTokens(
	Id int identity(1,1) not null primary key,
	UserId int not null foreign key references Users(Id),
	Token nvarchar(500) not null unique,
	ExpiresAt DateTime not null,
	CreatedAt DateTime default getDate()
)

CREATE TABLE Permissions(
	Id int identity(1,1) not null Primary Key,
	Name varchar(50) not null unique,
	KeyName varchar(50) not null unique,
)

ALTER TABLE Users ADD Permissions NVARCHAR(MAX);
