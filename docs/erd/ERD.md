# ERD - SAA Flight Booking

Entity Relationship Diagram source for the SAA Flight Booking database.
Render this with any Mermaid-capable tool (for example the Mermaid Live Editor
at https://mermaid.live) and export the image for the assessment document.

```mermaid
erDiagram
    AIRPORTS ||--o{ FLIGHTS : "departs from"
    AIRPORTS ||--o{ FLIGHTS : "arrives at"
    PASSENGERS ||--o{ BOOKINGS : "makes"
    BOOKINGS ||--o{ BOOKINGPASSENGERS : "includes"
    PASSENGERS ||--o{ BOOKINGPASSENGERS : "travels on"
    BOOKINGS ||--o{ TICKETS : "has"
    PASSENGERS ||--o{ TICKETS : "holds"
    FLIGHTS ||--o{ TICKETS : "issued for"
    BOOKINGS ||--o{ PAYMENTS : "paid by"

    AIRPORTS {
        int AirportId PK
        char IataCode UK
        nvarchar Name
        nvarchar City
        nvarchar Country
    }
    PASSENGERS {
        int PassengerId PK
        nvarchar FirstName
        nvarchar LastName
        varchar PassportNumber UK
        nvarchar Email UK
        varchar Phone
        date DateOfBirth
    }
    FLIGHTS {
        int FlightId PK
        varchar FlightNumber
        int DepartureAirportId FK
        int ArrivalAirportId FK
        datetime2 DepartureTime
        datetime2 ArrivalTime
        nvarchar Aircraft
        int SeatCapacity
        decimal BaseFare
    }
    BOOKINGS {
        int BookingId PK
        char BookingReference UK
        int PassengerId FK
        datetime2 BookingDate
        nvarchar Status
    }
    BOOKINGPASSENGERS {
        int BookingId PK,FK
        int PassengerId PK,FK
    }
    TICKETS {
        int TicketId PK
        int BookingId FK
        int PassengerId FK
        int FlightId FK
        varchar SeatNumber
        nvarchar TicketClass
        decimal Fare
        datetime2 IssuedDate
    }
    PAYMENTS {
        int PaymentId PK
        int BookingId FK
        decimal Amount
        datetime2 PaymentDate
        nvarchar Method
        nvarchar Status
    }
```

## Relationships

- Airports 1 to many Flights (departure and arrival).
- Passengers 1 to many Bookings (the passenger who books).
- Bookings many to many Passengers, resolved by BookingPassengers.
- Bookings 1 to many Tickets; Passengers 1 to many Tickets; Flights 1 to many Tickets.
- Tickets carry a composite foreign key (BookingId, PassengerId) to BookingPassengers,
  so a ticket can only be issued for a passenger who is on the booking.
- Bookings 1 to many Payments.
