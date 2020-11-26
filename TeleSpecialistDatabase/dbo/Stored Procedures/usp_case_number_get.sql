-- =============================================
-- Author:		Adnan K
-- Created date: April 1, 2020	
-- Description:	Purpose of the store procedure is Generate Case Number, moved this code to store procedure to include the 
-- uncommitted transaction for the case number

-- =============================================

CREATE PROCEDURE [dbo].[usp_case_number_get]
AS
BEGIN

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

Select (ISNULL(Max(cas_case_number),99999999) + 1) as value FROM [case] 
WHERE cas_case_number is not null

-- SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

END