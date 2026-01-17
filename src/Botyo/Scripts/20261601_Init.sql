CREATE TABLE Notifications (
                               Id INTEGER PRIMARY KEY AUTOINCREMENT,
                               Content TEXT NOT NULL,
                               Cron TEXT NOT NULL,
                               Active INTEGER NOT NULL
);