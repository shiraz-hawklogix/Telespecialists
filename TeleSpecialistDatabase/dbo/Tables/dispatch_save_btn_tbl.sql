CREATE TABLE [dbo].[dispatch_save_btn_tbl] (
    [cas_key]               INT              NULL,
    [cas_physician_key_old] UNIQUEIDENTIFIER NULL,
    [cas_physician_key_new] UNIQUEIDENTIFIER NULL,
    [row_status]            BIT              NULL
);


GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[trigger_for_dispatch]
   ON  [dbo].[dispatch_save_btn_tbl]
   AFTER insert, update,delete
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	INSERT INTO [dbo].[dispatch_trigger_tbl](
        trgr_caskey, 
        trgr_rowstatus,
        trgr_operation
    )
    SELECT
        i.cas_key,
        i.row_status,
		'INS'
    FROM
        inserted i
    UNION ALL
    SELECT
        d.cas_key,
        d.row_status, 
		'DEL'
    FROM
        deleted d;

END
