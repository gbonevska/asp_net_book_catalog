USE [HomeLibrary]
GO

/****** Object:  Table [dbo].[authors]    Script Date: 12-Jan-17 21:33:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[authors](
	[author_id] [int] IDENTITY(1,1) NOT NULL,
	[author_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_authors] PRIMARY KEY CLUSTERED 
(
	[author_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

USE [HomeLibrary]
GO

/****** Object:  Table [dbo].[collections]    Script Date: 12-Jan-17 21:34:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[collections](
	[collection_id] [int] IDENTITY(1,1) NOT NULL,
	[collection_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_collections] PRIMARY KEY CLUSTERED 
(
	[collection_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


USE [HomeLibrary]
GO

/****** Object:  Table [dbo].[books]    Script Date: 12-Jan-17 21:34:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[books](
	[book_id] [int] IDENTITY(1,1) NOT NULL,
	[book_title] [nvarchar](40) NOT NULL,
	[notes] [nvarchar](50) NULL,
 CONSTRAINT [PK_books] PRIMARY KEY CLUSTERED 
(
	[book_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [HomeLibrary]
GO

/****** Object:  Table [dbo].[books_authors]    Script Date: 12-Jan-17 21:34:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[books_authors](
	[book_id] [int] NOT NULL,
	[author_id] [int] NOT NULL,
	[collection_id] [int] NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[books_authors]  WITH CHECK ADD FOREIGN KEY([author_id])
REFERENCES [dbo].[authors] ([author_id])
GO

ALTER TABLE [dbo].[books_authors]  WITH CHECK ADD FOREIGN KEY([author_id])
REFERENCES [dbo].[authors] ([author_id])
GO

ALTER TABLE [dbo].[books_authors]  WITH CHECK ADD FOREIGN KEY([book_id])
REFERENCES [dbo].[books] ([book_id])
GO

ALTER TABLE [dbo].[books_authors]  WITH CHECK ADD FOREIGN KEY([book_id])
REFERENCES [dbo].[books] ([book_id])
GO

ALTER TABLE [dbo].[books_authors]  WITH CHECK ADD FOREIGN KEY([collection_id])
REFERENCES [dbo].[collections] ([collection_id])
GO

