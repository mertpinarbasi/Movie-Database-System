CREATE TABLE [dbo].[Person]
(
	[UserId] INT NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(20) NOT NULL, 
    [Surname] NVARCHAR(30) NOT NULL, 
    [Username] NVARCHAR(20) NOT NULL, 
    [Password] NVARCHAR(20) NOT NULL
)

INSERT INTO Person([UserId],[Name],[Surname],[Username],[Password])
VALUES
(1, 'Eda İrem', 'Deniz', 'iremdnz26', 'dnzirem123'),
(2, 'Mert', 'Pınarbaşı', 'mert41', 'pnbmert65')
GO
