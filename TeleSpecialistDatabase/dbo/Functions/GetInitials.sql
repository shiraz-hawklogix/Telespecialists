-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetInitials]
(
	-- Add the parameters for the function here
	@inputval VARCHAR(max)
)
RETURNS varchar(max)
AS
BEGIN

	DECLARE @result varchar(max) = ''
	if(@inputval is not null)
		Begin
			DECLARE @initials varchar(max) = UPPER(substring(@inputval,1,1))
			DECLARE @currentSpace int = charindex(' ',@inputval)
			SET @initials += UPPER(substring(@inputval, @currentSpace+1, 1))
			Set @result =  @initials
		End
	-- Return the result of the function
	RETURN REVERSE(@result)
END