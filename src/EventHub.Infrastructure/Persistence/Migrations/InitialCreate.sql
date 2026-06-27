-- EventHub - Initial Database Migration
-- PostgreSQL 16
-- Generated from Entity Framework Core Model

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================
-- MÓDULO: IDENTIDAD Y AUTENTICACIÓN
-- ============================================

CREATE TABLE "Users" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(256) NOT NULL,
    "PasswordHash" VARCHAR(512) NOT NULL,
    "AvatarUrl" VARCHAR(512),
    "EmailVerified" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "FailedLoginAttempts" INTEGER NOT NULL DEFAULT 0,
    "LockoutEnd" TIMESTAMPTZ,
    "LastLoginAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Users_IsActive" ON "Users" ("IsActive");
CREATE INDEX "IX_Users_LastLoginAt" ON "Users" ("LastLoginAt");

CREATE TABLE "Roles" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Name" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(200),
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");

CREATE TABLE "UserRoles" (
    "UserId" UUID NOT NULL,
    "RoleId" UUID NOT NULL,
    "AssignedBy" UUID,
    "AssignedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoles_Users_AssignedBy" FOREIGN KEY ("AssignedBy") REFERENCES "Users" ("Id")
);

CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");

-- ============================================
-- MÓDULO: TOKENS Y SEGURIDAD
-- ============================================

CREATE TABLE "RefreshTokens" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Token" VARCHAR(512) NOT NULL,
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "IsRevoked" BOOLEAN NOT NULL DEFAULT FALSE,
    "RevokedAt" TIMESTAMPTZ,
    "ReplacedByToken" VARCHAR(512),
    "CreatedByIp" VARCHAR(45) NOT NULL,
    "RevokedByIp" VARCHAR(45),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");
CREATE INDEX "IX_RefreshTokens_UserId_IsRevoked" ON "RefreshTokens" ("UserId", "IsRevoked");

CREATE TABLE "EmailVerificationTokens" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Token" VARCHAR(256) NOT NULL,
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "UsedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_EmailVerificationTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmailVerificationTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_EmailVerificationTokens_Token" ON "EmailVerificationTokens" ("Token");

CREATE TABLE "PasswordResetTokens" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Token" VARCHAR(256) NOT NULL,
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PasswordResetTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_PasswordResetTokens_Token" ON "PasswordResetTokens" ("Token");

CREATE TABLE "LoginAttempts" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID,
    "Email" VARCHAR(256) NOT NULL,
    "IpAddress" VARCHAR(45) NOT NULL,
    "IsSuccess" BOOLEAN NOT NULL,
    "FailureReason" VARCHAR(100),
    "AttemptedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_LoginAttempts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LoginAttempts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_LoginAttempts_UserId_AttemptedAt" ON "LoginAttempts" ("UserId", "AttemptedAt");
CREATE INDEX "IX_LoginAttempts_IpAddress_AttemptedAt" ON "LoginAttempts" ("IpAddress", "AttemptedAt");

-- ============================================
-- MÓDULO: NOTIFICACIONES
-- ============================================

CREATE TABLE "NotificationPreferences" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "EventReminders" BOOLEAN NOT NULL DEFAULT TRUE,
    "TicketPurchase" BOOLEAN NOT NULL DEFAULT TRUE,
    "EventStatusChanges" BOOLEAN NOT NULL DEFAULT TRUE,
    "PromotionsAndNews" BOOLEAN NOT NULL DEFAULT FALSE,
    "StaffInvitations" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_NotificationPreferences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_NotificationPreferences_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_NotificationPreferences_UserId" ON "NotificationPreferences" ("UserId");

CREATE TABLE "EmailLogs" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID,
    "ToEmail" VARCHAR(256) NOT NULL,
    "Subject" VARCHAR(500) NOT NULL,
    "Body" TEXT NOT NULL,
    "TemplateType" VARCHAR(50) NOT NULL,
    "Status" VARCHAR(10) NOT NULL DEFAULT 'Pending',
    "ErrorMessage" TEXT,
    "SentAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_EmailLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmailLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_EmailLogs_UserId" ON "EmailLogs" ("UserId");
