/****** Object:  Table [dbo].[BlacklistedMobs]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlacklistedMobs](
	[Guid] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Commands]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commands](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Command] [nvarchar](max) NOT NULL,
	[Player] [nvarchar](max) NOT NULL,
	[Args] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hotspots]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[HotspotsV2]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HotspotsV2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Polygon] [nvarchar](max) NOT NULL,
	[Zone] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Faction] [nvarchar](max) NOT NULL,
	[MinimumLevel] [int] NOT NULL,
	[TravelPathId] [int] NULL,
	[TownId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Kills]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[Npcs]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[QuestCompletions]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestCompletions](
	[PlayerGuid] [nvarchar](max) NOT NULL,
	[QuestId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestHubs]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestHubs](
	[Name] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestObjectives]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestObjectives](
	[Type] [nvarchar](max) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Source] [nvarchar](max) NOT NULL,
	[Quantity] [int] NOT NULL,
	[QuestId] [int] NOT NULL,
	[ObjectId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestPrerequisites]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestPrerequisites](
	[ParentId] [int] NOT NULL,
	[ChildId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quests]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[ReportSignatures]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportSignatures](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Player] [nvarchar](max) NOT NULL,
	[CommandId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Towns]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Towns](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InnkeeperId] [int] NULL,
	[RepairVendorId] [int] NULL,
	[AmmoVendorId] [int] NULL,
	[Name] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TravelPaths]    Script Date: 12/3/2022 9:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TravelPaths](
	[Name] [nvarchar](max) NOT NULL,
	[Waypoints] [nvarchar](max) NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
