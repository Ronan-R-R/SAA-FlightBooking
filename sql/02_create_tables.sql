/* =====================================================================
   Script 02 - Create Tables (Task 3, PA0102)
   Primary keys, data types and CHECK/UNIQUE constraints.
   Foreign key relationships are added in 03_relationships.sql.
   ===================================================================== */

USE [SAA_FlightBooking];
GO

CREATE TABLE dbo.Airports
(
    AirportId   INT IDENTITY(1,1) NOT NULL,
    IataCode    CHAR(3)        NOT NULL,
    Name        NVARCHAR(100)  NOT NULL,
    City        NVARCHAR(100)  NOT NULL,
    Country     NVARCHAR(100)  NOT NULL,
    CONSTRAINT PK_Airports PRIMARY KEY (AirportId),
    CONSTRAINT UQ_Airports_IataCode UNIQUE (IataCode)
);
GO

CREATE TABLE dbo.Passengers
(
    PassengerId     INT IDENTITY(1,1) NOT NULL,
    FirstName       NVARCHAR(50)  NOT NULL,
    LastName        NVARCHAR(50)  NOT NULL,
    PassportNumber  VARCHAR(20)   NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    Phone           VARCHAR(20)   NULL,
    DateOfBirth     DATE          NULL,
    CONSTRAINT PK_Passengers PRIMARY KEY (PassengerId),
    CONSTRAINT UQ_Passengers_Passport UNIQUE (PassportNumber),
    CONSTRAINT UQ_Passengers_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Flights
(
    FlightId            INT IDENTITY(1,1) NOT NULL,
    FlightNumber        VARCHAR(10)   NOT NULL,
    DepartureAirportId  INT           NOT NULL,
    ArrivalAirportId    INT           NOT NULL,
    DepartureTime       DATETIME2(0)  NOT NULL,
    ArrivalTime         DATETIME2(0)  NOT NULL,
    Aircraft            NVARCHAR(50)  NULL,
    SeatCapacity        INT           NOT NULL,
    BaseFare            DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_Flights PRIMARY KEY (FlightId),
    CONSTRAINT CK_Flights_Airports_Differ CHECK (DepartureAirportId <> ArrivalAirportId),
    CONSTRAINT CK_Flights_Times CHECK (ArrivalTime > DepartureTime),
    CONSTRAINT CK_Flights_SeatCapacity CHECK (SeatCapacity > 0),
    CONSTRAINT CK_Flights_BaseFare CHECK (BaseFare >= 0)
);
GO

CREATE TABLE dbo.Bookings
(
    BookingId         INT IDENTITY(1,1) NOT NULL,
    BookingReference  CHAR(6)       NOT NULL,
    PassengerId       INT           NOT NULL,   -- passenger who made the booking
    BookingDate       DATETIME2(0)  NOT NULL CONSTRAINT DF_Bookings_Date DEFAULT (SYSDATETIME()),
    Status            NVARCHAR(20)  NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT (N'Pending'),
    CONSTRAINT PK_Bookings PRIMARY KEY (BookingId),
    CONSTRAINT UQ_Bookings_Reference UNIQUE (BookingReference),
    CONSTRAINT CK_Bookings_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled'))
);
GO

/* Associative table resolving the many-to-many relationship
   between Bookings and Passengers (travellers on a booking). */
CREATE TABLE dbo.BookingPassengers
(
    BookingId    INT NOT NULL,
    PassengerId  INT NOT NULL,
    CONSTRAINT PK_BookingPassengers PRIMARY KEY (BookingId, PassengerId)
);
GO

CREATE TABLE dbo.Tickets
(
    TicketId     INT IDENTITY(1,1) NOT NULL,
    BookingId    INT           NOT NULL,
    PassengerId  INT           NOT NULL,
    FlightId     INT           NOT NULL,
    SeatNumber   VARCHAR(5)    NOT NULL,
    TicketClass  NVARCHAR(20)  NOT NULL CONSTRAINT DF_Tickets_Class DEFAULT (N'Economy'),
    Fare         DECIMAL(10,2) NOT NULL,
    IssuedDate   DATETIME2(0)  NOT NULL CONSTRAINT DF_Tickets_Issued DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Tickets PRIMARY KEY (TicketId),
    CONSTRAINT UQ_Tickets_Seat UNIQUE (FlightId, SeatNumber),
    CONSTRAINT CK_Tickets_Class CHECK (TicketClass IN (N'Economy', N'Business', N'First')),
    CONSTRAINT CK_Tickets_Fare CHECK (Fare >= 0)
);
GO

CREATE TABLE dbo.Payments
(
    PaymentId    INT IDENTITY(1,1) NOT NULL,
    BookingId    INT           NOT NULL,
    Amount       DECIMAL(10,2) NOT NULL,
    PaymentDate  DATETIME2(0)  NOT NULL CONSTRAINT DF_Payments_Date DEFAULT (SYSDATETIME()),
    Method       NVARCHAR(20)  NOT NULL,
    Status       NVARCHAR(20)  NOT NULL CONSTRAINT DF_Payments_Status DEFAULT (N'Pending'),
    CONSTRAINT PK_Payments PRIMARY KEY (PaymentId),
    CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Payments_Method CHECK (Method IN (N'Card', N'EFT', N'Cash')),
    CONSTRAINT CK_Payments_Status CHECK (Status IN (N'Pending', N'Paid', N'Refunded'))
);
GO
