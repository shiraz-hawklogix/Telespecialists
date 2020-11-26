

create PROCEDURE [dbo].[usp_case_read_status]
@cas_key int
AS
BEGIN

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

Select cas_cst_key FROM [case] 
WHERE cas_key = @cas_key

END