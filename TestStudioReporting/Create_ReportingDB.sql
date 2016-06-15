CREATE TABLE [dbo].[ResultType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](32) NOT NULL
 CONSTRAINT [PK_ResultType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO [dbo].[ResultType] ([Id], [Name]) VALUES (0, 'Pass')
INSERT INTO [dbo].[ResultType] ([Id], [Name]) VALUES (1, 'Fail')
INSERT INTO [dbo].[ResultType] ([Id], [Name]) VALUES (2, 'NotRun')

CREATE TABLE [dbo].[BrowserType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](32) NOT NULL
 CONSTRAINT [PK_BrowserType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (0, 'NotSet')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (1, 'InternetExplorer')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (2, 'FireFox')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (3, 'AspNetHost')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (4, 'Designer')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (5, 'Safari')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (6, 'SilverlightOutOfBrowser')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (7, 'Chrome')
INSERT INTO [dbo].[BrowserType] ([Id], [Name]) VALUES (8, 'NativeApp')

CREATE TABLE [dbo].[RunResult](
	[Id] [uniqueidentifier] NOT NULL,
	[TestListId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](512) NULL,
	[FileName] [nvarchar](512) NULL,
	[Passed] [bit] NOT NULL,
	[Summary] [nvarchar](MAX) NULL,
	[Comment] [nvarchar](MAX) NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[IsManual] [bit] NOT NULL CONSTRAINT [DF_RunResult_IsManual]  DEFAULT ((0)),
	[AllCount] [int] NULL,
	[NotRunCount] [int] NULL,
	[PassedCount] [int] NULL,
	[FailedCount] [int] NULL,
 CONSTRAINT [PK_RunResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[TestResult](
	[Id] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](MAX) NULL,
	[Name] [nvarchar](512) NULL,
	[Path] [nvarchar](512) NULL,
	[TestId] [nvarchar](512) NULL,
	[Message] [nvarchar](MAX) NULL,
	[IsDataDriven] [bit] NOT NULL CONSTRAINT [DF_TestResult_IsDataDriven]  DEFAULT ((0)),
	[IsManual] [bit] NOT NULL CONSTRAINT [DF_TestResult_IsManual]  DEFAULT ((0)),
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[FailureException] [nvarchar](MAX) NULL,
	[FailedStepComment] [nvarchar](MAX) NULL,
	[AllCount] [int] NULL,
	[NotRunCount] [int] NULL,
	[PassedCount] [int] NULL,
	[BrowserTypeId] [int] NULL,
	[ResultTypeId] [int] NULL,
	[RunResultId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_TestResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[TestResult]  WITH CHECK ADD  CONSTRAINT [FK_TestResult_BrowserType] FOREIGN KEY([BrowserTypeId])
REFERENCES [dbo].[BrowserType] ([Id])
GO

ALTER TABLE [dbo].[TestResult] CHECK CONSTRAINT [FK_TestResult_BrowserType]
GO

ALTER TABLE [dbo].[TestResult]  WITH CHECK ADD  CONSTRAINT [FK_TestResult_ResultType] FOREIGN KEY([ResultTypeId])
REFERENCES [dbo].[ResultType] ([Id])
GO

ALTER TABLE [dbo].[TestResult] CHECK CONSTRAINT [FK_TestResult_ResultType]
GO

ALTER TABLE [dbo].[TestResult]  WITH CHECK ADD  CONSTRAINT [FK_TestResult_RunResult] FOREIGN KEY([RunResultId])
REFERENCES [dbo].[RunResult] ([Id])
GO

ALTER TABLE [dbo].[TestResult] CHECK CONSTRAINT [FK_TestResult_RunResult]
GO

CREATE TABLE [dbo].[StepResult](
	[Description] [nvarchar](MAX) NULL,
	[Comment] [nvarchar](MAX) NULL,
	[Order] [int] NOT NULL, 
	[WasEnabled] [bit] NOT NULL CONSTRAINT [DF_StepResult_WasEnabled]  DEFAULT ((0)),
	[IsManual] [bit] NOT NULL CONSTRAINT [DF_StepResult_IsManual]  DEFAULT ((0)),
	[FailureException] [nvarchar](MAX) NULL,
	[ResultTypeId] [int] NULL,
	[TestResultId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_StepResult] PRIMARY KEY CLUSTERED 
(
	[TestResultId],
	[Order] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StepResult]  WITH CHECK ADD  CONSTRAINT [FK_StepResult_ResultType] FOREIGN KEY([ResultTypeId])
REFERENCES [dbo].[ResultType] ([Id])
GO

ALTER TABLE [dbo].[StepResult] CHECK CONSTRAINT [FK_StepResult_ResultType]
GO

ALTER TABLE [dbo].[StepResult]  WITH CHECK ADD  CONSTRAINT [FK_StepResult_TestResult] FOREIGN KEY([TestResultId])
REFERENCES [dbo].[TestResult] ([Id])
GO

ALTER TABLE [dbo].[StepResult] CHECK CONSTRAINT [FK_StepResult_TestResult]
GO

CREATE TABLE [dbo].[StepResultData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Screenshot] [varbinary](MAX) NULL,
	[DOM] [nvarchar](MAX) NULL,
	[TestResultId] [uniqueidentifier] NOT NULL,
	[StepOrder] [int] NOT NULL,
 CONSTRAINT [PK_StepResultData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StepResultData]  WITH CHECK ADD  CONSTRAINT [FK_StepResultData_StepResult] FOREIGN KEY([TestResultId],[StepOrder])
REFERENCES [dbo].[StepResult] ([TestResultId],[Order])
GO

ALTER TABLE [dbo].[StepResultData] CHECK CONSTRAINT [FK_StepResultData_StepResult]
GO

-- CLEAN UP

--DELETE FROM [dbo].[StepResultData]
--DELETE FROM [dbo].[StepResult]
--DELETE FROM [dbo].[TestResult]
--DELETE FROM [dbo].[RunResult]