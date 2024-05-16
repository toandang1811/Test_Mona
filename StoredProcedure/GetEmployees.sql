IF EXISTS (SELECT TOP 1 1 FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[DBO].[GetEmployees]') AND  OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [DBO].[GetEmployees]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE GetEmployees
(
	@pageSize INT,
	@pageNumber INT
)
AS
BEGIN
	SELECT Employees.*, YEAR(GETDATE()) - YEAR(Employees.DayOfBirth) AS YearsOld
	FROM Employees
	Order BY EmployeeID
	OFFSET (@PageNumber-1) * @PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY
END
--exec GetEmployess @pageNumber = 1, @pageSize = 25