CREATE INDEX "IX_EmailLogs_Status" ON "EmailLogs" ("Status");

-- ============================================
-- MÓDULO: CATEGORÍAS Y TAGS
-- ============================================

CREATE TABLE "Categories" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Name" VARCHAR(100) NOT NULL,
    "Slug" VARCHAR(120) NOT NULL,
    "Description" VARCHAR(500),
    "IconName" VARCHAR(50),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Categories_Name" ON "Categories" ("Name");
CREATE UNIQUE INDEX "IX_Categories_Slug" ON "Categories" ("Slug");

CREATE TABLE "Tags" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Name" VARCHAR(80) NOT NULL,
    "Slug" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Tags" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Tags_Name" ON "Tags" ("Name");
CREATE UNIQUE INDEX "IX_Tags_Slug" ON "Tags" ("Slug");

-- ============================================
-- MÓDULO: SEDES
-- ============================================

CREATE TABLE "Venues" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedBy" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Address" VARCHAR(300) NOT NULL,
    "City" VARCHAR(100) NOT NULL,
    "State" VARCHAR(100),
    "Country" VARCHAR(100) NOT NULL,
    "ZipCode" VARCHAR(20),
    "Latitude" DECIMAL(9,6),
    "Longitude" DECIMAL(9,6),
    "Capacity" INTEGER NOT NULL CHECK ("Capacity" > 0),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DeletedAt" TIMESTAMPTZ,
    CONSTRAINT "PK_Venues" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Venues_Users_CreatedBy" FOREIGN KEY ("CreatedBy") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_Venues_CreatedBy" ON "Venues" ("CreatedBy");
CREATE INDEX "IX_Venues_City_Country" ON "Venues" ("City", "Country");

-- ============================================
-- MÓDULO: EVENTOS (TABLA PRINCIPAL)
-- ============================================

CREATE TABLE "Events" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizerId" UUID NOT NULL,
    "CategoryId" UUID,
    "VenueId" UUID,
    "Title" VARCHAR(300) NOT NULL,
    "Slug" VARCHAR(350) NOT NULL,
    "Description" TEXT,
    "StartDate" TIMESTAMPTZ NOT NULL,
    "EndDate" TIMESTAMPTZ NOT NULL,
    "Timezone" VARCHAR(50) NOT NULL DEFAULT 'UTC',
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Draft',
    "MainImageUrl" VARCHAR(512),
    "ThumbnailUrl" VARCHAR(512),
    "CardImageUrl" VARCHAR(512),
    "HeroImageUrl" VARCHAR(512),
    "MaxAttendees" INTEGER,
    "IsFeatured" BOOLEAN NOT NULL DEFAULT FALSE,
    "PublishedAt" TIMESTAMPTZ,
    "CancelledAt" TIMESTAMPTZ,
    "CancellationReason" TEXT,
    "CompletedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DeletedAt" TIMESTAMPTZ,
    CONSTRAINT "PK_Events" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Events_Users_OrganizerId" FOREIGN KEY ("OrganizerId") REFERENCES "Users" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_Events_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Events_Venues_VenueId" FOREIGN KEY ("VenueId") REFERENCES "Venues" ("Id") ON DELETE SET NULL,
    CONSTRAINT "CK_Events_Status" CHECK ("Status" IN ('Draft', 'Published', 'Cancelled', 'Completed'))
);

CREATE UNIQUE INDEX "IX_Events_Slug" ON "Events" ("Slug");
CREATE INDEX "IX_Events_Status_StartDate" ON "Events" ("Status", "StartDate") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Events_OrganizerId_Status" ON "Events" ("OrganizerId", "Status");
CREATE INDEX "IX_Events_CategoryId_Status" ON "Events" ("CategoryId", "Status");
CREATE INDEX "IX_Events_VenueId_Status" ON "Events" ("VenueId", "Status");

CREATE TABLE "ScheduledPublications" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "EventId" UUID NOT NULL,
    "ScheduledAt" TIMESTAMPTZ NOT NULL,
    "IsExecuted" BOOLEAN NOT NULL DEFAULT FALSE,
    "ExecutedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_ScheduledPublications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ScheduledPublications_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_ScheduledPublications_EventId" ON "ScheduledPublications" ("EventId");

