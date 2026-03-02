-- =============================================
-- ACSEL 2026 Database Schema (SQLite)
-- =============================================

-- Updates & News Table
CREATE TABLE IF NOT EXISTS UpdatesNews (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    PublishDate TEXT NOT NULL DEFAULT (datetime('now')),
    IsActive INTEGER NOT NULL DEFAULT 1
);

-- Registration Table
CREATE TABLE IF NOT EXISTS Registrations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Email TEXT NOT NULL,
    Phone TEXT,
    Institution TEXT,
    TicketType TEXT NOT NULL,
    RegistrationDate TEXT NOT NULL DEFAULT (datetime('now')),
    PaymentStatus TEXT NOT NULL DEFAULT 'Pending',
    Comments TEXT
);

-- Sample Seed Data for Updates
INSERT INTO UpdatesNews (Title, Content, PublishDate)
VALUES 
('Registration is now open', 'Registration is now open. Early bird ends 15 August 2026.', '2026-03-01'),
('Programme Schedule', 'The preliminary programme schedule has been released.', '2026-02-15');
