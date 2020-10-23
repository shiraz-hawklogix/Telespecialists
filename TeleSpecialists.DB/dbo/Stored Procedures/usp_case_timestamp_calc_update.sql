-- =============================================
-- Author:		<Adnan K>
-- Create date: <2020-March-02>
-- Description:	<Update TimeStamps table after case table is updated>
-- =============================================
CREATE PROCEDURE [dbo].[usp_case_timestamp_calc_update] 
	-- Add the parameters for the stored procedure here	
	@cas_key varchar(max)	
AS
BEGIN
 
	DECLARE @TotalRows INT;

	SET NOCOUNT ON;

	
	SELECT 
    [Extent1].[cas_key] AS [cas_key],     
	Extent1.cas_created_by,
	Extent1.cas_created_date,
	Extent1.cas_modified_by,
	Extent1.cas_modified_date,

	-- case history table fields ---
	[Limit2].[cah_created_date] as cts_waiting_to_accept_est,
 [Limit2].[cah_created_date_utc] as cts_waiting_to_accept_utc,
 [Limit1].[cah_created_date] as cts_accepted_est,
 [Limit1].[cah_created_date_utc] as cts_accepted_utc,
 [dbo].[DiffSeconds]([Extent1].[cas_response_ts_notification], [Extent1].[cas_metric_stamp_time]) AS cts_start_to_stamp_sec, -- handle_time     
 [dbo].[DiffSeconds]([Limit2].[cah_created_date_utc], [Limit1].[cah_created_date_utc]) AS cts_waiting_to_accept_accept_sec, -- assignment_time_cmp 

    
 CASE WHEN (([Extent1].[cas_response_ts_notification] IS NOT NULL) AND ([Limit1].[cah_created_date_utc] IS NOT NULL)) THEN dbo.DiffSeconds([Extent1].[cas_response_ts_notification], [Limit1].[cah_created_date_utc])  ELSE 0 END AS cts_start_to_accept_sec, -- start_accepted_cmp  
 -- end of case history fields ---
    
    
    CASE WHEN ([Extent1].[cas_response_first_atempt] < [Extent1].[cas_metric_stamp_time]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_stamp_time], [Extent1].[cas_response_first_atempt]) END AS cts_bedside_response_sec, -- bedside_response_time_cmp
    
     CASE WHEN ([Extent1].[cas_metric_video_start_time] < [Extent1].[cas_response_first_atempt]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_video_start_time], [Extent1].[cas_response_first_atempt]) END AS cts_login_to_handle_sec, -- login_handletime_cmp 
    --CASE WHEN ([Extent1].[cas_metric_video_end_time] < [Extent1].[cas_metric_video_start_time]) THEN N'00:00:00' ELSE [dbo].[FormatSeconds_v2]([Extent1].[cas_metric_video_end_time], [Extent1].[cas_metric_video_start_time]) END AS [C35], 
    CASE WHEN ([Extent1].[cas_metric_video_end_time] < [Extent1].[cas_metric_video_start_time]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_video_end_time], [Extent1].[cas_metric_video_start_time]) END AS cts_on_screen_sec,  -- on_screen_time_cmp

    --CASE WHEN ([Extent1].[cas_metric_door_time] < [Extent1].[cas_response_ts_notification]) THEN N'00:00:00' ELSE [dbo].[FormatSeconds_v2]([Extent1].[cas_metric_door_time], [Extent1].[cas_response_ts_notification]) END AS [C37],  -- activation_time
    CASE WHEN ([Extent1].[cas_metric_door_time] < [Extent1].[cas_response_ts_notification]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_door_time], [Extent1].[cas_response_ts_notification]) END AS cts_arrival_to_start_sec, 

	CASE WHEN ([Extent1].[cas_metric_door_time] < [Extent1].cas_metric_stamp_time) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_door_time], [Extent1].[cas_metric_stamp_time]) END AS cts_arrival_to_stamp_sec, 
    
    CASE WHEN ([Extent1].[cas_metric_needle_time] < [Extent1].[cas_metric_door_time]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_needle_time], [Extent1].[cas_metric_door_time]) END AS cts_arrival_to_needle_sec,  -- arrival_needle_time_cmp

	CASE WHEN Extent1.cas_metric_symptom_onset_during_ed_stay = 1 AND Extent1.cas_metric_symptom_onset_during_ed_stay_time IS NOT NULL THEN 
	CASE WHEN ([Extent1].[cas_metric_needle_time] < [Extent1].[cas_metric_symptom_onset_during_ed_stay_time]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_needle_time], [Extent1].[cas_metric_symptom_onset_during_ed_stay_time]) END

	ELSE 0 END as cts_symptom_to_needle_sec, 

    --CASE WHEN ([Extent1].[cas_metric_tpa_verbal_order_time] < [Extent1].[cas_metric_video_start_time]) THEN N'00:00:00' ELSE [dbo].[FormatSeconds_v2]([Extent1].[cas_metric_tpa_verbal_order_time], [Extent1].[cas_metric_video_start_time]) END AS [C43],  -- physician_MDM
    CASE WHEN ([Extent1].[cas_metric_tpa_verbal_order_time] < [Extent1].[cas_metric_video_start_time]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_metric_tpa_verbal_order_time], [Extent1].[cas_metric_video_start_time]) END AS cts_physician_mdm_sec, 

    CASE WHEN ([Extent1].[cas_response_first_atempt] < [Extent1].[cas_response_ts_notification]) THEN 0 ELSE [dbo].[DiffSeconds]([Extent1].[cas_response_first_atempt], [Extent1].[cas_response_ts_notification]) END AS cts_response_sec,  -- order_to_needle_cmp

	[dbo].[DiffSeconds]([Extent1].[cas_response_ts_notification] , [Extent1].[cas_callback_response_time]) AS cts_callback_to_tsnotification,

    [dbo].[DiffSeconds]([Extent1].[cas_metric_needle_time], [Extent1].[cas_metric_tpa_verbal_order_time]) AS cts_verbal_order_to_needle_sec,  -- order_to_needle_cmp


    [dbo].[DiffSeconds]([Extent1].[cas_metric_pa_ordertime], [Extent1].[cas_metric_tpa_verbal_order_time]) AS cts_verbal_order_to_cpoe_sec, -- verbal_order_to_ocopr_order_cmp


    [dbo].[DiffSeconds]([Extent1].[cas_metric_pa_ordertime], [Extent1].[cas_metric_needle_time]) AS cts_cpoe_order_to_needle,  -- cpoe_order_to_needle_cmp


    CASE WHEN ([Extent1].[cas_metric_needle_time_est] < [Extent1].[cas_metric_lastwell_date_est]) THEN 
	0
	ELSE 	
	[dbo].DiffSeconds([Extent1].[cas_metric_lastwell_date_est], [Extent1].[cas_metric_needle_time_est]) END AS cts_last_known_well_to_needle_sec
	    
	INTO #TempCaseCalc

    FROM        [dbo].[case] AS [Extent1]

	 OUTER APPLY  (SELECT TOP (1) [Project1].[cah_created_date_utc] AS [cah_created_date_utc], [Project1].[cah_created_date] 
        FROM ( SELECT 
            [Extent6].[cah_created_date] AS [cah_created_date], 
            [Extent6].[cah_created_date_utc] AS [cah_created_date_utc]
            FROM [dbo].[case_assign_history] AS [Extent6]
            WHERE ('Accepted' = [Extent6].[cah_action]) AND ([Extent1].[cas_key] = [Extent6].[cah_cas_key])
        )  AS [Project1]
        ORDER BY [Project1].[cah_created_date] DESC ) AS [Limit1]
    OUTER APPLY  (SELECT TOP (1) [Project2].[cah_created_date_utc] AS [cah_created_date_utc], [Project2].[cah_created_date] 
        FROM ( SELECT 
            [Extent7].[cah_created_date] AS [cah_created_date], 
            [Extent7].[cah_created_date_utc] AS [cah_created_date_utc]
            FROM [dbo].[case_assign_history] AS [Extent7]
            WHERE ('Waiting to Accept' = [Extent7].[cah_action]) AND ([Extent1].[cas_key] = [Extent7].[cah_cas_key])
        )  AS [Project2]
        ORDER BY [Project2].[cah_created_date] DESC ) AS [Limit2]


  
    WHERE (Extent1.cas_key IN (Select val from dbo.SplitData(@cas_key, ',')))	 
	