CREATE TABLE "EventImages" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "EventId" UUID NOT NULL,
    "ImageUrl" VARCHAR(512) NOT NULL,
    "OrderIndex" INTEGER NOT NULL DEFAULT 0,
    "AltText" VARCHAR(200),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_EventImages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EventImages_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_EventImages_EventId_OrderIndex" ON "EventImages" ("EventId", "OrderIndex");

CREATE TABLE "EventTags" (
    "EventId" UUID NOT NULL,
    "TagId" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_EventTags" PRIMARY KEY ("EventId", "TagId"),
    CONSTRAINT "FK_EventTags_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventTags_Tags_TagId" FOREIGN KEY ("TagId") REFERENCES "Tags" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_EventTags_TagId" ON "EventTags" ("TagId");

CREATE TABLE "EventFavorites" (
    "UserId" UUID NOT NULL,
    "EventId" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_EventFavorites" PRIMARY KEY ("UserId", "EventId"),
    CONSTRAINT "FK_EventFavorites_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventFavorites_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_EventFavorites_EventId" ON "EventFavorites" ("EventId");

-- ============================================
-- MÓDULO: TIPOS DE BOLETO Y DESCUENTOS
-- ============================================

CREATE TABLE "TicketTypes" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "EventId" UUID NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT,
    "Price" DECIMAL(10,2) NOT NULL CHECK ("Price" >= 0),
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'USD',
    "TotalQuantity" INTEGER NOT NULL CHECK ("TotalQuantity" > 0),
    "SoldQuantity" INTEGER NOT NULL DEFAULT 0,
    "MinPerOrder" INTEGER NOT NULL DEFAULT 1,
    "MaxPerOrder" INTEGER NOT NULL DEFAULT 10,
    "Type" VARCHAR(20) NOT NULL DEFAULT 'Standard',
    "SalesStartAt" TIMESTAMPTZ,
    "SalesEndAt" TIMESTAMPTZ,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_TicketTypes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TicketTypes_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_TicketTypes_EventId_IsActive" ON "TicketTypes" ("EventId", "IsActive");

CREATE TABLE "DiscountCodes" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "EventId" UUID NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(300),
    "Type" VARCHAR(15) NOT NULL,
    "Value" DECIMAL(10,2) NOT NULL CHECK ("Value" > 0),
    "MaxTotalUses" INTEGER NOT NULL,
    "CurrentUses" INTEGER NOT NULL DEFAULT 0,
    "MaxUsesPerUser" INTEGER NOT NULL DEFAULT 1,
    "MinPurchaseAmount" DECIMAL(10,2),
    "StartsAt" TIMESTAMPTZ,
    "EndsAt" TIMESTAMPTZ,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_DiscountCodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DiscountCodes_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_DiscountCodes_Type" CHECK ("Type" IN ('Percentage', 'FixedAmount'))
);

CREATE UNIQUE INDEX "IX_DiscountCodes_Code_EventId" ON "DiscountCodes" ("Code", "EventId");
CREATE INDEX "IX_DiscountCodes_EventId_IsActive" ON "DiscountCodes" ("EventId", "IsActive");

-- ============================================
-- MÓDULO: ÓRDENES Y COMPRAS
-- ============================================

CREATE TABLE "Orders" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "DiscountCodeId" UUID,
    "OrderNumber" VARCHAR(30) NOT NULL,
    "SubtotalAmount" DECIMAL(10,2) NOT NULL,
    "DiscountAmount" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "TotalAmount" DECIMAL(10,2) NOT NULL,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'USD',
    "Status" VARCHAR(15) NOT NULL DEFAULT 'Pending',
    "CancelledAt" TIMESTAMPTZ,
    "CancellationReason" VARCHAR(500),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ConfirmedAt" TIMESTAMPTZ,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_Orders_DiscountCodes_DiscountCodeId" FOREIGN KEY ("DiscountCodeId") REFERENCES "DiscountCodes" ("Id") ON DELETE SET NULL,
    CONSTRAINT "CK_Orders_Status" CHECK ("Status" IN ('Pending', 'Confirmed', 'Cancelled', 'Expired'))
);

