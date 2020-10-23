-- =============================================
-- Author:		<Adnan K>
-- Create date: <11/12/2019>
-- Description:	Function to Format the Physician Assign History Initials String - Replacment of the javascript code function formatPhysicianInitials
-- =============================================
CREATE FUNCTION [dbo].[FormatPhysiciansInitial]
(
	@phystringString  varchar(MAX)
)
RETURNS varchar(MAX)
AS
BEGIN
	
DECLARE    
    @id   INT,
	 @val VARCHAR(5);
 
DECLARE @PreviousPhy varchar(5);
DECLARE @Result as varchar(max)
DECLARE cursor_history CURSOR
FOR SELECT 
        id, 
        val
    FROM 
       dbo.SplitData(@phystringString, '/');
 
OPEN cursor_history;

 
FETCH NEXT FROM cursor_history INTO 
    @id, 
    @val;
 
WHILE @@FETCH_STATUS = 0
    BEGIN
        --PRINT @val; 
		IF (@val <> @PreviousPhy) BEGIN
		SET @Result = CONCAT(@Result, @val, '/');		
		END
		SET @PreviousPhy = @val;
        FETCH NEXT FROM cursor_history INTO 
            @id, 
            @val;
    END;

	
 
CLOSE cursor_history;
 
DEALLOCATE cursor_history;

 IF LEN(@Result) > 0  
      RETURN LEFT(@Result, LEN(@Result) - 1) 
  
Return @Result;

END


