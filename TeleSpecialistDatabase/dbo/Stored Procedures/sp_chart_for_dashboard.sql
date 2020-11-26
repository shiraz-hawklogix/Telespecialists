
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_chart_for_dashboard]
	-- Add the parameters for the stored procedure here
	@UserId		NVARCHAR(128) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SET NOCOUNT ON;

    -- Select statements for procedure here
	SELECT 
				ChartType = 'Cases',
				ChartTitle = 'Cases',
				ChartStyle = 'column',
				ChartStack = 'false',
				
				Category = 'Legends', 
				Legends = ctp_name, 
				CaseCount = COUNT(*), 
				Fields = 'CaseCount',

				ChartLabelTemplate = ''

	FROM [case]
	INNER JOIN case_type ON ctp_key = cas_ctp_key

	GROUP BY ctp_name

END