Select @TotalRows = Count(*) FROM case_timestamp Where cts_cas_key IN (Select val from dbo.SplitData(@cas_key, ','));

select @TotalRows;

IF (@TotalRows > 0) 
BEGIN 
Update case_timestamp
set 
--- case history table fields---
cts_waiting_to_accept_est = #TempCaseCalc.cts_waiting_to_accept_est,
cts_waiting_to_accept_utc = #TempCaseCalc.cts_waiting_to_accept_utc,
cts_accepted_est = #TempCaseCalc.cts_accepted_est,
cts_accepted_utc = #TempCaseCalc.cts_accepted_utc,
cts_waiting_to_accept_accept_sec = #TempCaseCalc.cts_waiting_to_accept_accept_sec,

--- case history table fields---

cts_arrival_to_needle_sec = #TempCaseCalc.cts_arrival_to_needle_sec,
cts_physician_mdm_sec = #TempCaseCalc.cts_physician_mdm_sec,
cts_response_sec = #TempCaseCalc.cts_response_sec,
cts_callback_to_tsnotification = #TempCaseCalc.cts_callback_to_tsnotification,
cts_symptom_to_needle_sec = #TempCaseCalc.cts_symptom_to_needle_sec,
cts_last_known_well_to_needle_sec = #TempCaseCalc.cts_last_known_well_to_needle_sec,
cts_bedside_response_sec = #TempCaseCalc.cts_bedside_response_sec,
cts_start_to_stamp_sec = #TempCaseCalc.cts_start_to_stamp_sec,
cts_start_to_accept_sec = #TempCaseCalc.cts_start_to_accept_sec,
cts_arrival_to_stamp_sec = #TempCaseCalc.cts_arrival_to_stamp_sec,
cts_arrival_to_start_sec = #TempCaseCalc.cts_arrival_to_start_sec,
cts_cpoe_order_to_needle = #TempCaseCalc.cts_cpoe_order_to_needle,
cts_login_to_handle_sec = #TempCaseCalc.cts_login_to_handle_sec,
cts_on_screen_sec = #TempCaseCalc.cts_on_screen_sec,
cts_verbal_order_to_cpoe_sec = #TempCaseCalc.cts_verbal_order_to_cpoe_sec,
cts_verbal_order_to_needle_sec = #TempCaseCalc.cts_verbal_order_to_needle_sec,
cts_modified_date = dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', GETUTCDATE()),
cts_modified_by = #TempCaseCalc.cas_modified_by

