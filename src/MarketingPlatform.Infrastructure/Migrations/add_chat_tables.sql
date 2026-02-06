-- Add Chat Tables for Support Chat Feature
-- Run this script against your MarketingPlatformDb database

-- Check if IsOnline and LastSeenAt columns exist on AspNetUsers, add if not
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'IsOnline')
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsOnline] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsOnline column to AspNetUsers';
END
ELSE
BEGIN
    PRINT 'IsOnline column already exists';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'LastSeenAt')
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastSeenAt] DATETIME2 NULL;
    PRINT 'Added LastSeenAt column to AspNetUsers';
END
ELSE
BEGIN
    PRINT 'LastSeenAt column already exists';
END
GO

-- Create ChatRooms table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatRooms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatRooms] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [GuestName] NVARCHAR(200) NULL,
        [GuestEmail] NVARCHAR(256) NULL,
        [CustomerId] NVARCHAR(450) NULL,
        [AssignedEmployeeId] NVARCHAR(450) NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ChatRooms] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatRooms_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatRooms_AspNetUsers_AssignedEmployeeId] FOREIGN KEY ([AssignedEmployeeId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    );

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_ChatRooms_CustomerId] ON [dbo].[ChatRooms] ([CustomerId]);
    CREATE NONCLUSTERED INDEX [IX_ChatRooms_AssignedEmployeeId] ON [dbo].[ChatRooms] ([AssignedEmployeeId]);
    CREATE NONCLUSTERED INDEX [IX_ChatRooms_Status] ON [dbo].[ChatRooms] ([Status]);
    CREATE NONCLUSTERED INDEX [IX_ChatRooms_CreatedAt] ON [dbo].[ChatRooms] ([CreatedAt]);
    CREATE NONCLUSTERED INDEX [IX_ChatRooms_GuestEmail] ON [dbo].[ChatRooms] ([GuestEmail]);

    PRINT 'Created ChatRooms table with indexes';
END
ELSE
BEGIN
    PRINT 'ChatRooms table already exists';
END
GO

-- Create ChatMessages table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ChatRoomId] INT NOT NULL,
        [SenderId] NVARCHAR(450) NULL,
        [MessageText] NVARCHAR(2000) NOT NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [MessageType] INT NOT NULL DEFAULT 0,
        [AttachmentUrl] NVARCHAR(500) NULL,
        [AttachmentFileName] NVARCHAR(255) NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatMessages_ChatRooms_ChatRoomId] FOREIGN KEY ([ChatRoomId])
            REFERENCES [dbo].[ChatRooms] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    );

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_ChatMessages_ChatRoomId] ON [dbo].[ChatMessages] ([ChatRoomId]);
    CREATE NONCLUSTERED INDEX [IX_ChatMessages_SenderId] ON [dbo].[ChatMessages] ([SenderId]);
    CREATE NONCLUSTERED INDEX [IX_ChatMessages_IsRead] ON [dbo].[ChatMessages] ([IsRead]);
    CREATE NONCLUSTERED INDEX [IX_ChatMessages_SentAt] ON [dbo].[ChatMessages] ([SentAt]);

    PRINT 'Created ChatMessages table with indexes';
END
ELSE
BEGIN
    PRINT 'ChatMessages table already exists';
END
GO

-- Add migration history entry if tables were created
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260119080146_AddChatEntities')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260119080146_AddChatEntities', '8.0.0');
    PRINT 'Added migration history entry';
END
GO

PRINT 'Chat tables setup complete!';
