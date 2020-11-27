
-- ============================================= 
CREATE PROCEDURE [dbo].[usp_dispatch_latest_case] 
AS
BEGIN
	SET NOCOUNT ON;
Select  Top 1 cas_key 
        FROM     [dbo].[case] 
        WHERE 
		cas_is_active = 1 	
		AND cas_cst_key = 18
		AND cas_ctp_key = 9
					
		ORDER BY cas_key DESC 
END
