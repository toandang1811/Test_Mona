IF EXISTS (SELECT TOP 1 1 FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[DBO].[Import_data]') AND  OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [DBO].[Import_data]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

-- Import data use xml
CREATE PROCEDURE Import_data
(
	@Data XML
)
AS
BEGIN
	CREATE TABLE #Data
	(
		EmployeeName VARCHAR(50),
		DayOfBirth DATETIME
	)

	IF @Data IS NOT NULL
	BEGIN
		
		INSERT INTO #Data
				(
					EmployeeName,
					DayOfBirth
				)
		SELECT	X.Data.query('EmployeeName').value('.','VARCHAR(50)') AS TransactionID,
				CASE WHEN ISNULL(X.Data.query('DayOfBirth').value('.', 'VARCHAR(20)'), '') = '' THEN NULL ELSE CONVERT(DATETIME,X.Data.query('DayOfBirth').value('.', 'VARCHAR(20)'), 103) END AS DayOfBirth
		FROM @Data.nodes('//Data') AS X (Data)

	END

	BEGIN TRANSACTION
	BEGIN TRY
		DECLARE @key INT = (SELECT LastKey FROM Keys WHERE TableName = 'Employees'),
				@EmployeeName NVARCHAR(50),
				@DayOfBirth DATETIME

		DECLARE cur CURSOR FOR
		SELECT * 
		FROM #Data

		OPEN cur
		FETCH NEXT FROM cur INTO @EmployeeName, @DayOfBirth

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @key = @key + 1

			INSERT INTO Employees(EmployeeID, EmployeeName, DayOfBirth)
			VALUES (CONCAT('NV_', @key), @EmployeeName, @DayOfBirth)

			FETCH NEXT FROM cur INTO @EmployeeName, @DayOfBirth
		END

		UPDATE Keys 
		SET LastKey = @key 
		WHERE TableName = 'Employees'

		CLOSE cur
		DEALLOCATE cur
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0  
			ROLLBACK TRANSACTION;
	END CATCH

	IF @@TRANCOUNT > 0  
		COMMIT TRANSACTION; 
END

--exec Import_data @Data = '<Data><EmployeeName>Nguyen Van A</EmployeeName><DayOfBirth>2021-06-12</DayOfBirth></Data>'
