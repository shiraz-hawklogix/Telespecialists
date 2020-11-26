


-- ============================================= 
CREATE PROCEDURE [dbo].[usp_set_refresh_case] 
AS
BEGIN
	
UPDATE [dbo].[For_Case]
SET refresh_requried = 0
where fcs_key = 1
       
END
