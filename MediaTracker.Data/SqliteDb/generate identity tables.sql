BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
	"Id"	INTEGER NOT NULL,
	"ClaimType"	TEXT,
	"ClaimValue"	TEXT,
	"RoleId"	TEXT NOT NULL,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
	"Id"	TEXT NOT NULL,
	"ConcurrencyStamp"	TEXT,
	"Name"	TEXT,
	"NormalizedName"	TEXT,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
	"Id"	INTEGER NOT NULL,
	"ClaimType"	TEXT,
	"ClaimValue"	TEXT,
	"UserId"	TEXT NOT NULL,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
	"LoginProvider"	TEXT NOT NULL,
	"ProviderKey"	TEXT NOT NULL,
	"ProviderDisplayName"	TEXT,
	"UserId"	TEXT NOT NULL,
	PRIMARY KEY("LoginProvider","ProviderKey")
);
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
	"UserId"	TEXT NOT NULL,
	"RoleId"	TEXT NOT NULL,
	PRIMARY KEY("UserId","RoleId")
);
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
	"UserId"	TEXT NOT NULL,
	"LoginProvider"	TEXT NOT NULL,
	"Name"	TEXT NOT NULL,
	"Value"	TEXT,
	PRIMARY KEY("UserId","LoginProvider","Name")
);
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
	"Id"	TEXT NOT NULL,
	"AccessFailedCount"	INTEGER NOT NULL,
	"ConcurrencyStamp"	TEXT,
	"Email"	TEXT,
	"EmailConfirmed"	INTEGER NOT NULL DEFAULT 0,
	"LockoutEnabled"	INTEGER NOT NULL DEFAULT 0,
	"LockoutEnd"	TEXT,
	"NormalizedEmail"	TEXT,
	"NormalizedUserName"	TEXT,
	"PasswordHash"	TEXT,
	"PhoneNumber"	TEXT,
	"PhoneNumberConfirmed"	INTEGER NOT NULL DEFAULT 0,
	"SecurityStamp"	TEXT,
	"TwoFactorEnabled"	INTEGER NOT NULL DEFAULT 0,
	"UserName"	TEXT,
	"ProfilePicturePath"	TEXT,
	"IsActive"	INTEGER NOT NULL DEFAULT 0,
	"HasAgreedWithTerms"	INTEGER NOT NULL DEFAULT 0,
	PRIMARY KEY("Id")
);
COMMIT;
