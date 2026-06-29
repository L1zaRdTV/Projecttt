IF DB_ID(N'BanquetRoomsDb') IS NULL
    CREATE DATABASE [BanquetRoomsDb];
GO
USE [BanquetRoomsDb];
GO

IF OBJECT_ID(N'dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID(N'dbo.Rooms', 'U') IS NOT NULL DROP TABLE dbo.Rooms;
IF OBJECT_ID(N'dbo.Clients', 'U') IS NOT NULL DROP TABLE dbo.Clients;
IF OBJECT_ID(N'dbo.Managers', 'U') IS NOT NULL DROP TABLE dbo.Managers;
IF OBJECT_ID(N'dbo.PaymentStatuses', 'U') IS NOT NULL DROP TABLE dbo.PaymentStatuses;
IF OBJECT_ID(N'dbo.Styles', 'U') IS NOT NULL DROP TABLE dbo.Styles;
IF OBJECT_ID(N'dbo.CustomerTypes', 'U') IS NOT NULL DROP TABLE dbo.CustomerTypes;
GO

CREATE TABLE dbo.CustomerTypes (
    CustomerTypeId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerTypes PRIMARY KEY,
    Name nvarchar(50) NOT NULL CONSTRAINT UQ_CustomerTypes_Name UNIQUE
);

CREATE TABLE dbo.Styles (
    StyleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Styles PRIMARY KEY,
    Name nvarchar(80) NOT NULL CONSTRAINT UQ_Styles_Name UNIQUE
);

CREATE TABLE dbo.PaymentStatuses (
    PaymentStatusId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentStatuses PRIMARY KEY,
    Name nvarchar(80) NOT NULL CONSTRAINT UQ_PaymentStatuses_Name UNIQUE
);

CREATE TABLE dbo.Clients (
    ClientId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clients PRIMARY KEY,
    FullName nvarchar(120) NOT NULL,
    Phone nvarchar(20) NOT NULL,
    Email nvarchar(120) NOT NULL,
    CustomerTypeId int NOT NULL,
    CONSTRAINT FK_Clients_CustomerTypes FOREIGN KEY (CustomerTypeId) REFERENCES dbo.CustomerTypes(CustomerTypeId)
);

CREATE TABLE dbo.Managers (
    ManagerId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Managers PRIMARY KEY,
    FullName nvarchar(120) NOT NULL,
    Phone nvarchar(20) NOT NULL,
    Email nvarchar(120) NOT NULL
);

CREATE TABLE dbo.Rooms (
    RoomId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rooms PRIMARY KEY,
    Name nvarchar(80) NOT NULL CONSTRAINT UQ_Rooms_Name UNIQUE,
    StyleId int NOT NULL,
    TablesCount int NOT NULL CONSTRAINT CK_Rooms_TablesCount CHECK (TablesCount > 0),
    Description nvarchar(1000) NOT NULL,
    PhotoPath nvarchar(255) NOT NULL,
    RentPricePerHour decimal(10,2) NOT NULL CONSTRAINT CK_Rooms_RentPricePerHour CHECK (RentPricePerHour > 0),
    CONSTRAINT FK_Rooms_Styles FOREIGN KEY (StyleId) REFERENCES dbo.Styles(StyleId)
);

CREATE TABLE dbo.Bookings (
    BookingId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
    ClientId int NOT NULL,
    RoomId int NOT NULL,
    StartsAt datetime NOT NULL,
    EndsAt datetime NOT NULL,
    PaymentStatusId int NOT NULL,
    PrepaymentAmount decimal(10,2) NOT NULL CONSTRAINT DF_Bookings_PrepaymentAmount DEFAULT 0,
    PrepaymentDate date NULL,
    CONSTRAINT FK_Bookings_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId),
    CONSTRAINT FK_Bookings_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT FK_Bookings_PaymentStatuses FOREIGN KEY (PaymentStatusId) REFERENCES dbo.PaymentStatuses(PaymentStatusId),
    CONSTRAINT CK_Bookings_Period CHECK (EndsAt > StartsAt)
);
GO

INSERT INTO dbo.CustomerTypes (Name) VALUES (N'Физическое лицо'), (N'ООО');
INSERT INTO dbo.Styles (Name) VALUES (N'Японский стиль'), (N'Европейский стиль'), (N'Мексиканский стиль');
INSERT INTO dbo.PaymentStatuses (Name) VALUES (N'Полная предоплата'), (N'Частичная предоплата'), (N'Не оплачено');

INSERT INTO dbo.Clients (FullName, Phone, CustomerTypeId, Email) VALUES
(N'Иванов Максим Иванович', N'8911-132-15-15', 1, N'ivan@ya.ru'),
(N'Гулихина Ольга Петровна', N'8923-526-86-97', 1, N'petrofon@mail.ru'),
(N'Васильев Иван Олегович', N'523-12-23', 2, N'romashka@rambler.ru'),
(N'Кирилова Марина Артемовна', N'8923-987-65-32', 1, N'marina@gmail.com');

INSERT INTO dbo.Managers (FullName, Phone, Email) VALUES
(N'Смирнов Игорь Андреевич', N'8921-963-32-65', N'smirnov@ya.ru'),
(N'Петрова Олеся Юрьевна', N'8911-125-45-65', N'olesya@mail.ru');

INSERT INTO dbo.Rooms (Name, StyleId, TablesCount, Description, PhotoPath, RentPricePerHour) VALUES
(N'Красный зал', 1, 100, N'Уютный зал в японском стиле с лаконичным интерьером и спокойной атмосферой, идеально подходящий для семейных праздников и деловых встреч.', N'', 500.00),
(N'Зелёный зал', 2, 250, N'Просторный зал в классическом европейском стиле, отлично подходящий для свадеб, корпоративных мероприятий и крупных торжеств.', N'', 1000.00),
(N'Жёлтый зал', 3, 160, N'Яркий зал в мексиканском стиле с тёплой атмосферой и оригинальным оформлением, идеально подходящий для весёлых праздников и тематических мероприятий.', N'', 750.00);

INSERT INTO dbo.Bookings (ClientId, RoomId, StartsAt, EndsAt, PaymentStatusId, PrepaymentAmount, PrepaymentDate) VALUES
(1, 1, '2026-01-23T13:00:00', '2026-01-23T20:00:00', 1, 3500.00, '2026-01-21'),
(2, 2, '2026-02-26T09:00:00', '2026-02-26T18:00:00', 2, 5000.00, '2026-02-24'),
(3, 2, '2026-03-03T10:00:00', '2026-03-03T19:00:00', 3, 0.00, NULL),
(4, 3, '2026-03-04T11:00:00', '2026-03-04T18:00:00', 3, 0.00, NULL);
GO
