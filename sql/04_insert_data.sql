/* =====================================================================
   Script 04 - Insert Sample Data (Task 6, PA0202)
   Minimums required by the brief: 3 airports, 5 flights, 5 passengers,
   3 bookings, corresponding tickets and payment records.
   Insert order respects referential integrity.
   ===================================================================== */

USE [SAA_FlightBooking];
GO

INSERT INTO dbo.Airports (IataCode, Name, City, Country) VALUES
    ('JNB', N'O.R. Tambo International Airport', N'Johannesburg', N'South Africa'),
    ('CPT', N'Cape Town International Airport',   N'Cape Town',    N'South Africa'),
    ('DUR', N'King Shaka International Airport',  N'Durban',       N'South Africa'),
    ('PLZ', N'Chief Dawid Stuurman International Airport', N'Gqeberha', N'South Africa'),
    ('BFN', N'Bram Fischer International Airport', N'Bloemfontein', N'South Africa'),
    ('GRJ', N'George Airport',                    N'George',       N'South Africa');
GO

INSERT INTO dbo.Passengers (FirstName, LastName, PassportNumber, Email, Phone, DateOfBirth) VALUES
    (N'Thabo',    N'Mokoena',  'ZA1000001', N'thabo.mokoena@example.co.za',  '+27 82 111 2233', '1990-03-14'),
    (N'Naledi',   N'Dlamini',  'ZA1000002', N'naledi.dlamini@example.co.za', '+27 83 222 3344', '1985-11-02'),
    (N'Sipho',    N'Nkosi',    'ZA1000003', N'sipho.nkosi@example.co.za',    '+27 84 333 4455', '1998-07-21'),
    (N'Aisha',    N'Patel',    'ZA1000004', N'aisha.patel@example.co.za',    '+27 82 444 5566', '1992-01-30'),
    (N'Johan',    N'van Wyk',  'ZA1000005', N'johan.vanwyk@example.co.za',   '+27 71 555 6677', '1979-09-09');
GO

INSERT INTO dbo.Flights (FlightNumber, DepartureAirportId, ArrivalAirportId, DepartureTime, ArrivalTime, Aircraft, SeatCapacity, BaseFare) VALUES
    ('SA301', 1, 2, '2026-10-05T06:00:00', '2026-10-05T08:15:00', N'Airbus A320', 174, 1500.00), -- JNB -> CPT
    ('SA302', 2, 1, '2026-10-05T09:30:00', '2026-10-05T11:45:00', N'Airbus A320', 174, 1600.00), -- CPT -> JNB
    ('SA401', 1, 3, '2026-10-06T07:00:00', '2026-10-06T08:10:00', N'Boeing 737-800', 162, 1200.00), -- JNB -> DUR
    ('SA501', 3, 2, '2026-10-06T12:00:00', '2026-10-06T14:20:00', N'Boeing 737-800', 162, 1400.00), -- DUR -> CPT
    ('SA601', 1, 2, '2026-10-07T15:00:00', '2026-10-07T17:15:00', N'Airbus A330', 249, 1550.00); -- JNB -> CPT
GO

INSERT INTO dbo.Bookings (BookingReference, PassengerId, BookingDate, Status) VALUES
    ('SAA001', 1, '2026-09-20T10:15:00', N'Confirmed'),
    ('SAA002', 2, '2026-09-21T14:40:00', N'Pending'),
    ('SAA003', 3, '2026-09-22T09:05:00', N'Confirmed');
GO

-- Many-to-many: travellers on each booking
INSERT INTO dbo.BookingPassengers (BookingId, PassengerId) VALUES
    (1, 1), (1, 4),   -- Booking SAA001: Thabo + Aisha
    (2, 2),           -- Booking SAA002: Naledi
    (3, 3), (3, 5);   -- Booking SAA003: Sipho + Johan
GO

INSERT INTO dbo.Tickets (BookingId, PassengerId, FlightId, SeatNumber, TicketClass, Fare) VALUES
    (1, 1, 1, '12A', N'Economy',  1500.00),
    (1, 4, 1, '12B', N'Economy',  1500.00),
    (2, 2, 2, '03A', N'Business', 3200.00),
    (3, 3, 5, '20C', N'Economy',  1550.00),
    (3, 5, 5, '20D', N'Economy',  1550.00);
GO

INSERT INTO dbo.Payments (BookingId, Amount, PaymentDate, Method, Status) VALUES
    (1, 3000.00, '2026-09-20T10:20:00', N'Card', N'Paid'),
    (2, 3200.00, '2026-09-21T14:45:00', N'EFT',  N'Pending'),
    (3, 3100.00, '2026-09-22T09:10:00', N'Card', N'Paid');
GO