FROM case_timestamp as ct
INNER JOIN #TempCaseCalc on ct.cts_cas_key = #TempCaseCalc.cas_key
END
ELSE 
BEGIN
INSERT INTO case_timestamp (
cts_cas_key,
---- case hisotory table fields ---
cts_waiting_to_accept_est 					
,cts_waiting_to_accept_utc				
,cts_accepted_utc 				
,cts_waiting_to_accept_accept_sec 						
,cts_arrival_to_needle_sec 				
,cts_physician_mdm_sec
-- end of case history ---
			
,cts_response_sec
,cts_callback_to_tsnotification				
,cts_symptom_to_needle_sec 				
,cts_last_known_well_to_needle_sec 						
,cts_bedside_response_sec 				
,cts_start_to_stamp_sec 				
,cts_start_to_accept_sec 				
,cts_arrival_to_stamp_sec 				
,cts_arrival_to_start_sec 					
,cts_cpoe_order_to_needle 						
,cts_login_to_handle_sec 			
,cts_verbal_order_to_cpoe_sec 			
,cts_verbal_order_to_needle_sec 				
,cts_created_date
,cts_created_by
) 

SELECT 
#TempCaseCalc.cas_key as cts_cas_key,
---- case hisotory table fields ---
#TempCaseCalc.cts_waiting_to_accept_est 					
,#TempCaseCalc.cts_waiting_to_accept_utc				
,#TempCaseCalc.cts_accepted_utc 				
,#TempCaseCalc.cts_waiting_to_accept_accept_sec 						
,#TempCaseCalc.cts_arrival_to_needle_sec 				
,#TempCaseCalc.cts_physician_mdm_sec
-- end of case history ---
			
,#TempCaseCalc.cts_response_sec
,#TempCaseCalc.cts_callback_to_tsnotification				
,#TempCaseCalc.cts_symptom_to_needle_sec 				
,#TempCaseCalc.cts_last_known_well_to_needle_sec 						
,#TempCaseCalc.cts_bedside_response_sec 				
,#TempCaseCalc.cts_start_to_stamp_sec 				
,#TempCaseCalc.cts_start_to_accept_sec 				
,#TempCaseCalc.cts_arrival_to_stamp_sec 				
,#TempCaseCalc.cts_arrival_to_start_sec 					
,#TempCaseCalc.cts_cpoe_order_to_needle 						
,#TempCaseCalc.cts_login_to_handle_sec 			
,#TempCaseCalc.cts_verbal_order_to_cpoe_sec 			
,#TempCaseCalc.cts_verbal_order_to_needle_sec 				

,dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', GETUTCDATE())  as cts_created_date
,cas_created_by as cts_created_by

FROM #TempCaseCalc
END 

DROP Table #TempCaseCalc
END