CREATE UNIQUE INDEX "IX_Orders_OrderNumber" ON "Orders" ("OrderNumber");
CREATE INDEX "IX_Orders_UserId_Status" ON "Orders" ("UserId", "Status", "CreatedAt");
CREATE INDEX "IX_Orders_Status" ON "Orders" ("Status", "CreatedAt");

CREATE TABLE "OrderItems" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrderId" UUID NOT NULL,
    "TicketTypeId" UUID NOT NULL,
    "Quantity" INTEGER NOT NULL CHECK ("Quantity" > 0),
    "UnitPrice" DECIMAL(10,2) NOT NULL,
    "Subtotal" DECIMAL(10,2) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_TicketTypes_TicketTypeId" FOREIGN KEY ("TicketTypeId") REFERENCES "TicketTypes" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
CREATE INDEX "IX_OrderItems_TicketTypeId" ON "OrderItems" ("TicketTypeId");

-- ============================================
-- MÓDULO: BOLETOS INDIVIDUALES (con QR)
-- ============================================

CREATE TABLE "Tickets" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrderItemId" UUID NOT NULL,
    "OrderId" UUID NOT NULL,
    "EventId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "TicketNumber" VARCHAR(30) NOT NULL,
    "QrCodeData" TEXT NOT NULL,
    "QrCodeImageUrl" VARCHAR(512),
    "Status" VARCHAR(15) NOT NULL DEFAULT 'Active',
    "CheckedInAt" TIMESTAMPTZ,
    "CheckedInBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Tickets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Tickets_OrderItems_OrderItemId" FOREIGN KEY ("OrderItemId") REFERENCES "OrderItems" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Tickets_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_Tickets_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_Tickets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_Tickets_Users_CheckedInBy" FOREIGN KEY ("CheckedInBy") REFERENCES "Users" ("Id") ON DELETE SET NULL,
    CONSTRAINT "CK_Tickets_Status" CHECK ("Status" IN ('Active', 'Used', 'Cancelled', 'Refunded'))
);

CREATE UNIQUE INDEX "IX_Tickets_TicketNumber" ON "Tickets" ("TicketNumber");
CREATE UNIQUE INDEX "IX_Tickets_QrCodeData" ON "Tickets" ("QrCodeData");
CREATE INDEX "IX_Tickets_UserId_Status" ON "Tickets" ("UserId", "Status", "CreatedAt");
CREATE INDEX "IX_Tickets_EventId_Status" ON "Tickets" ("EventId", "Status", "CreatedAt");
CREATE INDEX "IX_Tickets_EventId_CheckedIn" ON "Tickets" ("EventId", "Status") WHERE "Status" = 'Active';

CREATE TABLE "TicketReservations" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "TicketTypeId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "Quantity" INTEGER NOT NULL CHECK ("Quantity" > 0),
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "IsConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "OrderId" UUID,
    "ConfirmedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_TicketReservations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TicketReservations_TicketTypes_TicketTypeId" FOREIGN KEY ("TicketTypeId") REFERENCES "TicketTypes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TicketReservations_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_TicketReservations_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_TicketReservations_ExpiresAt" ON "TicketReservations" ("ExpiresAt") WHERE "IsConfirmed" = FALSE;
CREATE INDEX "IX_TicketReservations_TicketTypeId" ON "TicketReservations" ("TicketTypeId", "IsConfirmed");

-- ============================================
-- MÓDULO: STAFF
-- ============================================

CREATE TABLE "EventStaff" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "EventId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "InvitedBy" UUID NOT NULL,
    "Status" VARCHAR(10) NOT NULL DEFAULT 'Pending',
    "InvitedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "AcceptedAt" TIMESTAMPTZ,
    "RemovedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_EventStaff" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EventStaff_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventStaff_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventStaff_Users_InvitedBy" FOREIGN KEY ("InvitedBy") REFERENCES "Users" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "CK_EventStaff_Status" CHECK ("Status" IN ('Pending', 'Accepted', 'Declined', 'Removed'))
);

