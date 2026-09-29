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

-- Additional scheduled flights across the domestic network (both directions).
INSERT INTO dbo.Flights (FlightNumber, DepartureAirportId, ArrivalAirportId, DepartureTime, ArrivalTime, Aircraft, SeatCapacity, BaseFare) VALUES
    ('SA302B', 2, 1, '2026-10-08T17:30:00', '2026-10-08T19:45:00', N'Airbus A320',     174, 1620.00), -- CPT -> JNB
    ('SA303',  1, 2, '2026-10-08T11:00:00', '2026-10-08T13:15:00', N'Airbus A320',     174, 1480.00), -- JNB -> CPT
    ('SA304',  2, 1, '2026-10-09T06:15:00', '2026-10-09T08:30:00', N'Airbus A319',     144, 1580.00), -- CPT -> JNB
    ('SA402',  3, 1, '2026-10-08T09:30:00', '2026-10-08T10:40:00', N'Boeing 737-800',  162, 1250.00), -- DUR -> JNB
    ('SA403',  1, 3, '2026-10-09T13:00:00', '2026-10-09T14:10:00', N'Boeing 737-800',  162, 1180.00), -- JNB -> DUR
    ('SA404',  3, 1, '2026-10-09T18:00:00', '2026-10-09T19:10:00', N'Airbus A320',     174, 1290.00), -- DUR -> JNB
    ('SA502',  2, 3, '2026-10-08T07:45:00', '2026-10-08T10:05:00', N'Boeing 737-800',  162, 1430.00), -- CPT -> DUR
    ('SA503',  3, 2, '2026-10-09T16:30:00', '2026-10-09T18:50:00', N'Airbus A320',     174, 1410.00), -- DUR -> CPT
    ('SA701',  1, 4, '2026-10-08T08:00:00', '2026-10-08T09:40:00', N'Embraer E190',    100, 1350.00), -- JNB -> PLZ
    ('SA702',  4, 1, '2026-10-08T10:30:00', '2026-10-08T12:10:00', N'Embraer E190',    100, 1370.00), -- PLZ -> JNB
    ('SA703',  2, 4, '2026-10-09T12:00:00', '2026-10-09T13:20:00', N'Embraer E190',    100, 1120.00), -- CPT -> PLZ
    ('SA704',  4, 2, '2026-10-09T14:10:00', '2026-10-09T15:30:00', N'Embraer E190',    100, 1140.00), -- PLZ -> CPT
    ('SA801',  1, 5, '2026-10-08T06:45:00', '2026-10-08T07:55:00', N'Embraer E190',    100, 1080.00), -- JNB -> BFN
    ('SA802',  5, 1, '2026-10-08T08:40:00', '2026-10-08T09:50:00', N'Embraer E190',    100, 1090.00), -- BFN -> JNB
    ('SA803',  2, 5, '2026-10-09T11:15:00', '2026-10-09T13:00:00', N'Embraer E190',    100, 1260.00), -- CPT -> BFN
    ('SA804',  5, 2, '2026-10-09T14:00:00', '2026-10-09T15:45:00', N'Embraer E190',    100, 1280.00), -- BFN -> CPT
    ('SA901',  1, 6, '2026-10-08T07:20:00', '2026-10-08T09:15:00', N'Boeing 737-800',  162, 1520.00), -- JNB -> GRJ
    ('SA902',  6, 1, '2026-10-08T10:00:00', '2026-10-08T11:55:00', N'Boeing 737-800',  162, 1540.00), -- GRJ -> JNB
    ('SA903',  2, 6, '2026-10-09T09:00:00', '2026-10-09T10:05:00', N'Embraer E190',    100, 990.00),  -- CPT -> GRJ
    ('SA904',  6, 2, '2026-10-09T11:00:00', '2026-10-09T12:05:00', N'Embraer E190',    100, 1010.00), -- GRJ -> CPT
    ('SA605',  1, 2, '2026-10-10T18:30:00', '2026-10-10T20:45:00', N'Airbus A330',     249, 1500.00), -- JNB -> CPT
    ('SA606',  2, 1, '2026-10-10T20:00:00', '2026-10-10T22:15:00', N'Airbus A330',     249, 1600.00); -- CPT -> JNB
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
