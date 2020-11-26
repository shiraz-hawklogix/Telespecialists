-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[CheckPhysBlastOnShift]
(
	-- Add the parameters for the function here
	@BlastDateTime datetime,
	@PhyId varchar(max)
)
RETURNS varchar(50)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Resutl varchar(50) = 'No'

	Set  @Resutl = 
		(	Select  case when Count(*) > 0 then 'Yes' Else 'No' END   
			from 
				[dbo].[user_schedule]
			where 
				uss_time_from_calc <= @BlastDateTime and
				uss_time_to_calc >= @BlastDateTime and
				uss_user_id =  @PhyId
			)
	-- Return the result of the function
	RETURN @Resutl

END