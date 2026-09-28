/* =====================================================================
   Script 05 - Retrieve Data (Task 7, PA0201)
   Uses WHERE, ORDER BY and column aliases to filter, sort and present
   passenger, flight, booking and payment information, plus the two
   required reports (departing Johannesburg / arriving Cape Town).
   ===================================================================== */

USE [SAA_FlightBooking];
GO

-- 7.1 All passengers, sorted by surname
SELECT  PassengerId          AS [ID],
        FirstName + ' ' + LastName AS [Passenger Name],
        Email                AS [Email Address],
        Phone                AS [Contact Number]
FROM    dbo.Passengers
ORDER BY LastName, FirstName;
GO

-- 7.2 All flights with readable airport names
SELECT  f.FlightNumber                    AS [Flight],
        dep.City + ' (' + dep.IataCode + ')' AS [From],
        arr.City + ' (' + arr.IataCode + ')' AS [To],
        f.DepartureTime                   AS [Departs],
        f.ArrivalTime                     AS [Arrives],
        f.BaseFare                        AS [Base Fare (ZAR)]
FROM    dbo.Flights f
JOIN    dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
JOIN    dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
ORDER BY f.DepartureTime;
GO

-- 7.3 Booking overview with the booking passenger and payment status
SELECT  b.BookingReference AS [Reference],
        p.FirstName + ' ' + p.LastName AS [Booked By],
        b.Status           AS [Booking Status],
        pay.Amount         AS [Amount (ZAR)],
        pay.Status         AS [Payment Status]
FROM    dbo.Bookings b
JOIN    dbo.Passengers p ON p.PassengerId = b.PassengerId
LEFT JOIN dbo.Payments pay ON pay.BookingId = b.BookingId
ORDER BY b.BookingDate;
GO

-- 7.4 REPORT: Flights departing from Johannesburg
SELECT  f.FlightNumber      AS [Flight],
        arr.City            AS [Destination],
        f.DepartureTime     AS [Departure],
        f.BaseFare          AS [Fare (ZAR)]
FROM    dbo.Flights f
JOIN    dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
JOIN    dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
WHERE   dep.City = N'Johannesburg'
ORDER BY f.DepartureTime;
GO

-- 7.5 REPORT: Flights arriving in Cape Town
SELECT  f.FlightNumber      AS [Flight],
        dep.City            AS [Origin],
        f.ArrivalTime       AS [Arrival],
        f.BaseFare          AS [Fare (ZAR)]
FROM    dbo.Flights f
JOIN    dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
JOIN    dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
WHERE   arr.City = N'Cape Town'
ORDER BY f.ArrivalTime;
GO
