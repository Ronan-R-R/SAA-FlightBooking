/* =====================================================================
   Script 06 - Update Data (Task 8, PA0204)
   Updates a passenger email, a booking status and a payment status.
   SELECT statements show the before and after state for evidence.
   ===================================================================== */

USE [SAA_FlightBooking];
GO

-- 8.1 Update a passenger email address
SELECT PassengerId, Email AS [Email Before] FROM dbo.Passengers WHERE PassengerId = 3;
UPDATE dbo.Passengers
SET    Email = N'sipho.nkosi@flysaa-demo.co.za'
WHERE  PassengerId = 3;
SELECT PassengerId, Email AS [Email After] FROM dbo.Passengers WHERE PassengerId = 3;
GO

-- 8.2 Update a booking status (Pending -> Confirmed)
SELECT BookingId, BookingReference, Status AS [Status Before] FROM dbo.Bookings WHERE BookingReference = 'SAA002';
UPDATE dbo.Bookings
SET    Status = N'Confirmed'
WHERE  BookingReference = 'SAA002';
SELECT BookingId, BookingReference, Status AS [Status After] FROM dbo.Bookings WHERE BookingReference = 'SAA002';
GO

-- 8.3 Update a payment status (Pending -> Paid) for that booking
SELECT PaymentId, BookingId, Status AS [Payment Before]
FROM   dbo.Payments
WHERE  BookingId = (SELECT BookingId FROM dbo.Bookings WHERE BookingReference = 'SAA002');
UPDATE dbo.Payments
SET    Status = N'Paid'
WHERE  BookingId = (SELECT BookingId FROM dbo.Bookings WHERE BookingReference = 'SAA002');
SELECT PaymentId, BookingId, Status AS [Payment After]
FROM   dbo.Payments
WHERE  BookingId = (SELECT BookingId FROM dbo.Bookings WHERE BookingReference = 'SAA002');
GO
