IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000000_CreateIdentitySchema', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [AspNetRoleClaims];
GO

DROP TABLE [AspNetUserClaims];
GO

DROP TABLE [AspNetUserLogins];
GO

DROP TABLE [AspNetUserRoles];
GO

DROP TABLE [AspNetUserTokens];
GO

DROP TABLE [AspNetRoles];
GO

DROP TABLE [AspNetUsers];
GO

CREATE TABLE [JobTitles] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_JobTitles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [HireDate] datetime2 NOT NULL,
    [DepartmentId] int NULL,
    [JobTitleId] int NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_JobTitles_JobTitleId] FOREIGN KEY ([JobTitleId]) REFERENCES [JobTitles] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [LeaveRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [Reason] nvarchar(max) NULL,
    CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Payrolls] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [PayDate] datetime2 NOT NULL,
    [BaseSalary] decimal(18,2) NOT NULL,
    [Bonus] decimal(18,2) NOT NULL,
    [Deductions] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Payrolls] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payrolls_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [PerformanceReviews] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [ReviewDate] datetime2 NOT NULL,
    [Rating] int NOT NULL,
    [Comments] nvarchar(max) NULL,
    [Reviewer] nvarchar(max) NULL,
    CONSTRAINT [PK_PerformanceReviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerformanceReviews_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Employees_JobTitleId] ON [Employees] ([JobTitleId]);
GO

CREATE INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests] ([EmployeeId]);
GO

CREATE INDEX [IX_Payrolls_EmployeeId] ON [Payrolls] ([EmployeeId]);
GO

CREATE INDEX [IX_PerformanceReviews_EmployeeId] ON [PerformanceReviews] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250511103901_InitialCreate', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250511105310_users', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [MonthlyAttendanceSummaries] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [DaysAbsent] int NOT NULL,
    [LateArrivals] int NOT NULL,
    [TotalDeduction] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_MonthlyAttendanceSummaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MonthlyAttendanceSummaries_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_MonthlyAttendanceSummaries_EmployeeId] ON [MonthlyAttendanceSummaries] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250513102329_summuries', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Employees] DROP COLUMN [FirstName];
GO

EXEC sp_rename N'[Employees].[LastName]', N'FullName', N'COLUMN';
GO

ALTER TABLE [Employees] ADD [MonthlySalary] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250513144242_fullName', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PayrollAdjustmentRules] (
    [Id] int NOT NULL IDENTITY,
    [JobTitleId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [IsPercentage] bit NOT NULL,
    CONSTRAINT [PK_PayrollAdjustmentRules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PayrollAdjustmentRules_JobTitles_JobTitleId] FOREIGN KEY ([JobTitleId]) REFERENCES [JobTitles] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PayrollAdjustmentRules_JobTitleId] ON [PayrollAdjustmentRules] ([JobTitleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250610131707_payrollrules', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PayrollAdjustmentRules] DROP CONSTRAINT [FK_PayrollAdjustmentRules_JobTitles_JobTitleId];
GO

DROP INDEX [IX_PayrollAdjustmentRules_JobTitleId] ON [PayrollAdjustmentRules];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PayrollAdjustmentRules]') AND [c].[name] = N'JobTitleId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PayrollAdjustmentRules] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PayrollAdjustmentRules] DROP COLUMN [JobTitleId];
GO

CREATE TABLE [JobTitlePayrollAdjustmentRule] (
    [JobTitlesId] int NOT NULL,
    [PayrollAdjustmentRulesId] int NOT NULL,
    CONSTRAINT [PK_JobTitlePayrollAdjustmentRule] PRIMARY KEY ([JobTitlesId], [PayrollAdjustmentRulesId]),
    CONSTRAINT [FK_JobTitlePayrollAdjustmentRule_JobTitles_JobTitlesId] FOREIGN KEY ([JobTitlesId]) REFERENCES [JobTitles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JobTitlePayrollAdjustmentRule_PayrollAdjustmentRules_PayrollAdjustmentRulesId] FOREIGN KEY ([PayrollAdjustmentRulesId]) REFERENCES [PayrollAdjustmentRules] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_JobTitlePayrollAdjustmentRule_PayrollAdjustmentRulesId] ON [JobTitlePayrollAdjustmentRule] ([PayrollAdjustmentRulesId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250612084320_AdjustmentRules', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PayrollAppliedRules] (
    [Id] int NOT NULL IDENTITY,
    [PayrollId] int NOT NULL,
    [RuleId] int NOT NULL,
    CONSTRAINT [PK_PayrollAppliedRules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PayrollAppliedRules_PayrollAdjustmentRules_RuleId] FOREIGN KEY ([RuleId]) REFERENCES [PayrollAdjustmentRules] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PayrollAppliedRules_Payrolls_PayrollId] FOREIGN KEY ([PayrollId]) REFERENCES [Payrolls] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PayrollAppliedRules_PayrollId] ON [PayrollAppliedRules] ([PayrollId]);
