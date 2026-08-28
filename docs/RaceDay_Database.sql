USE master;
GO

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO
-- Stores all system users (Organisers and Participants), distinguished by the Role column
CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO

CREATE TABLE EventTypes
(
    EventTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);
GO
 -- Events created by Organisers; each event belongs to one Organiser
CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventTypeID INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Events_EventTypes
        FOREIGN KEY (EventTypeID)
        REFERENCES EventTypes(EventTypeID),

    CONSTRAINT CK_Events_Distance
        CHECK (Distance > 0)
);
GO
 -- Distance/category options available within an event (e.g. 5km, 10km)
CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    CategoryType NVARCHAR(50) NOT NULL,

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
);
GO

CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),

    CONSTRAINT UQ_Enrolments_Participant_Event
        UNIQUE (ParticipantID, EventID)
);
GO

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NOT NULL,
    FinishingPosition INT NOT NULL,

    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolments(EnrolmentID),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentID),

    CONSTRAINT CK_Results_FinishingPosition
        CHECK (FinishingPosition > 0)
);
GO

INSERT INTO EventTypes (TypeName, Description)
VALUES
('Run', 'Road running events'),
('Walk', 'Community and charity walking events'),
('Cycle', 'Road cycling events');
GO

INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
VALUES
    ('Thabo', 'Mokoena', 'thabo.mokoena@raceday.co.za', 'HASHED_PASSWORD_1', 'Organiser', '0712345678'),
    ('Lerato', 'Nkosi', 'lerato.nkosi@raceday.co.za', 'HASHED_PASSWORD_2', 'Organiser', '0723456789'),
    ('Mpho', 'Dlamini', 'mpho.dlamini@example.com', 'HASHED_PASSWORD_3', 'Participant', '0734567890'),
    ('Karabo', 'Molefe', 'karabo.molefe@example.com', 'HASHED_PASSWORD_4', 'Participant', '0745678901');
GO

INSERT INTO Events
    (OrganiserID, EventTypeID, Name, Description, EventDate, Location, Distance)
VALUES
    (1, 1, 'Polokwane City Run', 
     'A community road running event through Polokwane.',
     '2026-10-10', 'Polokwane, Limpopo', 10.00),

    (2, 2, 'Limpopo Charity Walk', 
     'A community walking event supporting local charities.',
     '2026-11-07', 'Polokwane, Limpopo', 5.00),

    (1, 3, 'Limpopo Cycle Challenge', 
     'A road cycling event for recreational and competitive cyclists.',
     '2026-12-05', 'Polokwane, Limpopo', 50.00);
GO

INSERT INTO Categories
    (EventID, CategoryName, CategoryType)
VALUES
    (1, 'Under 20', 'Age'),
    (1, 'Senior', 'Age'),
    
    (2, 'Under 20', 'Age'),
    (2, 'Senior', 'Age'),

    (3, '10 km', 'Distance'),
    (3, '50 km', 'Distance');
GO

INSERT INTO Enrolments
    (ParticipantID, EventID, CategoryID)
VALUES
    (3, 1, 2),
    (3, 2, 4),
    (4, 1, 1),
    (4, 3, 6);
GO

INSERT INTO Results
    (EnrolmentID, FinishTime, FinishingPosition)
VALUES
    (1, '01:12:35', 15),
    (2, '00:48:20', 8),
    (3, '01:18:10', 22),
    (4, '02:15:45', 12);
GO
