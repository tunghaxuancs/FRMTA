
USE [FRMTA]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 4/12/2017 1:57:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](50) NULL,
	[Address] [nvarchar](150) NULL,
	[Birthday] [date] NULL,
	[Mobile] [nchar](15) NULL,
	[Career] [nvarchar](100) NULL,
	[Status] [bit] NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FaceData]    Script Date: 4/12/2017 1:57:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FaceData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[FaceFolder] [nvarchar](max) NULL,
	[FaceImage] [nvarchar](50) NULL,
 CONSTRAINT [PK_FaceData] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[FaceData]  WITH CHECK ADD  CONSTRAINT [FK_FaceData_Customer] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customer] ([ID])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[FaceData] CHECK CONSTRAINT [FK_FaceData_Customer]
GO
USE [master]
GO
ALTER DATABASE [FRMTA] SET  READ_WRITE 
GO