CREATE UNIQUE INDEX "IX_EventStaff_EventId_UserId" ON "EventStaff" ("EventId", "UserId");
CREATE INDEX "IX_EventStaff_UserId" ON "EventStaff" ("UserId");

-- ============================================
-- MÓDULO: AUDITORÍA
-- ============================================

CREATE TABLE "AuditLogs" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UserId" UUID,
    "Action" VARCHAR(50) NOT NULL,
    "EntityType" VARCHAR(50) NOT NULL,
    "EntityId" UUID NOT NULL,
    "OldValues" JSONB,
    "NewValues" JSONB,
    "IpAddress" VARCHAR(45) NOT NULL,
    "UserAgent" VARCHAR(500),
    "CorrelationId" VARCHAR(100),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_AuditLogs_EntityType_EntityId" ON "AuditLogs" ("EntityType", "EntityId", "CreatedAt");
CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId", "CreatedAt");
CREATE INDEX "IX_AuditLogs_Action" ON "AuditLogs" ("Action", "CreatedAt");
CREATE INDEX "IX_AuditLogs_CreatedAt" ON "AuditLogs" ("CreatedAt" DESC);

-- ============================================
-- MÓDULO: LOGS DEL SISTEMA
-- ============================================

CREATE TABLE "SystemLogs" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Level" VARCHAR(10) NOT NULL,
    "Source" VARCHAR(200) NOT NULL,
    "Message" TEXT NOT NULL,
    "Exception" TEXT,
    "RequestPath" VARCHAR(500),
    "RequestMethod" VARCHAR(10),
    "StatusCode" INTEGER,
    "IpAddress" VARCHAR(45),
    "CorrelationId" VARCHAR(100),
    "AdditionalData" JSONB,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_SystemLogs" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_SystemLogs_Level" ON "SystemLogs" ("Level", "CreatedAt");
CREATE INDEX "IX_SystemLogs_RequestPath" ON "SystemLogs" ("RequestPath", "CreatedAt");
CREATE INDEX "IX_SystemLogs_CreatedAt" ON "SystemLogs" ("CreatedAt" DESC);
CREATE INDEX "IX_SystemLogs_CorrelationId" ON "SystemLogs" ("CorrelationId");

-- ============================================
-- SEED DATA: ROLES INICIALES
-- ============================================

INSERT INTO "Roles" ("Id", "Name", "Description") VALUES
    ('a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'Admin', 'Control total del sistema'),
    ('b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'Organizer', 'Crea y gestiona sus propios eventos'),
    ('c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Staff', 'Check-in y validación de QR en eventos asignados'),
    ('d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a', 'Customer', 'Compra boletos y gestiona su perfil');

-- ============================================
-- SEED DATA: CATEGORÍAS INICIALES
-- ============================================

INSERT INTO "Categories" ("Id", "Name", "Slug", "Description", "IconName") VALUES
    ('e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b', 'Música', 'musica', 'Conciertos y festivales musicales', 'music'),
    ('f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c', 'Deportes', 'deportes', 'Eventos deportivos y competencias', 'trophy'),
    ('a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d', 'Tecnología', 'tecnologia', 'Conferencias tech, hackathones y workshops', 'code'),
    ('b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e', 'Arte y Cultura', 'arte-y-cultura', 'Teatro, museos y exposiciones', 'palette'),
    ('c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f', 'Negocios', 'negocios', 'Networking y conferencias empresariales', 'briefcase'),
    ('d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a', 'Gastronomía', 'gastronomia', 'Ferias y festivales gastronómicos', 'utensils');

-- ============================================
-- TRIGGER: Actualizar UpdatedAt automáticamente
-- ============================================

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated_at BEFORE UPDATE ON "Users" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_events_updated_at BEFORE UPDATE ON "Events" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_venues_updated_at BEFORE UPDATE ON "Venues" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_ticket_types_updated_at BEFORE UPDATE ON "TicketTypes" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_discount_codes_updated_at BEFORE UPDATE ON "DiscountCodes" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_orders_updated_at BEFORE UPDATE ON "Orders" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_tickets_updated_at BEFORE UPDATE ON "Tickets" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER trg_event_staff_updated_at BEFORE UPDATE ON "EventStaff" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();