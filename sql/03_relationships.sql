/* =====================================================================
   Script 03 - Relationships (Task 4, PA0103)
   Foreign key constraints enforcing referential integrity.

   Relationship summary:
     Airports  1--*  Flights        (departure)
     Airports  1--*  Flights        (arrival)
     Passengers 1--* Bookings       (booker)
     Bookings  *--*  Passengers     (via BookingPassengers)
     Bookings  1--*  Tickets
     Passengers 1--* Tickets
     Flights   1--*  Tickets
     Bookings  1--*  Payments
   Deletes are intentionally NOT cascaded so that referential
   integrity blocks removal of a parent that still has children
   (demonstrated in Task 9).
   ===================================================================== */

USE [SAA_FlightBooking];
GO

ALTER TABLE dbo.Flights
    ADD CONSTRAINT FK_Flights_DepartureAirport
        FOREIGN KEY (DepartureAirportId) REFERENCES dbo.Airports(AirportId);
ALTER TABLE dbo.Flights
    ADD CONSTRAINT FK_Flights_ArrivalAirport
        FOREIGN KEY (ArrivalAirportId) REFERENCES dbo.Airports(AirportId);
GO

ALTER TABLE dbo.Bookings
    ADD CONSTRAINT FK_Bookings_Passenger
        FOREIGN KEY (PassengerId) REFERENCES dbo.Passengers(PassengerId);
GO

ALTER TABLE dbo.BookingPassengers
    ADD CONSTRAINT FK_BookingPassengers_Booking
        FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId);
ALTER TABLE dbo.BookingPassengers
    ADD CONSTRAINT FK_BookingPassengers_Passenger
        FOREIGN KEY (PassengerId) REFERENCES dbo.Passengers(PassengerId);
GO

ALTER TABLE dbo.Tickets
    ADD CONSTRAINT FK_Tickets_Booking
        FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId);
ALTER TABLE dbo.Tickets
    ADD CONSTRAINT FK_Tickets_Passenger
        FOREIGN KEY (PassengerId) REFERENCES dbo.Passengers(PassengerId);
ALTER TABLE dbo.Tickets
    ADD CONSTRAINT FK_Tickets_Flight
        FOREIGN KEY (FlightId) REFERENCES dbo.Flights(FlightId);
/* Composite FK guaranteeing a ticketed passenger is actually
   listed on the booking (integrity across the associative table). */
ALTER TABLE dbo.Tickets
    ADD CONSTRAINT FK_Tickets_BookingPassenger
        FOREIGN KEY (BookingId, PassengerId)
        REFERENCES dbo.BookingPassengers(BookingId, PassengerId);
GO

ALTER TABLE dbo.Payments
    ADD CONSTRAINT FK_Payments_Booking
        FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId);
GO
