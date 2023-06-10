
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.BlacklistedMobs', N'U') IS NULL
BEGIN
CREATE TABLE[dbo].[BlacklistedMobs](
	[Guid] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.Commands', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Commands](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Command] [nvarchar](max) NOT NULL,
	[Player] [nvarchar](max) NOT NULL,
	[Args] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[Hotspots]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.Hotspots', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Hotspots](
	[Zone] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Faction] [nvarchar](max) NOT NULL,
	[Waypoints] [nvarchar](max) NOT NULL,
	[InnkeeperId] [int] NULL,
	[RepairVendorId] [int] NULL,
	[AmmoVendorId] [int] NULL,
	[MinimumLevel] [int] NOT NULL,
	[TravelPathId] [int] NULL,
	[SafeForGrinding] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[HotspotsV2]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.HotspotsV2', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[HotspotsV2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Polyn] [nvarchar](max) NOT NULL,
	[Zone] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Faction] [nvarchar](max) NOT NULL,
	[MinimumLevel] [int] NOT NULL,
	[TravelPathId] [int] NULL,
	[TownId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[Kills]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.Kills', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Kills](
	[PlayerLevel] [int] NOT NULL,
	[PlayerName] [nvarchar](max) NOT NULL,
	[PlayerClass] [nvarchar](max) NOT NULL,
	[EnemyName] [nvarchar](max) NOT NULL,
	[EnemyLevel] [int] NOT NULL,
	[EnemyType] [nvarchar](max) NOT NULL,
	[HealthPercentLost] [int] NOT NULL,
	[CombatDuration] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[Npcs]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.Npcs', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Npcs](
	[Name] [nvarchar](max) NOT NULL,
	[IsInnkeeper] [bit] NOT NULL,
	[SellsAmmo] [bit] NOT NULL,
	[Repairs] [bit] NOT NULL,
	[Quest] [bit] NOT NULL,
	[Horde] [bit] NOT NULL,
	[Alliance] [bit] NOT NULL,
	[PositionX] [numeric](18, 0) NOT NULL,
	[PositionY] [numeric](18, 0) NOT NULL,
	[PositionZ] [numeric](18, 0) NOT NULL,
	[Zone] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[QuestCompletions]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.QuestCompletions', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[QuestCompletions](
	[PlayerGuid] [nvarchar](max) NOT NULL,
	[QuestId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[QuestHubs]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.QuestHubs', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[QuestHubs](
	[Name] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[QuestObjectives]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.QuestObjectives', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[QuestObjectives](
	[Type] [nvarchar](max) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Source] [nvarchar](max) NOT NULL,
	[Quantity] [int] NOT NULL,
	[QuestId] [int] NOT NULL,
	[ObjectId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[QuestPrerequisites]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.QuestPrerequisites', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[QuestPrerequisites](
	[ParentId] [int] NOT NULL,
	[ChildId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
END
/****** Object:  Table [dbo].[Quests]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.Quests', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Quests](
	[Name] [nvarchar](max) NOT NULL,
	[QuestHubId] [int] NOT NULL,
	[HotspotId] [int] NULL,
	[StartNpcId] [int] NOT NULL,
	[EndNpcId] [int] NOT NULL,
	[RequiredLevel] [int] NOT NULL,
	[Class] [nvarchar](max) NOT NULL,
	[QuestId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[ReportSignatures]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.ReportSignatures', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReportSignatures](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Player] [nvarchar](max) NOT NULL,
	[CommandId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[Towns]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.Towns', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Towns](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InnkeeperId] [int] NULL,
	[RepairVendorId] [int] NULL,
	[AmmoVendorId] [int] NULL,
	[Name] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
/****** Object:  Table [dbo].[TravelPaths]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON
IF OBJECT_ID(N'dbo.TravelPaths', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TravelPaths](
	[Name] [nvarchar](max) NOT NULL,
	[Waypoints] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
