

CREATE PROCEDURE [dbo].[sp_dispatch_save_btn_clicked]
	@cas_key INT = NULL
AS
BEGIN
	
	UPDATE [dbo].[dispatch_save_btn_tbl]
	SET row_status = 0
    WHERE [dbo].[dispatch_save_btn_tbl].cas_key  = @cas_key
END