GO

CREATE INDEX [IX_PayrollAppliedRules_RuleId] ON [PayrollAppliedRules] ([RuleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250615100738_PayrollappliedRules', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Payrolls] ADD [AbsenceDays] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Payrolls] ADD [AbsenceDeduction] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [JobTitles] ADD [BaseSalary] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [AbsenceRecords] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    CONSTRAINT [PK_AbsenceRecords] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250617101933_AbsenceRecords', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LeaveRequests]') AND [c].[name] = N'Reason');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [LeaveRequests] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [LeaveRequests] ALTER COLUMN [Reason] nvarchar(500) NULL;
GO

ALTER TABLE [LeaveRequests] ADD [IsPaid] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [LeavePolicies] (
    [Id] int NOT NULL IDENTITY,
    [AnnualLeaveDays] int NOT NULL,
    CONSTRAINT [PK_LeavePolicies] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [LeavePolicyJobTitles] (
    [JobTitlesId] int NOT NULL,
    [LeavePoliciesId] int NOT NULL,
    CONSTRAINT [PK_LeavePolicyJobTitles] PRIMARY KEY ([JobTitlesId], [LeavePoliciesId]),
    CONSTRAINT [FK_LeavePolicyJobTitles_JobTitles_JobTitlesId] FOREIGN KEY ([JobTitlesId]) REFERENCES [JobTitles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeavePolicyJobTitles_LeavePolicies_LeavePoliciesId] FOREIGN KEY ([LeavePoliciesId]) REFERENCES [LeavePolicies] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_LeavePolicyJobTitles_LeavePoliciesId] ON [LeavePolicyJobTitles] ([LeavePoliciesId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250618094331_leave_requestChanges', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AbsenceRecords] ADD [Counted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [AbsenceRecords] ADD [Reason] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250621133637_absence-modif', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AbsenceRecords] ADD [DeductionValue] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250621140825_deductionValue', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AbsenceRecords]') AND [c].[name] = N'DeductionValue');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AbsenceRecords] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [AbsenceRecords] DROP COLUMN [DeductionValue];
GO

ALTER TABLE [AbsenceRecords] ADD [PayrollId] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250623103837_AbsencePayroll', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_AbsenceRecords_PayrollId] ON [AbsenceRecords] ([PayrollId]);
GO

ALTER TABLE [AbsenceRecords] ADD CONSTRAINT [FK_AbsenceRecords_Payrolls_PayrollId] FOREIGN KEY ([PayrollId]) REFERENCES [Payrolls] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250623103947_relationPayrollAbsence', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Payrolls] ADD [DeductionPerAbsenceDay] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Payrolls] ADD [ManualAbsenceDays] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250625101547_absencesDetailsInPayroll', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'HireDate');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Employees] ALTER COLUMN [HireDate] datetime2 NULL;
GO

ALTER TABLE [Employees] ADD [ConsumedLeaveDaysThisYear] real NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250626085943_nullable_hireDate', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[Employees].[ConsumedLeaveDaysThisYear]', N'InitialRemainingDays', N'COLUMN';
GO

ALTER TABLE [Employees] ADD [InscriptionDate] datetime2 NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250626102638_InitialRemainingDays', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Employees] DROP CONSTRAINT [FK_Employees_JobTitles_JobTitleId];
GO

DROP TABLE [LeavePolicyJobTitles];
GO

ALTER TABLE [JobTitles] ADD [LeavePolicyId] int NULL;
GO

CREATE INDEX [IX_JobTitles_LeavePolicyId] ON [JobTitles] ([LeavePolicyId]);
GO

ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_JobTitles_JobTitleId] FOREIGN KEY ([JobTitleId]) REFERENCES [JobTitles] ([Id]);
GO

ALTER TABLE [JobTitles] ADD CONSTRAINT [FK_JobTitles_LeavePolicies_LeavePolicyId] FOREIGN KEY ([LeavePolicyId]) REFERENCES [LeavePolicies] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250630083947_jobTitle_one_leave', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PayrollAppliedRules] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [PayrollAppliedRules] ADD [Notes] nvarchar(max) NULL;
GO

ALTER TABLE [PayrollAppliedRules] ADD [Quantity] int NULL;
GO

ALTER TABLE [PayrollAdjustmentRules] ADD [IsCountable] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250630093617_countableRule', N'8.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PayrollAdjustmentRules] ADD [IsEditable] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250630143602_isEditable', N'8.0.13');
GO

COMMIT;
GO

