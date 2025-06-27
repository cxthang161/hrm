
create table Roles(
	Id int identity(1,1) not null primary key,
	Name nvarchar(50) not null
)

insert into Roles (Name) values ('admin') 
insert into Roles (Name) values ('user')

create table Agents(
	Id int identity(1,1) not null primary key,
	AgentCode nvarchar(50) not null unique,
	AgentName nvarchar(100) not null,
	Address nvarchar(200),
	Phone nvarchar(20),
)

insert into Agents (AgentCode, AgentName, Address, Phone) values ('AGT001', 'Agent One', 'Hà Nội', '123-456-7890')
insert into Agents (AgentCode, AgentName, Address, Phone) values ('AGT002', 'Agent Two', 'Hải Phòng', '987-654-3210')

create table Users (
	Id int identity(1,1) not null primary key,
	UserName nvarchar(100) unique not null,
	Password nvarchar(100) not null,
	RoleId int not null foreign key references Roles(Id),
	AgentId int not null foreign key references Agents(Id),
	CreatedAt Datetime default getDate()
)

insert into Users (UserName, Password, RoleId, AgentId) values ('admin', 'admin123', 1, 1)
insert into Users (UserName, Password, RoleId, AgentId) values ('user1', 'user123', 2, 1)

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
	Description varchar(50) not null unique,
)

INSERT INTO Permissions (Name, Description) VALUES ('create_config', 'Create config');
INSERT INTO Permissions (Name, Description) VALUES ('edit_config', 'Edit config');
INSERT INTO Permissions (Name, Description) VALUES ('getAll_config', 'Get all config');
INSERT INTO Permissions (Name, Description) VALUES ('get_config', 'Get config');


CREATE TABLE UserPermissions (
    UserId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (UserId, PermissionId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);

INSERT INTO UserPermissions (UserId, PermissionId) VALUES (1, 1);
INSERT INTO UserPermissions (UserId, PermissionId) VALUES (1, 2);
INSERT INTO UserPermissions (UserId, PermissionId) VALUES (1, 3);
INSERT INTO UserPermissions (UserId, PermissionId) VALUES (2, 4);