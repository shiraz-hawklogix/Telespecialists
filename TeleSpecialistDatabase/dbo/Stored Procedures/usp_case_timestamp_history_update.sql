-- =============================================
-- Author:		<Adnan K>
-- Create date: <2020-March-02>
-- Description:	<Update TimeStamps table after case table is updated>
-- =============================================
CREATE PROCEDURE [dbo].[usp_case_timestamp_history_update] 
	-- Add the parameters for the stored procedure here	
	@cas_key INT	
AS
BEGIN
	DECLARE @TotalRows INT;

	SET NOCOUNT ON;

	
SELECT 
 [Extent1].[cas_key] AS [cas_key],   
 cas_created_by,
 cas_created_date,
 cas_modified_by,
 cas_modified_date,
    
 [Limit2].[cah_created_date] as cts_waiting_to_accept_est,
 [Limit2].[cah_created_date_utc] as cts_waiting_to_accept_utc,
 [Limit1].[cah_created_date] as cts_accepted_est,
 [Limit1].[cah_created_date_utc] as cts_accepted_utc,
 [dbo].[DiffSeconds]([Extent1].[cas_response_ts_notification], [Extent1].[cas_metric_stamp_time]) AS cts_start_to_stamp_sec, -- handle_time     
 [dbo].[DiffSeconds]([Limit2].[cah_created_date_utc], [Limit1].[cah_created_date_utc]) AS cts_waiting_to_accept_accept_sec, -- assignment_time_cmp 

    
 CASE WHEN (([Extent1].[cas_response_ts_notification] IS NOT NULL) AND ([Limit1].[cah_created_date_utc] IS NOT NULL)) THEN CASE WHEN ([Extent1].[cas_response_ts_notification] > [Limit1].[cah_created_date_utc]) THEN DATEDIFF (second, [Limit1].[cah_created_date_utc], [Extent1].[cas_response_ts_notification]) ELSE DATEDIFF (second, [Extent1].[cas_response_ts_notification], [Limit1].[cah_created_date_utc]) END ELSE 0 END AS cts_start_to_accept_sec -- start_accepted_cmp  

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




  
    WHERE (Extent1.cas_key = @cas_key)	 
	



Select @TotalRows = Count(*) FROM case_timestamp Where cts_cas_key = @cas_key;

select @TotalRows;

IF (@TotalRows > 0) 
BEGIN 
Update case_timestamp
set cts_waiting_to_accept_est = #TempCaseCalc.cts_waiting_to_accept_est,
cts_waiting_to_accept_utc = #TempCaseCalc.cts_waiting_to_accept_utc,
cts_accepted_est = #TempCaseCalc.cts_accepted_est,
cts_accepted_utc = #TempCaseCalc.cts_accepted_utc,
cts_start_to_stamp_sec = #TempCaseCalc.cts_start_to_stamp_sec,
cts_waiting_to_accept_accept_sec = #TempCaseCalc.cts_waiting_to_accept_accept_sec,
cts_modified_by = cas_modified_by,
cts_modified_date = cas_modified_date
FROM case_timestamp as ct
INNER JOIN #TempCaseCalc on ct.cts_cas_key = #TempCaseCalc.cas_key
END
ELSE 
BEGIN
INSERT INTO case_timestamp (
cts_cas_key,
cts_waiting_to_accept_est 					
,cts_waiting_to_accept_utc				
,cts_accepted_est 				
,cts_accepted_utc 						
,cts_start_to_stamp_sec 				
,cts_waiting_to_accept_accept_sec,
cts_created_date,
cts_created_by				

) 

SELECT 
cas_key as cts_cas_key,
cts_waiting_to_accept_est 					
,cts_waiting_to_accept_utc				
,cts_accepted_est 				
,cts_accepted_utc 						
,cts_start_to_stamp_sec 				
,cts_waiting_to_accept_accept_sec,
cas_created_date,
cas_created_by
FROM #TempCaseCalc
END 

DROP Table #TempCaseCalc
END