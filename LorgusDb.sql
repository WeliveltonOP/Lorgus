CREATE DATABASE lorgus;

USE lorgus;

CREATE TABLE `budget`
(
	BudgetId INT AUTO_INCREMENT PRIMARY KEY,
	FullName VARCHAR(50) NOT NULL,
    Phone VARCHAR(15),
    Email VARCHAR(50) NOT NULL,
    Message VARCHAR(2000) NOT NULL,
    `Date` DATETIME NOT NULL
);