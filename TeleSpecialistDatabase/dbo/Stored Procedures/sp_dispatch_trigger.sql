
-->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

CREATE PROCEDURE [dbo].[sp_dispatch_trigger]
	
AS
BEGIN
	
	SET NOCOUNT ON;
	select COUNT(trgr_caskey) as records from dispatch_trigger_tbl
END