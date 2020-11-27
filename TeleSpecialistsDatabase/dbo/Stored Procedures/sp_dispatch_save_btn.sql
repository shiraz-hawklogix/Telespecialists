

CREATE PROCEDURE [dbo].[sp_dispatch_save_btn]
	
AS
BEGIN
	
	SET NOCOUNT ON;
	select cas_key, row_status from dispatch_save_btn_tbl
END
