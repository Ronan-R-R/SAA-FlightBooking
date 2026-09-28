/* =====================================================================
   Script 07 - Delete Data (Task 9, PA0203)
   Demonstrates how foreign key constraints enforce referential
   integrity when deleting a parent row.

   Part A attempts to delete a booking that still has child rows
   (BookingPassengers, Tickets, Payments). SQL Server blocks it with
   a REFERENCE constraint error - the booking is NOT deleted.

   Part B shows the correct order: remove the child rows first, then
   the parent booking succeeds. Wrapped in a transaction that is rolled
   back so the sample data is preserved for other scripts.
   ===================================================================== */

USE [SAA_FlightBooking];
GO

-- State before
SELECT BookingId, BookingReference, Status FROM dbo.Bookings WHERE BookingReference = 'SAA001';
GO

-- Part A: attempt to delete a booking that still has children.
-- Expected: Msg 547, DELETE fails, referential integrity preserved.
BEGIN TRY
    DELETE FROM dbo.Bookings WHERE BookingReference = 'SAA001';
END TRY
BEGIN CATCH
    PRINT 'DELETE blocked by referential integrity.';
    PRINT 'Error ' + CAST(ERROR_NUMBER() AS VARCHAR(10)) + ': ' + ERROR_MESSAGE();
END CATCH
GO

-- Confirm the booking still exists after the blocked delete
SELECT BookingId, BookingReference, Status FROM dbo.Bookings WHERE BookingReference = 'SAA001';
GO

-- Part B: correct deletion order (child rows first), rolled back so the
-- demonstration does not alter the seeded data.
BEGIN TRANSACTION;
    DECLARE @bid INT = (SELECT BookingId FROM dbo.Bookings WHERE BookingReference = 'SAA001');

    DELETE FROM dbo.Payments          WHERE BookingId = @bid;
    DELETE FROM dbo.Tickets           WHERE BookingId = @bid;
    DELETE FROM dbo.BookingPassengers WHERE BookingId = @bid;
    DELETE FROM dbo.Bookings          WHERE BookingId = @bid;

    PRINT 'Child rows removed first, parent booking then deleted successfully.';
    SELECT COUNT(*) AS [SAA001 rows remaining] FROM dbo.Bookings WHERE BookingReference = 'SAA001';
ROLLBACK TRANSACTION;  -- undo so seed data is intact
GO

-- Confirm rollback restored the booking
SELECT BookingId, BookingReference, Status FROM dbo.Bookings WHERE BookingReference = 'SAA001';
GO
