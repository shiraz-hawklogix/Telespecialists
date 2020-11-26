Create PROCEDURE [dbo].[UspGetAllCaseDataForCCI]
AS
BEGIN
select 
cas_phy_key,
CONVERT(nvarchar(100) ,cas_fac_key) as cas_fac_key,
cas_metric_video_start_time,
cas_metric_video_end_time
FROM [dbo].[case]
where     cas_ctp_key =9 and cas_cst_key = 20 and cas_is_active = 1 
and   (cas_metric_video_end_time IS NOT NULL) and   (cas_metric_video_start_time IS NOT NULL)
end