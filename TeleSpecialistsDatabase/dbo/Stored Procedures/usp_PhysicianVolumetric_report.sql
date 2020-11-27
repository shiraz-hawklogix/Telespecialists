CREATE PROCEDURE [dbo].[usp_PhysicianVolumetric_report]
@querysdate datetime,
@queryedate datetime,
@Physicians nvarchar(MAX),
@Facilities nvarchar(MAX),
@DefaultType nvarchar(100)
AS
BEGIN
SELECT 
    
    CASE WHEN ([Extent2].[FirstName] IS NULL) THEN N'' ELSE [Extent2].[FirstName] END + N' ' + CASE WHEN ([Extent2].[LastName] IS NULL) THEN N'' ELSE [Extent2].[LastName] END AS PhysicianName, 
    [Extent1].[cas_billing_physician_blast] AS cas_billing_physician_blast, 
    [Extent1].[cas_fac_key] AS cas_fac_key, 
    [Extent1].[cas_phy_key] AS cas_phy_key, 
	[Extent1].[cas_ctp_key] AS cas_ctp_key, 
    [Extent1].[cas_billing_bic_key] AS cas_billing_bic_key, 
    CASE WHEN (@DefaultType = 'casetype') THEN CASE WHEN ([Extent1].[cas_ctp_key] IN (9,10)) THEN [Extent1].[cas_response_ts_notification] ELSE [Extent1].[cas_billing_date_of_consult] END WHEN ([Extent1].[cas_billing_bic_key] IN (1,2)) THEN [Extent1].[cas_response_ts_notification] ELSE [Extent1].[cas_billing_date_of_consult] END AS qeury_datetime
    FROM  [dbo].[case] AS [Extent1]
    LEFT OUTER JOIN [dbo].[AspNetUsers] AS [Extent2] ON [Extent1].[cas_phy_key] = [Extent2].[Id]
    WHERE (1 = [Extent1].[cas_is_active]) AND (20 = [Extent1].[cas_cst_key]) AND ([Extent1].[cas_created_date] >= @querysdate) AND ([Extent1].[cas_created_date] <= @queryedate) AND ([Extent1].[cas_phy_key] IN (select val FROM dbo.SplitData(@Physicians,','))) AND ([Extent1].[cas_fac_key] IN (select val FROM dbo.SplitData(@Facilities,',')))
END

