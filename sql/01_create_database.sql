/* =====================================================================
   South African Airways Flight Booking System
   Script 01 - Create Database
   Module: MDB622 Design and Manipulate Databases
   Target: Microsoft SQL Server (SSMS / .\SQLEXPRESS)
   ===================================================================== */

IF DB_ID(N'SAA_FlightBooking') IS NOT NULL
BEGIN
    ALTER DATABASE [SAA_FlightBooking] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [SAA_FlightBooking];
END
GO

CREATE DATABASE [SAA_FlightBooking];
GO

USE [SAA_FlightBooking];
GO
