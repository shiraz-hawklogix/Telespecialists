

-->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

CREATE PROCEDURE sp_case_trigger
	
AS
BEGIN
	
	SET NOCOUNT ON;
	select COUNT(cas_key) as records from case_trigger
END


-->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>