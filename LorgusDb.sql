CREATE DATABASE lorgus;

USE lorgus;

CREATE TABLE `Budget`
(
	BudgetId INT AUTO_INCREMENT PRIMARY KEY,
	FullName VARCHAR(50) NOT NULL,
    Phone VARCHAR(15),
    Email VARCHAR(50) NOT NULL,
    Message VARCHAR(2000) NOT NULL,
    `Date` DATETIME NOT NULL,
    Answered TINYINT(1) NOT NULL DEFAULT 0
);

CREATE TABLE `Access_Level`(
	AccessLevelId SMALLINT PRIMARY KEY,
    `Description` VARCHAR(10) NOT NULL
);

CREATE TABLE `User`(
	UserId INT AUTO_INCREMENT PRIMARY KEY,
	FullName VARCHAR(50) NOT NULL,
	Email VARCHAR(30) NOT NULL UNIQUE,
	Phone VARCHAR(11) NOT NULL,
	PasswordSalt BINARY(128),
	PasswordHash BINARY(64),
    AccessLevelId SMALLINT NOT NULL, 
    FOREIGN KEY(AccessLevelId) REFERENCES Access_Level(AccessLevelId)
);

CREATE TABLE `Request_Change_Password`(
	RequestChangePasswordId VARCHAR(38) PRIMARY KEY,
	`Date` DATETIME,
	UserId INT NOT NULL, 
    FOREIGN KEY(UserId) REFERENCES `User`(UserId)
);

INSERT INTO `Access_Level`(AccessLevelId, `Description`) VALUES (1, 'Admin');
INSERT INTO `Access_Level`(AccessLevelId, `Description`) VALUES (1, 'Padrão');